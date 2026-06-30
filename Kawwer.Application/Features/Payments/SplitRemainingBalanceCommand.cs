using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Payments;

/// <summary>
/// Distributes the still-missing amount equally across the players who have not yet fully paid,
/// recording the extra as cash so collection can complete.
/// </summary>
public sealed record SplitRemainingBalanceCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class SplitRemainingBalanceCommandHandler : IRequestHandler<SplitRemainingBalanceCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IPaymentRepository _payments;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public SplitRemainingBalanceCommandHandler(
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

    public async Task<Unit> HandleAsync(SplitRemainingBalanceCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can split the balance.");
        }

        var share = match.SharePerPlayer;
        var unpaid = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted && p.PaidAmount < share)
            .ToList();

        if (unpaid.Count == 0 || match.MissingAmount <= 0m)
        {
            return Unit.Value;
        }

        var remaining = match.MissingAmount;
        var perPlayer = Math.Ceiling(remaining / unpaid.Count);

        foreach (var participant in unpaid)
        {
            if (match.MissingAmount <= 0m)
            {
                break;
            }

            var amount = Math.Min(perPlayer, match.MissingAmount);
            match.RecordPayment(participant.UserId, amount);
            _payments.Add(new PaymentRecord(match.Id, participant.UserId, request.OrganizerId, amount));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.PaymentUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
