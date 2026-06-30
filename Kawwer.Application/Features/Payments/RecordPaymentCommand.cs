using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Payments;

/// <summary>Organizer records a cash payment (full or partial) from a player.</summary>
public sealed record RecordPaymentCommand(Guid OrganizerId, Guid MatchId, Guid PayerId, decimal Amount) : IRequest<Unit>;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
    }
}

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IPaymentRepository _payments;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandHandler(
        IMatchRepository matches,
        IPaymentRepository payments,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _payments = payments;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can record payments.");
        }

        match.RecordPayment(request.PayerId, request.Amount);
        _payments.Add(new PaymentRecord(match.Id, request.PayerId, request.OrganizerId, request.Amount));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.PaymentUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
