using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Payments;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Payments;

public sealed record GetPaymentSummaryQuery(Guid MatchId) : IRequest<PaymentSummaryDto>;

public sealed class GetPaymentSummaryQueryHandler : IRequestHandler<GetPaymentSummaryQuery, PaymentSummaryDto>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;

    public GetPaymentSummaryQueryHandler(IMatchRepository matches, IUserRepository users)
    {
        _matches = matches;
        _users = users;
    }

    public async Task<PaymentSummaryDto> HandleAsync(GetPaymentSummaryQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var accepted = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .ToList();

        var users = (await _users.GetByIdsAsync(accepted.Select(p => p.UserId).ToList(), cancellationToken))
            .ToDictionary(u => u.Id);

        var share = match.SharePerPlayer;
        var players = accepted
            .Where(p => users.ContainsKey(p.UserId))
            .Select(p => new PaymentPlayerDto(
                p.UserId,
                users[p.UserId].ToSummaryDto(),
                p.PaidAmount,
                share,
                p.PaymentStatus))
            .ToList();

        return new PaymentSummaryDto(
            match.Id,
            match.TotalFieldPrice,
            match.ReservationPaid,
            match.RemainingAmount,
            match.CollectedAmount,
            match.MissingAmount,
            share,
            players.Count(p => p.Status == PaymentStatus.Paid),
            players.Count(p => p.Status == PaymentStatus.PartiallyPaid),
            players.Count(p => p.Status == PaymentStatus.NotPaid),
            match.PaymentCollectionStarted,
            match.PaymentCompleted,
            players);
    }
}
