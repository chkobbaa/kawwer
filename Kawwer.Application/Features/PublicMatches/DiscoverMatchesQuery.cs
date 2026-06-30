using Kawwer.Application.Common;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Common;
using Kawwer.Contracts.PublicMatches;

namespace Kawwer.Application.Features.PublicMatches;

public sealed record DiscoverMatchesQuery(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    decimal? Latitude,
    decimal? Longitude,
    double? RadiusKm,
    int Page,
    int PageSize) : IRequest<PagedResult<DiscoverMatchDto>>;

public sealed class DiscoverMatchesQueryHandler : IRequestHandler<DiscoverMatchesQuery, PagedResult<DiscoverMatchDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IFootballFieldRepository _fields;
    private readonly IUserRepository _users;

    public DiscoverMatchesQueryHandler(IMatchRepository matches, IFootballFieldRepository fields, IUserRepository users)
    {
        _matches = matches;
        _fields = fields;
        _users = users;
    }

    public async Task<PagedResult<DiscoverMatchDto>> HandleAsync(DiscoverMatchesQuery request, CancellationToken cancellationToken)
    {
        // Pull a generous page from storage, then apply distance filtering/sorting in memory.
        var (matches, _) = await _matches.GetPublicAsync(request.DateFrom, request.DateTo, 1, 500, cancellationToken);

        var fieldIds = matches.Select(m => m.FootballFieldId).Distinct().ToList();
        var organizerIds = matches.Select(m => m.OrganizerId).Distinct().ToList();
        var organizers = (await _users.GetByIdsAsync(organizerIds, cancellationToken)).ToDictionary(u => u.Id);

        var items = new List<(DiscoverMatchDto Dto, double? Distance)>();
        var fieldCache = new Dictionary<Guid, Domain.Entities.FootballField>();

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

            if (!organizers.TryGetValue(match.OrganizerId, out var organizer))
            {
                continue;
            }

            double? distance = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                distance = GeoUtils.DistanceKm(request.Latitude.Value, request.Longitude.Value, field.Latitude, field.Longitude);
                if (request.RadiusKm.HasValue && distance > request.RadiusKm.Value)
                {
                    continue;
                }
            }

            var dto = new DiscoverMatchDto(
                match.Id,
                match.Title,
                match.MatchDate,
                match.StartTime,
                match.DurationMinutes,
                match.MaxPlayers,
                match.AcceptedCount,
                Math.Max(match.SpotsForInvitees - match.AcceptedCount, 0),
                field.Name,
                field.Address,
                field.Latitude,
                field.Longitude,
                field.Indoor,
                field.Surface,
                distance.HasValue ? Math.Round(distance.Value, 1) : null,
                organizer.ToSummaryDto());

            items.Add((dto, distance));
        }

        var ordered = items
            .OrderBy(x => x.Distance ?? double.MaxValue)
            .ThenBy(x => x.Dto.MatchDate)
            .ThenBy(x => x.Dto.StartTime)
            .Select(x => x.Dto)
            .ToList();

        var total = ordered.Count;
        var page = ordered
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<DiscoverMatchDto>(page, request.Page, request.PageSize, total);
    }
}
