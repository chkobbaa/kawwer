using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.FootballFields;

namespace Kawwer.Application.Features.FootballFields;

public sealed record GetFootballFieldQuery(Guid FieldId) : IRequest<FootballFieldDto>;

public sealed class GetFootballFieldQueryHandler : IRequestHandler<GetFootballFieldQuery, FootballFieldDto>
{
    private readonly IFootballFieldRepository _fields;

    public GetFootballFieldQueryHandler(IFootballFieldRepository fields) => _fields = fields;

    public async Task<FootballFieldDto> HandleAsync(GetFootballFieldQuery request, CancellationToken cancellationToken)
    {
        var field = await _fields.GetByIdAsync(request.FieldId, cancellationToken)
                    ?? throw NotFoundException.For("Football field", request.FieldId);
        return field.ToDto();
    }
}
