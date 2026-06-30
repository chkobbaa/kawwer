using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Payments;

public sealed record FinishPaymentCollectionCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class FinishPaymentCollectionCommandHandler : IRequestHandler<FinishPaymentCollectionCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public FinishPaymentCollectionCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(FinishPaymentCollectionCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can finish collection.");
        }

        match.FinishPaymentCollection();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.PaymentUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
