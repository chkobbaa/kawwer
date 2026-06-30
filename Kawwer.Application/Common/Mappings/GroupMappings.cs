using Kawwer.Contracts.Groups;
using Kawwer.Contracts.Users;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Mappings;

public static class GroupMappings
{
    public static GroupDto ToDto(this Group group, IReadOnlyDictionary<Guid, User> users)
    {
        var members = group.Members
            .Where(m => users.ContainsKey(m.UserId))
            .Select(m => users[m.UserId].ToSummaryDto())
            .ToList();

        return new GroupDto(
            group.Id,
            group.Name,
            group.Description,
            group.Members.Count,
            group.CreatedAt,
            members);
    }
}
