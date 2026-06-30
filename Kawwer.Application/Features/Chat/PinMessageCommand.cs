using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Chat;

/// <summary>Organizer pins a single message to the top of the match chat.</summary>
public sealed record PinMessageCommand(Guid OrganizerId, Guid MatchId, Guid MessageId) : IRequest<Unit>;

public sealed class PinMessageCommandHandler : IRequestHandler<PinMessageCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly IUnitOfWork _unitOfWork;

    public PinMessageCommandHandler(IMatchRepository matches, IChatRepository chat, IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(PinMessageCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can pin messages.");
        }

        var message = await _chat.GetByIdAsync(request.MessageId, cancellationToken)
                      ?? throw NotFoundException.For("Message", request.MessageId);

        if (message.MatchId != match.Id)
        {
            throw new ConflictException("The message does not belong to this match.");
        }

        match.PinMessage(message.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
