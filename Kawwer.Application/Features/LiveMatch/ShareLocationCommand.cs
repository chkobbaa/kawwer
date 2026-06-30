using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.LiveMatch;

/// <summary>A participant shares (or refreshes) their live location. Only accepted players may share.</summary>
public sealed record ShareLocationCommand(Guid UserId, Guid MatchId, decimal Latitude, decimal Longitude) : IRequest<Unit>;

public sealed class ShareLocationCommandHandler : IRequestHandler<ShareLocationCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public ShareLocationCommandHandler(
        IMatchRepository matches,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(ShareLocationCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var participant = match.GetParticipant(request.UserId);
        if (participant.Status != ParticipantStatus.Accepted)
        {
            throw new ForbiddenException("Only accepted players can share their location.");
        }

        participant.ShareLocation(request.Latitude, request.Longitude);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
