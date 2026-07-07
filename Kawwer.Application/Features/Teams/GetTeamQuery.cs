using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Teams;

namespace Kawwer.Application.Features.Teams;

public sealed record GetTeamQuery(Guid OwnerId, Guid TeamId) : IRequest<TeamDto>;

public sealed class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, TeamDto>
{
    private readonly ITeamRepository _teams;
    private readonly IUserRepository _users;

    public GetTeamQueryHandler(ITeamRepository teams, IUserRepository users)
    {
        _teams = teams;
        _users = users;
    }

    public async Task<TeamDto> HandleAsync(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await _teams.GetByIdAsync(request.TeamId, cancellationToken)
                   ?? throw NotFoundException.For("Team", request.TeamId);

        if (team.OwnerId != request.OwnerId)
        {
            throw new ForbiddenException("Only the team owner can view this team.");
        }

        var memberIds = team.Members.Select(m => m.UserId).ToList();
        var users = (await _users.GetByIdsAsync(memberIds, cancellationToken)).ToDictionary(u => u.Id);
        return team.ToDto(users);
    }
}
