using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record LeaveMatchCommand(Guid UserId, Guid MatchId) : IRequest<Unit>;

public sealed class LeaveMatchCommandHandler : IRequestHandler<LeaveMatchCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveMatchCommandHandler(
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

    public async Task<Unit> HandleAsync(LeaveMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        var name = user?.FullName ?? "A player";

        var promoted = match.Leave(request.UserId);
        _chat.Add(ChatMessage.CreateSystemMessage(match.Id, $"{name} left the match."));

        await _notifications.NotifyAsync(
            match.OrganizerId,
            NotificationCategory.Match,
            "Player left",
            $"{name} left \"{match.Title}\".",
            match.Id,
            cancellationToken);

        if (promoted is not null)
        {
            var promotedUser = await _users.GetByIdAsync(promoted.UserId, cancellationToken);
            _chat.Add(ChatMessage.CreateSystemMessage(match.Id, $"{promotedUser?.FullName ?? "A player"} was promoted from the waiting list."));

            await _notifications.NotifyAsync(
                promoted.UserId,
                NotificationCategory.WaitingList,
                "You're in!",
                $"A spot opened up and you've been promoted into \"{match.Title}\".",
                match.Id,
                cancellationToken);

            await _notifications.NotifyAsync(
                match.OrganizerId,
                NotificationCategory.WaitingList,
                "Waiting list promotion",
                $"{promotedUser?.FullName ?? "A player"} was promoted into \"{match.Title}\".",
                match.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        await _realtime.WaitingListUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
