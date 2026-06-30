using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Features.PublicMatches;

/// <summary>Organizer approves or rejects a pending public join request.</summary>
public sealed record RespondToJoinRequestCommand(Guid OrganizerId, Guid MatchId, Guid UserId, bool Approve) : IRequest<Unit>;

public sealed class RespondToJoinRequestCommandHandler : IRequestHandler<RespondToJoinRequestCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public RespondToJoinRequestCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        IUserRepository users,
        INotificationService notifications,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _users = users;
        _notifications = notifications;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RespondToJoinRequestCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can respond to join requests.");
        }

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        var name = user?.FullName ?? "A player";

        if (request.Approve)
        {
            var joined = match.ApproveJoinRequest(request.UserId);
            _chat.Add(ChatMessage.CreateSystemMessage(match.Id, $"{name} joined the match."));
            await _notifications.NotifyAsync(
                request.UserId,
                Domain.Enums.NotificationCategory.Match,
                joined ? "Join request approved" : "Added to the waiting list",
                joined
                    ? $"You've been approved for \"{match.Title}\"."
                    : $"\"{match.Title}\" is full — you're on the waiting list.",
                match.Id,
                cancellationToken);
        }
        else
        {
            match.RejectJoinRequest(request.UserId);
            await _notifications.NotifyAsync(
                request.UserId,
                Domain.Enums.NotificationCategory.Match,
                "Join request declined",
                $"Your request to join \"{match.Title}\" was declined.",
                match.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
