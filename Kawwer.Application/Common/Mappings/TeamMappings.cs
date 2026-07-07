using Kawwer.Contracts.Teams;
using Kawwer.Contracts.Users;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Mappings;

public static class TeamMappings
{
    public static TeamDto ToDto(this Team team, IReadOnlyDictionary<Guid, User> users)
    {
        var members = team.Members
            .Where(m => users.ContainsKey(m.UserId))
            .Select(m => users[m.UserId].ToSummaryDto())
            .ToList();

        return new TeamDto(
            team.Id,
            team.Name,
            team.Description,
            team.Members.Count,
            team.CreatedAt,
            members);
    }
}
