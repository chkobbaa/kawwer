using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Chat;

public sealed record EditMessageCommand(Guid UserId, Guid MessageId, string Content) : IRequest<Unit>;

public sealed class EditMessageCommandValidator : AbstractValidator<EditMessageCommand>
{
    public EditMessageCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public sealed class EditMessageCommandHandler : IRequestHandler<EditMessageCommand, Unit>
{
    private readonly IChatRepository _chat;
    private readonly IUnitOfWork _unitOfWork;

    public EditMessageCommandHandler(IChatRepository chat, IUnitOfWork unitOfWork)
    {
        _chat = chat;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(EditMessageCommand request, CancellationToken cancellationToken)
    {
        var message = await _chat.GetByIdAsync(request.MessageId, cancellationToken)
                      ?? throw NotFoundException.For("Message", request.MessageId);

        message.Edit(request.UserId, request.Content);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
