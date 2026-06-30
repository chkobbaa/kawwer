using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Kawwer.Api.Realtime;

/// <summary>
/// SignalR hub backing live match, payments, waiting list and chat. Clients join a per-match
/// group to receive updates scoped to a single match.
/// </summary>
[Authorize]
public sealed class MatchHub : Hub
{
    public static string GroupName(Guid matchId) => $"match-{matchId}";

    public Task JoinMatch(Guid matchId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(matchId));

    public Task LeaveMatch(Guid matchId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(matchId));
}
