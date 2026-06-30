using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Common;
using Kawwer.Contracts.FootballFields;

namespace Kawwer.Application.Features.FootballFields;

public sealed record SearchFootballFieldsQuery(string? Term, int Page, int PageSize)
    : IRequest<PagedResult<FootballFieldDto>>;

public sealed class SearchFootballFieldsQueryHandler
    : IRequestHandler<SearchFootballFieldsQuery, PagedResult<FootballFieldDto>>
{
    private readonly IFootballFieldRepository _fields;

    public SearchFootballFieldsQueryHandler(IFootballFieldRepository fields) => _fields = fields;

    public async Task<PagedResult<FootballFieldDto>> HandleAsync(SearchFootballFieldsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _fields.SearchAsync(request.Term, request.Page, request.PageSize, cancellationToken);
        return new PagedResult<FootballFieldDto>(
            items.Select(f => f.ToDto()).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
