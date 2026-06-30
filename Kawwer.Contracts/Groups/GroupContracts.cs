using Kawwer.Contracts.Users;

namespace Kawwer.Contracts.Groups;

public sealed record CreateGroupRequest(string Name, string? Description);

public sealed record UpdateGroupRequest(string Name, string? Description);

public sealed record AddGroupMemberRequest(Guid UserId);

public sealed record GroupDto(
    Guid Id,
    string Name,
    string? Description,
    int MemberCount,
    DateTime CreatedAt,
    IReadOnlyList<UserSummaryDto> Members);
