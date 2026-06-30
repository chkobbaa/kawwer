using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A football game organized by one user at a football field. Aggregate root that owns
/// its participants and the waiting list, and enforces the match lifecycle and money split.
/// </summary>
public class Match : AggregateRoot
{
    private readonly List<MatchParticipant> _participants = new();

    private Match()
    {
        Title = string.Empty;
    }

    public Match(
        Guid organizerId,
        Guid footballFieldId,
        string title,
        DateOnly matchDate,
        TimeOnly startTime,
        int durationMinutes,
        int maxPlayers,
        decimal totalFieldPrice,
        decimal reservationPaid,
        MatchVisibility visibility,
        string? description = null,
        bool autoAcceptPublic = false)
    {
        if (maxPlayers < 2)
        {
            throw new DomainException("A match must allow at least two players.");
        }

        Id = Guid.NewGuid();
        OrganizerId = organizerId;
        FootballFieldId = footballFieldId;
        Title = title;
        Description = description;
        MatchDate = matchDate;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        EndTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));
        MaxPlayers = maxPlayers;
        TotalFieldPrice = totalFieldPrice;
        ReservationPaid = reservationPaid;
        Visibility = visibility;
        AutoAcceptPublic = autoAcceptPublic;
        Status = MatchStatus.Draft;
        PaymentCollectionStarted = false;
        PaymentCompleted = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid OrganizerId { get; private set; }
    public Guid FootballFieldId { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public MatchVisibility Visibility { get; private set; }
    public MatchStatus Status { get; private set; }
    public DateOnly MatchDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public int MaxPlayers { get; private set; }
    public decimal TotalFieldPrice { get; private set; }
    public decimal ReservationPaid { get; private set; }
    public bool AutoAcceptPublic { get; private set; }
    public bool PaymentCollectionStarted { get; private set; }
    public bool PaymentCompleted { get; private set; }
    public bool LiveMatchStarted { get; private set; }
    public Guid? PinnedMessageId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<MatchParticipant> Participants => _participants.AsReadOnly();

    /// <summary>Kickoff as a UTC datetime, treating stored values as UTC.</summary>
    public DateTime KickoffUtc => MatchDate.ToDateTime(StartTime, DateTimeKind.Utc);

    /// <summary>Spots available to invited players (the organizer occupies one slot).</summary>
    public int SpotsForInvitees => Math.Max(MaxPlayers - 1, 0);

    public int AcceptedCount => _participants.Count(p => p.Status == ParticipantStatus.Accepted);

    public int WaitingCount => _participants.Count(p => p.Status == ParticipantStatus.WaitingList);

    public bool IsFull => AcceptedCount >= SpotsForInvitees;

    /// <summary>Amount still to collect for the field, after the reservation already paid.</summary>
    public decimal RemainingAmount => Math.Max(TotalFieldPrice - ReservationPaid, 0m);

    /// <summary>
    /// Per-player share rounded up to the nearest TND. The organizer absorbs the rounding remainder.
    /// </summary>
    public decimal SharePerPlayer
    {
        get
        {
            var payers = AcceptedCount;
            if (payers <= 0)
            {
                return 0m;
            }

            return Math.Ceiling(RemainingAmount / payers);
        }
    }

    public decimal CollectedAmount => _participants.Sum(p => p.PaidAmount);

    public decimal MissingAmount => Math.Max(RemainingAmount - CollectedAmount, 0m);

    // ----- Lifecycle -----

    public void Publish()
    {
        if (Status != MatchStatus.Draft)
        {
            throw new DomainException("Only a draft match can be published.");
        }

        Status = MatchStatus.Published;
        Touch();
    }

    public void Edit(DateOnly matchDate, TimeOnly startTime, int durationMinutes, string? description, MatchVisibility visibility)
    {
        EnsureNotClosed();
        MatchDate = matchDate;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        EndTime = startTime.Add(TimeSpan.FromMinutes(durationMinutes));
        Description = description;
        Visibility = visibility;
        Touch();
    }

    public void ChangeMaxPlayers(int maxPlayers)
    {
        EnsureNotClosed();
        if (maxPlayers - 1 < AcceptedCount)
        {
            throw new DomainException("Capacity cannot be reduced below the number of accepted players.");
        }

        MaxPlayers = maxPlayers;
        // Increasing capacity may free spots; promote waiting players.
        FillOpenSpotsFromWaitingList();
        RecalculateStatus();
        Touch();
    }

    public void Cancel()
    {
        if (Status is MatchStatus.Finished or MatchStatus.Cancelled)
        {
            throw new DomainException("A finished or already cancelled match cannot be cancelled.");
        }

        Status = MatchStatus.Cancelled;
        Touch();
    }

    public void StartPlaying()
    {
        if (Status is not (MatchStatus.Published or MatchStatus.Full))
        {
            throw new DomainException("Only a published or full match can start playing.");
        }

        Status = MatchStatus.Playing;
        Touch();
    }

    public void Finish()
    {
        if (Status is not (MatchStatus.Playing or MatchStatus.Published or MatchStatus.Full))
        {
            throw new DomainException("Only an active match can be finished.");
        }

        Status = MatchStatus.Finished;
        Touch();
    }

    public void StartLiveMatch()
    {
        EnsureNotClosed();
        LiveMatchStarted = true;
        Touch();
    }

    // ----- Participants & invitations -----

    public MatchParticipant Invite(Guid userId)
    {
        if (userId == OrganizerId)
        {
            throw new DomainException("The organizer cannot be invited to their own match.");
        }

        if (_participants.Any(p => p.UserId == userId && p.Status != ParticipantStatus.Removed))
        {
            throw new DomainException("The player has already been invited to this match.");
        }

        var participant = new MatchParticipant(Id, userId);
        _participants.Add(participant);
        return participant;
    }

    /// <summary>
    /// A player requests to join a public match. When auto-accept is enabled the request is
    /// resolved immediately (joining the match or the waiting list); otherwise it awaits approval.
    /// </summary>
    public MatchParticipant RequestToJoin(Guid userId)
    {
        if (Visibility != MatchVisibility.Public)
        {
            throw new DomainException("Only public matches accept join requests.");
        }

        if (Status is MatchStatus.Cancelled or MatchStatus.Finished)
        {
            throw new DomainException("This match is no longer open to join requests.");
        }

        if (userId == OrganizerId)
        {
            throw new DomainException("The organizer already owns this match.");
        }

        if (_participants.Any(p => p.UserId == userId && p.Status != ParticipantStatus.Removed))
        {
            throw new DomainException("A join request or invitation already exists for this player.");
        }

        var participant = new MatchParticipant(Id, userId, isJoinRequest: true);
        _participants.Add(participant);

        if (AutoAcceptPublic)
        {
            Accept(userId);
        }

        return participant;
    }

    /// <summary>Organizer approves a pending join request, joining the player or the waiting list.</summary>
    public bool ApproveJoinRequest(Guid userId) => Accept(userId);

    /// <summary>Organizer rejects a pending join request.</summary>
    public void RejectJoinRequest(Guid userId)
    {
        var participant = GetParticipant(userId);
        participant.Decline();
        RecalculateStatus();
    }

    public MatchParticipant GetParticipant(Guid userId)
    {
        return _participants.FirstOrDefault(p => p.UserId == userId)
               ?? throw new DomainException("The user is not a participant of this match.");
    }

    /// <summary>
    /// A player accepts. They join directly if a spot is free, otherwise they go to the waiting list.
    /// Returns true if accepted into the match, false if placed on the waiting list.
    /// </summary>
    public bool Accept(Guid userId)
    {
        var participant = GetParticipant(userId);

        if (IsFull)
        {
            participant.PlaceOnWaitingList(NextWaitingPosition());
            return false;
        }

        participant.Accept();
        RecalculateStatus();
        return true;
    }

    public void Decline(Guid userId)
    {
        var participant = GetParticipant(userId);
        var wasAccepted = participant.Status == ParticipantStatus.Accepted;
        participant.Decline();

        if (wasAccepted)
        {
            FillOpenSpotsFromWaitingList();
        }

        RecalculateStatus();
    }

    public void MarkThinking(Guid userId) => GetParticipant(userId).MarkThinking();

    public void MarkSeen(Guid userId) => GetParticipant(userId).MarkSeen();

    /// <summary>
    /// A player leaves. If they were accepted, the first waiting player is promoted.
    /// Returns the promoted participant, if any.
    /// </summary>
    public MatchParticipant? Leave(Guid userId)
    {
        var participant = GetParticipant(userId);
        var wasAccepted = participant.Status == ParticipantStatus.Accepted;
        participant.Leave();

        MatchParticipant? promoted = null;
        if (wasAccepted)
        {
            promoted = PromoteNextWaiting();
        }

        RecalculateWaitingPositions();
        RecalculateStatus();
        return promoted;
    }

    private MatchParticipant? PromoteNextWaiting()
    {
        var next = _participants
            .Where(p => p.Status == ParticipantStatus.WaitingList)
            .OrderBy(p => p.WaitingListPosition)
            .ThenBy(p => p.UserId)
            .FirstOrDefault();

        next?.PromoteFromWaitingList();
        return next;
    }

    private void FillOpenSpotsFromWaitingList()
    {
        while (!IsFull)
        {
            var promoted = PromoteNextWaiting();
            if (promoted is null)
            {
                break;
            }
        }

        RecalculateWaitingPositions();
    }

    private int NextWaitingPosition()
    {
        var current = _participants
            .Where(p => p.Status == ParticipantStatus.WaitingList)
            .Select(p => p.WaitingListPosition ?? 0)
            .DefaultIfEmpty(0)
            .Max();

        return current + 1;
    }

    private void RecalculateWaitingPositions()
    {
        var waiting = _participants
            .Where(p => p.Status == ParticipantStatus.WaitingList)
            .OrderBy(p => p.WaitingListPosition)
            .ThenBy(p => p.UserId)
            .ToList();

        for (var i = 0; i < waiting.Count; i++)
        {
            waiting[i].SetWaitingPosition(i + 1);
        }
    }

    private void RecalculateStatus()
    {
        if (Status is MatchStatus.Draft or MatchStatus.Cancelled or MatchStatus.Finished or MatchStatus.Playing)
        {
            return;
        }

        Status = IsFull ? MatchStatus.Full : MatchStatus.Published;
        Touch();
    }

    // ----- Payments -----

    public void StartPaymentCollection()
    {
        EnsureNotClosed();
        PaymentCollectionStarted = true;
        Touch();
    }

    public void RecordPayment(Guid userId, decimal amount)
    {
        if (!PaymentCollectionStarted)
        {
            throw new DomainException("Payment collection has not been started.");
        }

        if (PaymentCompleted)
        {
            throw new DomainException("Payment collection is already completed and is read-only.");
        }

        GetParticipant(userId).RecordPayment(amount, SharePerPlayer);
        Touch();
    }

    public void UndoPayment(Guid userId)
    {
        if (PaymentCompleted)
        {
            throw new DomainException("Payment collection is already completed and is read-only.");
        }

        GetParticipant(userId).ResetPayment();
        Touch();
    }

    public void FinishPaymentCollection()
    {
        if (MissingAmount > 0m)
        {
            throw new DomainException("Collection can only finish when the remaining amount reaches zero.");
        }

        PaymentCompleted = true;
        Touch();
    }

    // ----- Chat -----

    public void PinMessage(Guid messageId)
    {
        PinnedMessageId = messageId;
        Touch();
    }

    private void EnsureNotClosed()
    {
        if (Status is MatchStatus.Cancelled or MatchStatus.Finished)
        {
            throw new DomainException("The match is closed and can no longer be modified.");
        }
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}
