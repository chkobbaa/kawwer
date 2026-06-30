using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.FootballFields;

public sealed record UpdateFootballFieldCommand(
    Guid UserId,
    Guid FieldId,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    int Capacity,
    int MatchDurationMinutes,
    decimal Price,
    decimal ReservationFee,
    SurfaceType Surface,
    bool Indoor,
    bool Parking,
    bool Shower,
    bool Lights,
    string? PhoneNumber,
    string? GoogleMapsUrl,
    string? Notes) : IRequest<Unit>;

public sealed class UpdateFootballFieldCommandValidator : AbstractValidator<UpdateFootballFieldCommand>
{
    public UpdateFootballFieldCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(2);
        RuleFor(x => x.MatchDurationMinutes).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReservationFee).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateFootballFieldCommandHandler : IRequestHandler<UpdateFootballFieldCommand, Unit>
{
    private readonly IFootballFieldRepository _fields;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFootballFieldCommandHandler(IFootballFieldRepository fields, IUnitOfWork unitOfWork)
    {
        _fields = fields;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateFootballFieldCommand request, CancellationToken cancellationToken)
    {
        var field = await _fields.GetByIdAsync(request.FieldId, cancellationToken)
                    ?? throw NotFoundException.For("Football field", request.FieldId);

        if (field.CreatedBy != request.UserId)
        {
            throw new ForbiddenException("Only the field owner can edit this field.");
        }

        field.Update(
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.Capacity,
            request.MatchDurationMinutes,
            request.Price,
            request.ReservationFee,
            request.Surface,
            request.Indoor,
            request.Parking,
            request.Shower,
            request.Lights,
            request.PhoneNumber,
            request.GoogleMapsUrl,
            request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
