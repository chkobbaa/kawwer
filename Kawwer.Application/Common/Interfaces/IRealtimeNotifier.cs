using Kawwer.Contracts.Chat;

namespace Kawwer.Application.Common.Interfaces;

/// <summary>
/// Pushes real-time updates to connected clients (SignalR) for live match, payments,
/// waiting list and chat.
/// </summary>
public interface IRealtimeNotifier
{
    Task ChatMessagePostedAsync(Guid matchId, ChatMessageDto message, CancellationToken cancellationToken = default);
    Task MatchUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task PaymentUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default);
    Task WaitingListUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default);
}
