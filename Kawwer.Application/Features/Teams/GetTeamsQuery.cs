using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Teams;

namespace Kawwer.Application.Features.Teams;

public sealed record GetTeamsQuery(Guid OwnerId) : IRequest<IReadOnlyList<TeamDto>>;

public sealed class GetTeamsQueryHandler : IRequestHandler<GetTeamsQuery, IReadOnlyList<TeamDto>>
{
    private readonly ITeamRepository _teams;
    private readonly IUserRepository _users;

    public GetTeamsQueryHandler(ITeamRepository teams, IUserRepository users)
    {
        _teams = teams;
        _users = users;
    }

    public async Task<IReadOnlyList<TeamDto>> HandleAsync(GetTeamsQuery request, CancellationToken cancellationToken)
    {
        var teams = await _teams.GetForOwnerAsync(request.OwnerId, cancellationToken);

        var memberIds = teams.SelectMany(t => t.Members.Select(m => m.UserId)).Distinct().ToList();
        var users = (await _users.GetByIdsAsync(memberIds, cancellationToken)).ToDictionary(u => u.Id);

        return teams.Select(t => t.ToDto(users)).ToList();
    }
}
