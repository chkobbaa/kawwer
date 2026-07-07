using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Lineup;

/// <summary>Removes a guest from a match. The organizer or the person who added the guest may remove it.</summary>
public sealed record RemoveGuestPlayerCommand(
    Guid RequesterId,
    Guid MatchId,
    Guid GuestId) : IRequest<Unit>;

public sealed class RemoveGuestPlayerCommandHandler : IRequestHandler<RemoveGuestPlayerCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveGuestPlayerCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RemoveGuestPlayerCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var guest = match.GetGuest(request.GuestId);
        if (match.OrganizerId != request.RequesterId && guest.AddedByUserId != request.RequesterId)
        {
            throw new ForbiddenException("Only the organizer or the person who added the guest can remove it.");
        }

        match.RemoveGuest(request.GuestId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
