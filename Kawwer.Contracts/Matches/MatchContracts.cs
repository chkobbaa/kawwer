using Kawwer.Contracts.FootballFields;
using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Matches;

public sealed record CreateMatchRequest(
    Guid FootballFieldId,
    string Title,
    string? Description,
    DateOnly MatchDate,
    TimeOnly StartTime,
    int? MaxPlayers,
    decimal? TotalFieldPrice,
    MatchVisibility Visibility,
    bool AutoAcceptPublic,
    IReadOnlyList<Guid> InvitedUserIds,
    IReadOnlyList<Guid> InvitedTeamIds,
    MatchFormat Format = MatchFormat.Pickup,
    string? OpponentName = null,
    Guid? OpponentTeamId = null,
    SportType Sport = SportType.Football);

public sealed record UpdateMatchRequest(
    DateOnly MatchDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Description,
    MatchVisibility Visibility);

/// <summary>Moves a match to a new date/time, keeping everything else. Notifies the whole roster.</summary>
public sealed record RescheduleMatchRequest(
    DateOnly MatchDate,
    TimeOnly StartTime);

public sealed record ChangeCapacityRequest(int MaxPlayers);

public sealed record MatchDto(
    Guid Id,
    Guid OrganizerId,
    string Title,
    string? Description,
    MatchVisibility Visibility,
    MatchStatus Status,
    MatchFormat Format,
    SportType Sport,
    string? OpponentName,
    Guid? OpponentTeamId,
    DateOnly MatchDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes,
    int MaxPlayers,
    int AcceptedCount,
    int WaitingCount,
    decimal TotalFieldPrice,
    decimal ReservationPaid,
    decimal RemainingAmount,
    decimal SharePerPlayer,
    decimal CollectedAmount,
    decimal MissingAmount,
    bool PaymentCollectionStarted,
    bool PaymentCompleted,
    bool LiveMatchStarted,
    FootballFieldDto Field,
    UserSummaryDto Organizer,
    DateTime CreatedAt);

/// <summary>Row in the organizer dashboard.</summary>
public sealed record OrganizerDashboardItemDto(
    Guid MatchId,
    string Title,
    DateOnly MatchDate,
    TimeOnly StartTime,
    string FieldName,
    int AcceptedCount,
    int WaitingCount,
    int ThinkingCount,
    int DeclinedCount,
    decimal MoneyRemaining,
    MatchStatus Status);
