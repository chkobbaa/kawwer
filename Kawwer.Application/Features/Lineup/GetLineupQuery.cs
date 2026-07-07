using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Lineup;

/// <summary>Reads the tactical lineup board for a match: organizer, accepted players and guests.</summary>
public sealed record GetLineupQuery(Guid RequesterId, Guid MatchId) : IRequest<LineupDto>;

public sealed class GetLineupQueryHandler : IRequestHandler<GetLineupQuery, LineupDto>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;

    public GetLineupQueryHandler(IMatchRepository matches, IUserRepository users)
    {
        _matches = matches;
        _users = users;
    }

    public async Task<LineupDto> HandleAsync(GetLineupQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var isMemberOrOrganizer = match.OrganizerId == request.RequesterId
                                  || match.Participants.Any(p => p.UserId == request.RequesterId);
        if (!isMemberOrOrganizer)
        {
            throw new ForbiddenException("Only the organizer or a player in the match can view the lineup.");
        }

        // Load the organizer alongside the accepted players; guests carry their own display data.
        var userIds = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .Select(p => p.UserId)
            .Append(match.OrganizerId)
            .Distinct()
            .ToList();

        var users = (await _users.GetByIdsAsync(userIds, cancellationToken)).ToDictionary(u => u.Id);
        return match.ToLineupDto(users);
    }
}
