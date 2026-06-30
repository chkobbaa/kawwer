using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Chat;

public sealed record DeleteMessageCommand(Guid UserId, Guid MessageId) : IRequest<Unit>;

public sealed class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, Unit>
{
    private readonly IChatRepository _chat;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMessageCommandHandler(IChatRepository chat, IUnitOfWork unitOfWork)
    {
        _chat = chat;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _chat.GetByIdAsync(request.MessageId, cancellationToken)
                      ?? throw NotFoundException.For("Message", request.MessageId);

        message.Delete(request.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
