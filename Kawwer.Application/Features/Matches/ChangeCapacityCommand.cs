using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Matches;

public sealed record ChangeCapacityCommand(Guid OrganizerId, Guid MatchId, int MaxPlayers) : IRequest<Unit>;

public sealed class ChangeCapacityCommandHandler : IRequestHandler<ChangeCapacityCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeCapacityCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(ChangeCapacityCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can change the capacity.");
        }

        match.ChangeMaxPlayers(request.MaxPlayers);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.WaitingListUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
