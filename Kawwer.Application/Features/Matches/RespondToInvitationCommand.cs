using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

/// <summary>A player accepts or declines their invitation. Accepting joins the match or the waiting list.</summary>
public sealed record RespondToInvitationCommand(Guid UserId, Guid MatchId, bool Accept) : IRequest<bool>;

public sealed class RespondToInvitationCommandHandler : IRequestHandler<RespondToInvitationCommand, bool>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IDateTimeProvider _clock;
    private readonly IUnitOfWork _unitOfWork;

    public RespondToInvitationCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        IUserRepository users,
        INotificationService notifications,
        IRealtimeNotifier realtime,
        IDateTimeProvider clock,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _users = users;
        _notifications = notifications;
        _realtime = realtime;
        _clock = clock;
        _unitOfWork = unitOfWork;
    }

    /// <returns>True if the player joined the match, false if placed on the waiting list or declined.</returns>
    public async Task<bool> HandleAsync(RespondToInvitationCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        // Defensive expiry: even if the periodic sweep hasn't run yet, a match whose scheduled end
        // has already passed must not accept (or notify anyone about) a late response.
        if (match.TryExpire(_clock.UtcNow, _clock.AppTimeZone))
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        if (match.Status is MatchStatus.Cancelled or MatchStatus.Finished or MatchStatus.Expired)
        {
            throw new ConflictException("This match is no longer accepting responses.");
        }

        // Ensure the caller is actually invited.
        _ = match.GetParticipant(request.UserId);
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        var name = user?.FullName ?? "A player";

        if (!request.Accept)
        {
            match.Decline(request.UserId);
            await _notifications.NotifyAsync(
                match.OrganizerId,
                NotificationCategory.Invitation,
                "Invitation declined",
                $"{name} declined your invitation to \"{match.Title}\".",
                match.Id,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
            return false;
        }

        var joined = match.Accept(request.UserId);
        if (joined)
        {
            _chat.Add(ChatMessage.CreateSystemMessage(match.Id, $"{name} joined the match."));
            await _notifications.NotifyAsync(
                match.OrganizerId,
                NotificationCategory.Invitation,
                "Player accepted",
                $"{name} accepted your invitation to \"{match.Title}\".",
                match.Id,
                cancellationToken);
        }
        else
        {
            var participant = match.GetParticipant(request.UserId);
            await _notifications.NotifyAsync(
                request.UserId,
                NotificationCategory.WaitingList,
                "Added to the waiting list",
                $"\"{match.Title}\" is full. You're number {participant.WaitingListPosition} on the waiting list.",
                match.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        await _realtime.WaitingListUpdatedAsync(match.Id, cancellationToken);
        return joined;
    }
}
