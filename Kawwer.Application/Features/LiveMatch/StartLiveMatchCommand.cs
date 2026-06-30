using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.LiveMatch;

public sealed record StartLiveMatchCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class StartLiveMatchCommandHandler : IRequestHandler<StartLiveMatchCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public StartLiveMatchCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        INotificationService notifications,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _notifications = notifications;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(StartLiveMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can start Live Match.");
        }

        match.StartLiveMatch();
        _chat.Add(ChatMessage.CreateSystemMessage(match.Id, "Live Match has started."));

        var accepted = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .Select(p => p.UserId);

        await _notifications.NotifyManyAsync(
            accepted,
            NotificationCategory.LiveMatch,
            "Live Match started",
            $"\"{match.Title}\" is now live. See attendance and directions.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
