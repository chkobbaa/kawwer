using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Payments;

public sealed record UndoPaymentCommand(Guid OrganizerId, Guid MatchId, Guid PayerId) : IRequest<Unit>;

public sealed class UndoPaymentCommandHandler : IRequestHandler<UndoPaymentCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public UndoPaymentCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UndoPaymentCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can undo payments.");
        }

        match.UndoPayment(request.PayerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.PaymentUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
