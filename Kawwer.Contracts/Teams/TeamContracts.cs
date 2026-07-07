using Kawwer.Contracts.Users;

namespace Kawwer.Contracts.Teams;

public sealed record CreateTeamRequest(string Name, string? Description);

public sealed record UpdateTeamRequest(string Name, string? Description);

public sealed record AddTeamMemberRequest(Guid UserId);

public sealed record TeamDto(
    Guid Id,
    string Name,
    string? Description,
    int MemberCount,
    DateTime CreatedAt,
    IReadOnlyList<UserSummaryDto> Members);
