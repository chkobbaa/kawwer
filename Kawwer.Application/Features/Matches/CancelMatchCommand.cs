using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record CancelMatchCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class CancelMatchCommandHandler : IRequestHandler<CancelMatchCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly INotificationService _notifications;
    private readonly INotificationRepository _notificationRepository;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public CancelMatchCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        INotificationService notifications,
        INotificationRepository notificationRepository,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _notifications = notifications;
        _notificationRepository = notificationRepository;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(CancelMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can cancel this match.");
        }

        match.Cancel();
        _chat.Add(ChatMessage.CreateSystemMessage(match.Id, "The match has been cancelled by the organizer."));

        // Invitations to a cancelled match can no longer be accepted or declined;
        // remove them so no stale Accept/Decline buttons linger in anyone's inbox.
        await _notificationRepository.RemoveForMatchAsync(match.Id, NotificationCategory.Invitation, cancellationToken);

        var affected = match.Participants
            .Where(p => p.Status is ParticipantStatus.Accepted or ParticipantStatus.WaitingList or ParticipantStatus.Thinking or ParticipantStatus.Invited)
            .Select(p => p.UserId);

        await _notifications.NotifyManyAsync(
            affected,
            NotificationCategory.Match,
            "Match cancelled",
            $"\"{match.Title}\" on {match.MatchDate:dd MMM} has been cancelled.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
