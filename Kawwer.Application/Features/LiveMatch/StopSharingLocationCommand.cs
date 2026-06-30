using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.LiveMatch;

public sealed record StopSharingLocationCommand(Guid UserId, Guid MatchId) : IRequest<Unit>;

public sealed class StopSharingLocationCommandHandler : IRequestHandler<StopSharingLocationCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IUnitOfWork _unitOfWork;

    public StopSharingLocationCommandHandler(IMatchRepository matches, IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(StopSharingLocationCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        match.GetParticipant(request.UserId).StopSharingLocation();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
