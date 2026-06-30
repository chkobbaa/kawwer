using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;

namespace Kawwer.Application.Features.Matches;

/// <summary>Matches the user is involved in (organizing or accepted) that are still upcoming.</summary>
public sealed record GetUpcomingMatchesQuery(Guid UserId) : IRequest<IReadOnlyList<MatchDto>>;

public sealed class GetUpcomingMatchesQueryHandler : IRequestHandler<GetUpcomingMatchesQuery, IReadOnlyList<MatchDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IFootballFieldRepository _fields;
    private readonly IUserRepository _users;

    public GetUpcomingMatchesQueryHandler(IMatchRepository matches, IFootballFieldRepository fields, IUserRepository users)
    {
        _matches = matches;
        _fields = fields;
        _users = users;
    }

    public async Task<IReadOnlyList<MatchDto>> HandleAsync(GetUpcomingMatchesQuery request, CancellationToken cancellationToken)
    {
        var matches = await _matches.GetUpcomingForUserAsync(request.UserId, cancellationToken);

        var fieldCache = new Dictionary<Guid, Domain.Entities.FootballField>();
        var userCache = new Dictionary<Guid, Domain.Entities.User>();
        var result = new List<MatchDto>();

        foreach (var match in matches)
        {
            if (!fieldCache.TryGetValue(match.FootballFieldId, out var field))
            {
                field = await _fields.GetByIdAsync(match.FootballFieldId, cancellationToken);
                if (field is null)
                {
                    continue;
                }

                fieldCache[match.FootballFieldId] = field;
            }

            if (!userCache.TryGetValue(match.OrganizerId, out var organizer))
            {
                organizer = await _users.GetByIdAsync(match.OrganizerId, cancellationToken);
                if (organizer is null)
                {
                    continue;
                }

                userCache[match.OrganizerId] = organizer;
            }

            result.Add(match.ToDto(field, organizer));
        }

        return result;
    }
}
