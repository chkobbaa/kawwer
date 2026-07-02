using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;

namespace Kawwer.Application.Features.Matches;

public sealed record GetOrganizerDashboardQuery(Guid OrganizerId) : IRequest<IReadOnlyList<OrganizerDashboardItemDto>>;

public sealed class GetOrganizerDashboardQueryHandler
    : IRequestHandler<GetOrganizerDashboardQuery, IReadOnlyList<OrganizerDashboardItemDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IFootballFieldRepository _fields;

    public GetOrganizerDashboardQueryHandler(IMatchRepository matches, IFootballFieldRepository fields)
    {
        _matches = matches;
        _fields = fields;
    }

    public async Task<IReadOnlyList<OrganizerDashboardItemDto>> HandleAsync(GetOrganizerDashboardQuery request, CancellationToken cancellationToken)
    {
        var matches = await _matches.GetForOrganizerAsync(request.OrganizerId, cancellationToken);

        var result = new List<OrganizerDashboardItemDto>();
        var fieldNames = new Dictionary<Guid, string>();

        foreach (var match in matches)
        {
            // The dashboard lists matches that still need the organizer's attention;
            // cancelled and finished matches are history and must not appear here.
            if (match.Status is Domain.Enums.MatchStatus.Cancelled or Domain.Enums.MatchStatus.Finished)
            {
                continue;
            }

            if (!fieldNames.TryGetValue(match.FootballFieldId, out var fieldName))
            {
                var field = await _fields.GetByIdAsync(match.FootballFieldId, cancellationToken);
                fieldName = field?.Name ?? "Unknown field";
                fieldNames[match.FootballFieldId] = fieldName;
            }

            result.Add(match.ToDashboardItem(fieldName));
        }

        return result;
    }
}
