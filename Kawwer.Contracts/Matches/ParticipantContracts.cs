using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Matches;

public sealed record InvitePlayersRequest(
    IReadOnlyList<Guid> UserIds,
    IReadOnlyList<Guid> GroupIds);

public sealed record RespondToInvitationRequest(bool Accept);

public sealed record UpdateAttendanceRequest(Guid UserId, AttendanceStatus Attendance);

public sealed record ShareLocationRequest(decimal Latitude, decimal Longitude);

public sealed record MatchParticipantDto(
    Guid Id,
    UserSummaryDto User,
    ParticipantStatus Status,
    bool IsJoinRequest,
    int? WaitingListPosition,
    decimal PaidAmount,
    PaymentStatus PaymentStatus,
    AttendanceStatus Attendance,
    bool SharedLocation,
    decimal? Latitude,
    decimal? Longitude,
    DateTime? RespondedAt,
    DateTime InvitedAt);

public sealed record WaitingListPositionDto(
    Guid MatchId,
    int Position,
    int TotalWaiting,
    int AcceptedCount);
