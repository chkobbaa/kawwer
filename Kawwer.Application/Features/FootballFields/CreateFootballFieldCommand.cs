using FluentValidation;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.FootballFields;

public sealed record CreateFootballFieldCommand(
    Guid CreatedBy,
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
    string? Notes) : IRequest<Guid>;

public sealed class CreateFootballFieldCommandValidator : AbstractValidator<CreateFootballFieldCommand>
{
    public CreateFootballFieldCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m);
        RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(2);
        RuleFor(x => x.MatchDurationMinutes).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReservationFee).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateFootballFieldCommandHandler : IRequestHandler<CreateFootballFieldCommand, Guid>
{
    private readonly IFootballFieldRepository _fields;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFootballFieldCommandHandler(IFootballFieldRepository fields, IUnitOfWork unitOfWork)
    {
        _fields = fields;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(CreateFootballFieldCommand request, CancellationToken cancellationToken)
    {
        var field = new FootballField(
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
            request.CreatedBy,
            request.PhoneNumber,
            request.GoogleMapsUrl,
            request.Notes);

        _fields.Add(field);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return field.Id;
    }
}
