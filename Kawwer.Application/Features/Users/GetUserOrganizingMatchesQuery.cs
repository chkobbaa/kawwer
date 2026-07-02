using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Users;

/// <summary>
/// Upcoming matches a user is currently organizing, shown on their public profile.
/// Only friends of the user (or the user themself) may see this list; everyone else
/// receives an empty list. Private matches are never exposed to other users.
/// </summary>
public sealed record GetUserOrganizingMatchesQuery(Guid ViewerId, Guid UserId) : IRequest<IReadOnlyList<MatchDto>>;

public sealed class GetUserOrganizingMatchesQueryHandler
    : IRequestHandler<GetUserOrganizingMatchesQuery, IReadOnlyList<MatchDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IFootballFieldRepository _fields;
    private readonly IUserRepository _users;
    private readonly IFriendshipRepository _friendships;

    public GetUserOrganizingMatchesQueryHandler(
        IMatchRepository matches,
        IFootballFieldRepository fields,
        IUserRepository users,
        IFriendshipRepository friendships)
    {
        _matches = matches;
        _fields = fields;
        _users = users;
        _friendships = friendships;
    }

    public async Task<IReadOnlyList<MatchDto>> HandleAsync(GetUserOrganizingMatchesQuery request, CancellationToken cancellationToken)
    {
        var isSelf = request.ViewerId == request.UserId;
        if (!isSelf && !await _friendships.AreFriendsAsync(request.ViewerId, request.UserId, cancellationToken))
        {
            return Array.Empty<MatchDto>();
        }

        var organizer = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (organizer is null)
        {
            return Array.Empty<MatchDto>();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var matches = (await _matches.GetForOrganizerAsync(request.UserId, cancellationToken))
            .Where(m => m.MatchDate >= today)
            .Where(m => m.Status is MatchStatus.Published or MatchStatus.Full)
            .Where(m => isSelf || m.Visibility != MatchVisibility.Private)
            .OrderBy(m => m.MatchDate)
            .ThenBy(m => m.StartTime)
            .ToList();

        var fieldCache = new Dictionary<Guid, Domain.Entities.FootballField>();
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

            result.Add(match.ToDto(field, organizer));
        }

        return result;
    }
}
