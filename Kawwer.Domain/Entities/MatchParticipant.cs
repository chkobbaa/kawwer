using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// The relationship between a user and a match. A single record carries the player's
/// whole lifecycle: invitation, response, waiting list, payment, attendance, location and rating.
/// </summary>
public class MatchParticipant : Entity
{
    private MatchParticipant()
    {
    }

    public MatchParticipant(Guid matchId, Guid userId, bool isJoinRequest = false)
    {
        Id = Guid.NewGuid();
        MatchId = matchId;
        UserId = userId;
        Status = ParticipantStatus.Invited;
        IsJoinRequest = isJoinRequest;
        InvitedAt = DateTime.UtcNow;
        PaidAmount = 0m;
        PaymentCompleted = false;
        Attendance = AttendanceStatus.Unknown;
        SharedLocation = false;
        RatedOrganizer = false;
        RatedPlayers = false;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid MatchId { get; private set; }
    public Guid UserId { get; private set; }
    public ParticipantStatus Status { get; private set; }

    /// <summary>True when the record originated from a public-match join request rather than an organizer invite.</summary>
    public bool IsJoinRequest { get; private set; }
    public int? WaitingListPosition { get; private set; }
    public DateTime InvitedAt { get; private set; }
    public DateTime? SeenAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public DateTime? JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public decimal PaidAmount { get; private set; }
    public bool PaymentCompleted { get; private set; }
    public AttendanceStatus Attendance { get; private set; }
    public bool SharedLocation { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public DateTime? LocationUpdatedAt { get; private set; }
    public bool RatedOrganizer { get; private set; }
    public bool RatedPlayers { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public PaymentStatus PaymentStatus => PaymentCompleted
        ? PaymentStatus.Paid
        : PaidAmount > 0m
            ? PaymentStatus.PartiallyPaid
            : PaymentStatus.NotPaid;

    public void MarkSeen()
    {
        if (Status == ParticipantStatus.Invited)
        {
            Status = ParticipantStatus.Seen;
        }

        SeenAt ??= DateTime.UtcNow;
    }

    public void MarkThinking()
    {
        if (Status is ParticipantStatus.Invited or ParticipantStatus.Seen)
        {
            Status = ParticipantStatus.Thinking;
            SeenAt ??= DateTime.UtcNow;
        }
    }

    public void Accept()
    {
        Status = ParticipantStatus.Accepted;
        WaitingListPosition = null;
        RespondedAt = DateTime.UtcNow;
        JoinedAt = DateTime.UtcNow;
        LeftAt = null;
    }

    public void Decline()
    {
        Status = ParticipantStatus.Declined;
        WaitingListPosition = null;
        RespondedAt = DateTime.UtcNow;
    }

    public void PlaceOnWaitingList(int position)
    {
        Status = ParticipantStatus.WaitingList;
        WaitingListPosition = position;
        RespondedAt = DateTime.UtcNow;
    }

    public void SetWaitingPosition(int position)
    {
        if (Status == ParticipantStatus.WaitingList)
        {
            WaitingListPosition = position;
        }
    }

    public void PromoteFromWaitingList()
    {
        if (Status != ParticipantStatus.WaitingList)
        {
            throw new DomainException("Only waiting-list players can be promoted.");
        }

        Status = ParticipantStatus.Accepted;
        WaitingListPosition = null;
        JoinedAt = DateTime.UtcNow;
    }

    public void Leave()
    {
        Status = ParticipantStatus.Cancelled;
        WaitingListPosition = null;
        LeftAt = DateTime.UtcNow;
    }

    public void Remove()
    {
        Status = ParticipantStatus.Removed;
        WaitingListPosition = null;
        LeftAt = DateTime.UtcNow;
    }

    public void RecordPayment(decimal amount, decimal share)
    {
        if (amount < 0m)
        {
            throw new DomainException("Payment amount cannot be negative.");
        }

        PaidAmount += amount;
        PaymentCompleted = PaidAmount >= share && share > 0m;
    }

    /// <summary>Marks the participant as fully paid for the given share.</summary>
    public void MarkFullyPaid(decimal share)
    {
        PaidAmount = share;
        PaymentCompleted = true;
    }

    public void ResetPayment()
    {
        PaidAmount = 0m;
        PaymentCompleted = false;
    }

    public void SetAttendance(AttendanceStatus attendance) => Attendance = attendance;

    public void ShareLocation(decimal latitude, decimal longitude)
    {
        SharedLocation = true;
        Latitude = latitude;
        Longitude = longitude;
        LocationUpdatedAt = DateTime.UtcNow;
    }

    public void StopSharingLocation()
    {
        SharedLocation = false;
        Latitude = null;
        Longitude = null;
    }

    public void MarkRatedOrganizer() => RatedOrganizer = true;

    public void MarkRatedPlayers() => RatedPlayers = true;
}
