using FluentValidation;
using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Chat;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Chat;

public sealed record SendMessageCommand(Guid SenderId, Guid MatchId, string Content) : IRequest<ChatMessageDto>;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ChatMessageDto>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly IUserRepository _users;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        IUserRepository users,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _users = users;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatMessageDto> HandleAsync(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.Status is MatchStatus.Cancelled)
        {
            throw new ConflictException("The chat is read-only for a cancelled match.");
        }

        var isOrganizer = match.OrganizerId == request.SenderId;
        if (!isOrganizer)
        {
            var participant = match.Participants.FirstOrDefault(p => p.UserId == request.SenderId)
                              ?? throw new ForbiddenException("You are not a participant of this match.");

            if (participant.Status != ParticipantStatus.Accepted)
            {
                throw new ForbiddenException("Only accepted players can send messages. Waiting-list players have read-only access.");
            }
        }

        var message = ChatMessage.CreateUserMessage(match.Id, request.SenderId, request.Content);
        _chat.Add(message);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sender = await _users.GetByIdAsync(request.SenderId, cancellationToken);
        var dto = message.ToDto(sender?.FullName, isPinned: match.PinnedMessageId == message.Id);
        await _realtime.ChatMessagePostedAsync(match.Id, dto, cancellationToken);
        return dto;
    }
}
