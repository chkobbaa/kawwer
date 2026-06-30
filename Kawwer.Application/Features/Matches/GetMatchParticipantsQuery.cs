using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;

namespace Kawwer.Application.Features.Matches;

public sealed record GetMatchParticipantsQuery(Guid MatchId) : IRequest<IReadOnlyList<MatchParticipantDto>>;

public sealed class GetMatchParticipantsQueryHandler
    : IRequestHandler<GetMatchParticipantsQuery, IReadOnlyList<MatchParticipantDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;

    public GetMatchParticipantsQueryHandler(IMatchRepository matches, IUserRepository users)
    {
        _matches = matches;
        _users = users;
    }

    public async Task<IReadOnlyList<MatchParticipantDto>> HandleAsync(GetMatchParticipantsQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var userIds = match.Participants.Select(p => p.UserId).ToList();
        var users = (await _users.GetByIdsAsync(userIds, cancellationToken)).ToDictionary(u => u.Id);

        return match.Participants
            .Where(p => users.ContainsKey(p.UserId))
            .OrderBy(p => p.WaitingListPosition ?? 0)
            .Select(p => p.ToDto(users[p.UserId]))
            .ToList();
    }
}
