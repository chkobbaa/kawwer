using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Statistics;

public sealed record PlayerStatisticsDto(
    Guid UserId,
    int MatchesPlayed,
    int MatchesOrganized,
    int InvitationsReceived,
    int InvitationsAccepted,
    int InvitationsDeclined,
    int PublicMatchesJoined,
    double AttendanceRate,
    int OnTimeArrivals,
    int LateArrivals,
    int NoShows,
    int LateCancellations,
    double PaymentReliability,
    int PaymentsCompleted,
    decimal AverageRating,
    decimal OrganizerRating,
    decimal PlayerRating,
    decimal Reputation,
    ReliabilityBadge ReliabilityBadge,
    int Friends,
    int TeamsCreated);

public sealed record OrganizerStatisticsDto(
    Guid UserId,
    int MatchesOrganized,
    int MatchesCompleted,
    int MatchesCancelled,
    double AverageAttendance,
    decimal AveragePlayerRating,
    decimal AverageOrganizerRating);
