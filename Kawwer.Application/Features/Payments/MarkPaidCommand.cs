using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.Payments;

/// <summary>Marks a player as fully paid for their share, recording the outstanding difference as cash.</summary>
public sealed record MarkPaidCommand(Guid OrganizerId, Guid MatchId, Guid PayerId) : IRequest<Unit>;

public sealed class MarkPaidCommandHandler : IRequestHandler<MarkPaidCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IPaymentRepository _payments;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public MarkPaidCommandHandler(
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

    public async Task<Unit> HandleAsync(MarkPaidCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can record payments.");
        }

        var participant = match.GetParticipant(request.PayerId);
        var outstanding = match.SharePerPlayer - participant.PaidAmount;
        if (outstanding > 0m)
        {
            match.RecordPayment(request.PayerId, outstanding);
            _payments.Add(new PaymentRecord(match.Id, request.PayerId, request.OrganizerId, outstanding));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.PaymentUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
