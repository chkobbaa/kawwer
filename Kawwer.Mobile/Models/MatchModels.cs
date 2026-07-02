namespace Kawwer.Mobile.Models;

public sealed class MatchDto
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MatchVisibility Visibility { get; set; }
    public MatchStatus Status { get; set; }
    public DateOnly MatchDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxPlayers { get; set; }
    public int AcceptedCount { get; set; }
    public int WaitingCount { get; set; }
    public decimal TotalFieldPrice { get; set; }
    public decimal ReservationPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal SharePerPlayer { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal MissingAmount { get; set; }
    public bool PaymentCollectionStarted { get; set; }
    public bool PaymentCompleted { get; set; }
    public bool LiveMatchStarted { get; set; }
    public FootballFieldDto Field { get; set; } = new();
    public UserSummaryDto Organizer { get; set; } = new();

    public string WhenLabel => $"{MatchDate:ddd dd MMM} · {StartTime:HH\\:mm}";

    /// <summary>Player counter including the organizer, e.g. a fresh 14-player match shows 1/14.</summary>
    public string PlayersLabel => $"{AcceptedCount + 1}/{MaxPlayers} players";
    public string DayLabel => MatchDate.ToString("dd");
    public string MonthLabel => MatchDate.ToString("MMM").ToUpperInvariant();
    public string TimeLabel => StartTime.ToString("HH\\:mm");
}

public sealed class MatchParticipantDto
{
    public Guid Id { get; set; }
    public UserSummaryDto User { get; set; } = new();
    public ParticipantStatus Status { get; set; }
    public bool IsJoinRequest { get; set; }
    public int? WaitingListPosition { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public AttendanceStatus Attendance { get; set; }
    public bool SharedLocation { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }

    public string StatusLabel => Status switch
    {
        ParticipantStatus.Accepted => "In",
        ParticipantStatus.WaitingList => WaitingListPosition is { } p ? $"Waiting #{p}" : "Waiting",
        ParticipantStatus.Thinking => "Thinking",
        ParticipantStatus.Invited or ParticipantStatus.Seen when IsJoinRequest => "Wants in",
        ParticipantStatus.Invited => "Invited",
        ParticipantStatus.Seen => "Seen",
        ParticipantStatus.Declined => "Declined",
        _ => Status.ToString()
    };

    /// <summary>A public-match join request still waiting for the organizer's decision.</summary>
    public bool IsPendingJoinRequest =>
        IsJoinRequest && Status is ParticipantStatus.Invited or ParticipantStatus.Seen or ParticipantStatus.Thinking;

    /// <summary>Set by MatchDetailsViewModel: true when the viewer is the organizer and this row is a pending join request.</summary>
    public bool CanRespondToJoinRequest { get; set; }
}

public sealed class OrganizerDashboardItemDto
{
    public Guid MatchId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly MatchDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public int AcceptedCount { get; set; }
    public int WaitingCount { get; set; }
    public int ThinkingCount { get; set; }
    public int DeclinedCount { get; set; }
    public decimal MoneyRemaining { get; set; }
    public MatchStatus Status { get; set; }

    public string WhenLabel => $"{MatchDate:ddd dd MMM} · {StartTime:HH\\:mm}";
    public string DayLabel => MatchDate.ToString("dd");
    public string MonthLabel => MatchDate.ToString("MMM").ToUpperInvariant();
}

public sealed class DiscoverMatchDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly MatchDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public int DurationMinutes { get; set; }
    public int MaxPlayers { get; set; }
    public int AcceptedCount { get; set; }
    public int AvailableSpots { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldAddress { get; set; } = string.Empty;
    public bool Indoor { get; set; }
    public SurfaceType Surface { get; set; }
    public double? DistanceKm { get; set; }
    public bool IsJoined { get; set; }
    public UserSummaryDto Organizer { get; set; } = new();

    public string WhenLabel => $"{MatchDate:ddd dd MMM} · {StartTime:HH\\:mm}";
    public string DistanceLabel => DistanceKm is null ? FieldName : $"{FieldName} · {DistanceKm:0.0} km";
    public string DayLabel => MatchDate.ToString("dd");
    public string MonthLabel => MatchDate.ToString("MMM").ToUpperInvariant();

    /// <summary>How full the match is, 0..1. Drives the capacity bar on Discover cards.</summary>
    public double FillRatio
    {
        get
        {
            var total = AcceptedCount + AvailableSpots;
            return total <= 0 ? 0 : Math.Clamp((double)AcceptedCount / total, 0, 1);
        }
    }
}

public sealed class WaitingListPositionDto
{
    public Guid MatchId { get; set; }
    public int Position { get; set; }
    public int TotalWaiting { get; set; }
    public int AcceptedCount { get; set; }
}

public sealed class JoinMatchRequestDto
{
    public Guid MatchId { get; set; }
    public UserSummaryDto User { get; set; } = new();
    public DateTime RequestedAt { get; set; }
}

public sealed class PaymentPlayerDto
{
    public Guid UserId { get; set; }
    public UserSummaryDto User { get; set; } = new();
    public decimal PaidAmount { get; set; }
    public decimal Share { get; set; }
    public PaymentStatus Status { get; set; }

    public string StatusLabel => Status switch
    {
        PaymentStatus.Paid => "Paid",
        PaymentStatus.PartiallyPaid => "Partial",
        _ => "Unpaid"
    };
}

public sealed class PaymentSummaryDto
{
    public Guid MatchId { get; set; }
    public decimal TotalFieldPrice { get; set; }
    public decimal ReservationPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal MissingAmount { get; set; }
    public decimal SharePerPlayer { get; set; }
    public int PaidPlayers { get; set; }
    public int PartialPlayers { get; set; }
    public int UnpaidPlayers { get; set; }
    public bool CollectionStarted { get; set; }
    public bool CollectionCompleted { get; set; }
    public List<PaymentPlayerDto> Players { get; set; } = new();

    /// <summary>Collection progress, 0..1. Drives the progress bar on the Payments hero.</summary>
    public double CollectionProgress
    {
        get
        {
            var target = CollectedAmount + MissingAmount;
            return target <= 0 ? 0 : Math.Clamp((double)(CollectedAmount / target), 0, 1);
        }
    }
}

public sealed class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid? SenderId { get; set; }
    public string? SenderName { get; set; }
    public int Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
    public bool IsPinned { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsSystem => Type == 2;

    /// <summary>Set by the ChatViewModel after load; aligns the bubble to the right.</summary>
    public bool IsMine { get; set; }
}

public sealed class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? RelatedMatchId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// True for a match invitation addressed to the viewer, which gets inline
    /// Accept/Decline buttons. Matches the exact title produced by the API.
    /// </summary>
    public bool IsMatchInvitation =>
        Category == NotificationCategory.Invitation
        && RelatedMatchId is not null
        && Title.Equals("New match invitation", StringComparison.OrdinalIgnoreCase);

    public string Icon => Category switch
    {
        NotificationCategory.Match => "⚽",
        NotificationCategory.Invitation => "✉️",
        NotificationCategory.Payment => "💰",
        NotificationCategory.LiveMatch => "📍",
        NotificationCategory.Friend => "🤝",
        NotificationCategory.Group => "👥",
        NotificationCategory.WaitingList => "⏳",
        _ => "🔔"
    };
}

public sealed class PlayerStatisticsDto
{
    public int MatchesPlayed { get; set; }
    public int MatchesOrganized { get; set; }
    public int InvitationsAccepted { get; set; }
    public int InvitationsDeclined { get; set; }
    public double AttendanceRate { get; set; }
    public int NoShows { get; set; }
    public double PaymentReliability { get; set; }
    public decimal AverageRating { get; set; }
    public decimal Reputation { get; set; }
    public ReliabilityBadge ReliabilityBadge { get; set; }
    public int Friends { get; set; }
    public int GroupsCreated { get; set; }
}
