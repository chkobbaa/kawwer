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
    private readonly List<GuestPlayer> _guests = new();

    /// <summary>Upper bound on guests, a light guard against runaway input at the system boundary.</summary>
    private const int MaxGuests = 40;

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

    // ----- Lineup: the organizer is a player too, but is modelled implicitly (no participant row),
    // so their board slot lives directly on the match. -----
    public TeamSide OrganizerTeam { get; private set; }
    public double? OrganizerPositionX { get; private set; }
    public double? OrganizerPositionY { get; private set; }

    public IReadOnlyCollection<MatchParticipant> Participants => _participants.AsReadOnly();

    public IReadOnlyCollection<GuestPlayer> Guests => _guests.AsReadOnly();

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
    /// Per-player share rounded up to the nearest TND. The remaining amount (after the
    /// reservation already paid) is the WHOLE amount left for the field, and every player
    /// in the match (organizer included) pays an equal share of it.
    /// Example: 90 TND total, 5 TND reserved, 14 players => 85 / 14 => 7 TND each.
    /// </summary>
    public decimal SharePerPlayer
    {
        get
        {
            var payers = MaxPlayers;
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

        var existing = _participants.FirstOrDefault(p => p.UserId == userId);
        if (existing is not null)
        {
            // Declined, departed or removed players can be invited again; anyone with an
            // active invitation or spot cannot receive a duplicate.
            if (existing.Status is ParticipantStatus.Declined or ParticipantStatus.Cancelled or ParticipantStatus.Removed)
            {
                existing.Reinvite();
                return existing;
            }

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
        if (Visibility == MatchVisibility.Private)
        {
            throw new DomainException("This match is invitation-only and does not accept join requests.");
        }

        if (Status is MatchStatus.Cancelled or MatchStatus.Finished)
        {
            throw new DomainException("This match is no longer open to join requests.");
        }

        if (userId == OrganizerId)
        {
            throw new DomainException("The organizer already owns this match.");
        }

        var existing = _participants.FirstOrDefault(p => p.UserId == userId);
        MatchParticipant participant;
        if (existing is not null)
        {
            switch (existing.Status)
            {
                // Already in (or queued): joining again is a no-op, not an error, so a
                // retried request after a network hiccup never strands the player.
                case ParticipantStatus.Accepted or ParticipantStatus.WaitingList:
                    return existing;

                // A pending join request stays pending; repeating the tap is idempotent.
                case ParticipantStatus.Invited or ParticipantStatus.Seen or ParticipantStatus.Thinking when existing.IsJoinRequest:
                    participant = existing;
                    break;

                // Tapping "Join" while holding an open invitation simply accepts it.
                case ParticipantStatus.Invited or ParticipantStatus.Seen or ParticipantStatus.Thinking:
                    Accept(userId);
                    return existing;

                default:
                    // A player who declined or left earlier may change their mind and join again.
                    existing.Reinvite(asJoinRequest: true);
                    participant = existing;
                    break;
            }
        }
        else
        {
            participant = new MatchParticipant(Id, userId, isJoinRequest: true);
            _participants.Add(participant);
        }

        if (AutoAcceptPublic)
        {
            Accept(userId);
        }

        return participant;
    }

    /// <summary>
    /// A member of the match suggests adding a player. The suggestion lands in the organizer's
    /// pending list (as a join request) and only becomes a real spot once the organizer approves.
    /// </summary>
    public MatchParticipant Suggest(Guid userId)
    {
        EnsureNotClosed();

        if (userId == OrganizerId)
        {
            throw new DomainException("The organizer already owns this match.");
        }

        var existing = _participants.FirstOrDefault(p => p.UserId == userId);
        if (existing is not null)
        {
            if (existing.Status is not (ParticipantStatus.Declined or ParticipantStatus.Cancelled or ParticipantStatus.Removed))
            {
                throw new DomainException("The player is already linked to this match.");
            }

            existing.Reinvite(asJoinRequest: true);
            return existing;
        }

        var participant = new MatchParticipant(Id, userId, isJoinRequest: true);
        _participants.Add(participant);
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
        // The organizer's own share is paid directly to the field (it is never collected
        // through the app), so collection can also finish once every player in the match
        // has settled their individual share.
        var everyPlayerPaid = _participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .All(p => p.PaymentCompleted);

        if (MissingAmount > 0m && !everyPlayerPaid)
        {
            throw new DomainException("Collection can only finish when every player has paid their share.");
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

    // ----- Guests & tactical lineup -----

    /// <summary>
    /// Adds a guest player (someone without the app) to the match by name. Guests appear in the
    /// roster and on the lineup board but hold no account, payment or attendance state.
    /// </summary>
    public GuestPlayer AddGuest(string name, Guid addedByUserId, int? skillLevel = null)
    {
        EnsureNotClosed();

        if (_guests.Count >= MaxGuests)
        {
            throw new DomainException($"A match cannot have more than {MaxGuests} guest players.");
        }

        var guest = new GuestPlayer(Id, name, addedByUserId, skillLevel);
        _guests.Add(guest);
        Touch();
        return guest;
    }

    public GuestPlayer GetGuest(Guid guestId)
        => _guests.FirstOrDefault(g => g.Id == guestId)
           ?? throw new DomainException("The guest is not part of this match.");

    public void RemoveGuest(Guid guestId)
    {
        var guest = GetGuest(guestId);
        _guests.Remove(guest);
        Touch();
    }

    /// <summary>Places an accepted player on a lineup team at a normalized board position.</summary>
    public void PlaceParticipantInLineup(Guid userId, TeamSide team, double positionX, double positionY)
    {
        var participant = GetParticipant(userId);
        if (participant.Status != ParticipantStatus.Accepted)
        {
            throw new DomainException("Only accepted players can be placed on the lineup board.");
        }

        participant.PlaceInLineup(team, positionX, positionY);
        Touch();
    }

    public void PlaceGuestInLineup(Guid guestId, TeamSide team, double positionX, double positionY)
    {
        GetGuest(guestId).PlaceInLineup(team, positionX, positionY);
        Touch();
    }

    /// <summary>Places the organizer (an implicit player) on a lineup team, clamping coordinates to 0..1.</summary>
    public void PlaceOrganizerInLineup(TeamSide team, double positionX, double positionY)
    {
        OrganizerTeam = team;
        OrganizerPositionX = Math.Clamp(positionX, 0d, 1d);
        OrganizerPositionY = Math.Clamp(positionY, 0d, 1d);
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
