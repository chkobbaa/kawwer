using Kawwer.Application.Common.Interfaces;
using Kawwer.Contracts.Chat;
using Kawwer.Contracts.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Kawwer.Api.Realtime;

/// <summary>Broadcasts real-time updates to clients: match groups for shared screens, and the
/// per-user channel for anything scoped to a single account.</summary>
public sealed class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<MatchHub> _hub;

    public SignalRRealtimeNotifier(IHubContext<MatchHub> hub) => _hub = hub;

    public Task ChatMessagePostedAsync(Guid matchId, ChatMessageDto message, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(MatchHub.GroupName(matchId)).SendAsync("ChatMessagePosted", message, cancellationToken);

    public Task MatchUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(MatchHub.GroupName(matchId)).SendAsync("MatchUpdated", matchId, cancellationToken);

    public Task PaymentUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(MatchHub.GroupName(matchId)).SendAsync("PaymentUpdated", matchId, cancellationToken);

    public Task WaitingListUpdatedAsync(Guid matchId, CancellationToken cancellationToken = default)
        => _hub.Clients.Group(MatchHub.GroupName(matchId)).SendAsync("WaitingListUpdated", matchId, cancellationToken);

    public Task NotifyUserAsync(Guid userId, RealtimeUserEvent @event, CancellationToken cancellationToken = default)
        => _hub.Clients.User(userId.ToString()).SendAsync("UserEvent", @event, cancellationToken);
}
