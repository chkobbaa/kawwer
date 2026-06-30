using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.PublicMatches;

public sealed record DiscoverMatchDto(
    Guid Id,
    string Title,
    DateOnly MatchDate,
    TimeOnly StartTime,
    int DurationMinutes,
    int MaxPlayers,
    int AcceptedCount,
    int AvailableSpots,
    string FieldName,
    string FieldAddress,
    decimal Latitude,
    decimal Longitude,
    bool Indoor,
    SurfaceType Surface,
    double? DistanceKm,
    UserSummaryDto Organizer);

public sealed record JoinMatchRequestDto(
    Guid MatchId,
    UserSummaryDto User,
    DateTime RequestedAt);

public sealed record ApproveJoinRequest(Guid UserId);
