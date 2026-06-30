using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Matches;

/// <summary>Marks an invitation as seen or "thinking" for the calling player.</summary>
public sealed record MarkInvitationStatusCommand(Guid UserId, Guid MatchId, bool Thinking) : IRequest<Unit>;

public sealed class MarkInvitationStatusCommandHandler : IRequestHandler<MarkInvitationStatusCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IUnitOfWork _unitOfWork;

    public MarkInvitationStatusCommandHandler(IMatchRepository matches, IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(MarkInvitationStatusCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (request.Thinking)
        {
            match.MarkThinking(request.UserId);
        }
        else
        {
            match.MarkSeen(request.UserId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
