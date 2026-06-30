using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.PublicMatches;

/// <summary>A player requests to join a public match. Auto-accept resolves it immediately.</summary>
public sealed record RequestToJoinMatchCommand(Guid UserId, Guid MatchId) : IRequest<bool>;

public sealed class RequestToJoinMatchCommandHandler : IRequestHandler<RequestToJoinMatchCommand, bool>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public RequestToJoinMatchCommandHandler(
        IMatchRepository matches,
        IUserRepository users,
        INotificationService notifications,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _users = users;
        _notifications = notifications;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    /// <returns>True if accepted immediately (auto-accept), false if the request awaits approval.</returns>
    public async Task<bool> HandleAsync(RequestToJoinMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var participant = match.RequestToJoin(request.UserId);
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        var name = user?.FullName ?? "A player";
        var accepted = participant.Status == ParticipantStatus.Accepted;

        if (accepted)
        {
            await _notifications.NotifyAsync(
                request.UserId,
                NotificationCategory.Match,
                "You're in!",
                $"You've joined \"{match.Title}\".",
                match.Id,
                cancellationToken);
        }
        else
        {
            await _notifications.NotifyAsync(
                match.OrganizerId,
                NotificationCategory.Invitation,
                "New join request",
                $"{name} wants to join \"{match.Title}\".",
                match.Id,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return accepted;
    }
}
