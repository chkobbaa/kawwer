using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record GetWaitingListPositionQuery(Guid UserId, Guid MatchId) : IRequest<WaitingListPositionDto>;

public sealed class GetWaitingListPositionQueryHandler
    : IRequestHandler<GetWaitingListPositionQuery, WaitingListPositionDto>
{
    private readonly IMatchRepository _matches;

    public GetWaitingListPositionQueryHandler(IMatchRepository matches) => _matches = matches;

    public async Task<WaitingListPositionDto> HandleAsync(GetWaitingListPositionQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var participant = match.GetParticipant(request.UserId);
        if (participant.Status != ParticipantStatus.WaitingList)
        {
            throw new ConflictException("You are not currently on the waiting list for this match.");
        }

        return new WaitingListPositionDto(
            match.Id,
            participant.WaitingListPosition ?? 0,
            match.WaitingCount,
            match.AcceptedCount);
    }
}
