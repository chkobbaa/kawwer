using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;

namespace Kawwer.Application.Features.Matches;

public sealed record GetMatchQuery(Guid MatchId) : IRequest<MatchDto>;

public sealed class GetMatchQueryHandler : IRequestHandler<GetMatchQuery, MatchDto>
{
    private readonly IMatchRepository _matches;
    private readonly IFootballFieldRepository _fields;
    private readonly IUserRepository _users;

    public GetMatchQueryHandler(IMatchRepository matches, IFootballFieldRepository fields, IUserRepository users)
    {
        _matches = matches;
        _fields = fields;
        _users = users;
    }

    public async Task<MatchDto> HandleAsync(GetMatchQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var field = await _fields.GetByIdAsync(match.FootballFieldId, cancellationToken)
                    ?? throw NotFoundException.For("Football field", match.FootballFieldId);
        var organizer = await _users.GetByIdAsync(match.OrganizerId, cancellationToken)
                        ?? throw NotFoundException.For("User", match.OrganizerId);

        return match.ToDto(field, organizer);
    }
}
