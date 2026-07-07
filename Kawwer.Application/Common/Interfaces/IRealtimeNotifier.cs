using Kawwer.Contracts.Chat;
using Kawwer.Contracts.Realtime;

namespace Kawwer.Application.Common.Interfaces;

/// <summary>
/// Pushes real-time updates to connected clients (SignalR). Two scopes exist:
/// <list type="bullet">
/// <item>Match-scoped updates go to everyone currently viewing a match (live match, payments,
/// waiting list and chat) — clients opt in by joining the match group.</item>
/// <item>User-scoped updates go to a single user's own connections regardless of which screen
/// they are on (friend requests, match invitations, status changes, profile changes) — SignalR
/// routes these automatically by the authenticated user id.</item>
/// </list>
/// </summary>
public interface IRealtimeNotifier
{
    Task ChatMessagePostedAsync(Guid matchId, ChatMessageDto message, CancellationToken cancellationToken = default);
    Task MatchUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task PaymentUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task WaitingListUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes a user-scoped signal to every connection the given user has open, so their UI can
    /// refresh instantly. Best effort: a delivery failure must never break the calling use case.
    /// </summary>
    Task NotifyUserAsync(Guid userId, RealtimeUserEvent @event, CancellationToken cancellationToken = default);
}
