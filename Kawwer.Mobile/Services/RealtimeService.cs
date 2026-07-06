using Kawwer.Mobile.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace Kawwer.Mobile.Services;

/// <summary>
/// The app's real-time client. Holds a single SignalR connection to the API's match hub and
/// surfaces server pushes as plain C# events that view models subscribe to, so screens update
/// the instant data changes — no polling, no manual pull-to-refresh.
///
/// Two kinds of message arrive:
/// <list type="bullet">
/// <item><b>User-scoped</b> (<see cref="UserEvent"/>): friend requests, invitations, match status
/// changes and profile changes for the signed-in account. SignalR routes these automatically, so
/// the client just has to be connected.</item>
/// <item><b>Match-scoped</b> (<see cref="MatchUpdated"/>, <see cref="PaymentUpdated"/>,
/// <see cref="WaitingListUpdated"/>, <see cref="ChatMessagePosted"/>): only for a match the client
/// has explicitly joined via <see cref="JoinMatchAsync"/>.</item>
/// </list>
///
/// All events are raised on the UI thread so handlers can touch bound collections directly. The
/// connection authenticates with the session's access token and reconnects automatically; joined
/// matches are re-joined after a reconnect.
/// </summary>
public sealed class RealtimeService : IAsyncDisposable
{
    private readonly SessionState _session;
    private readonly SemaphoreSlim _gate = new(1, 1);

    // Ref-counted match memberships: several screens (details, chat, payments, live) can watch the
    // same match at once, so we only tell the server to join on the first and leave on the last.
    private readonly Dictionary<Guid, int> _joinedMatches = new();
    private readonly object _matchesLock = new();
    private HubConnection? _connection;

    public RealtimeService(SessionState session) => _session = session;

    /// <summary>A user-scoped signal (friend request, invitation, match/profile change) arrived.</summary>
    public event Action<RealtimeUserEvent>? UserEvent;

    /// <summary>A joined match changed (roster, capacity, live state, cancellation, ...).</summary>
    public event Action<Guid>? MatchUpdated;

    /// <summary>Payment collection for a joined match changed.</summary>
    public event Action<Guid>? PaymentUpdated;

    /// <summary>The waiting list for a joined match changed.</summary>
    public event Action<Guid>? WaitingListUpdated;

    /// <summary>A new chat message was posted to a joined match.</summary>
    public event Action<Guid, ChatMessageDto>? ChatMessagePosted;

    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    /// <summary>
    /// Ensures a live connection exists (starting one if needed). Safe to call often and from any
    /// screen; it no-ops when already connected and never throws — real-time is best effort and
    /// the app still works through normal loads if the socket can't be established.
    /// </summary>
    public async Task StartAsync()
    {
        await _session.EnsureLoadedAsync();
        if (!_session.IsAuthenticated)
        {
            return;
        }

        await _gate.WaitAsync();
        try
        {
            if (_connection is null)
            {
                _connection = BuildConnection();
            }

            if (_connection.State == HubConnectionState.Disconnected)
            {
                await _connection.StartAsync();
            }
        }
        catch
        {
            // Best effort: WithAutomaticReconnect only retries an established connection, so a
            // failed first start is retried the next time a screen calls StartAsync.
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Tears the connection down on logout so no stale pushes reach the next user.</summary>
    public async Task StopAsync()
    {
        await _gate.WaitAsync();
        HubConnection? connection;
        try
        {
            lock (_matchesLock)
            {
                _joinedMatches.Clear();
            }

            connection = _connection;
            _connection = null;
        }
        finally
        {
            _gate.Release();
        }

        if (connection is not null)
        {
            try
            {
                await connection.StopAsync();
            }
            catch
            {
                // Ignore; we're disposing it anyway.
            }

            await connection.DisposeAsync();
        }
    }

    /// <summary>Joins a match group to receive its live match/payment/waiting-list/chat updates.</summary>
    public async Task JoinMatchAsync(Guid matchId)
    {
        if (matchId == Guid.Empty)
        {
            return;
        }

        await StartAsync();

        bool firstWatcher;
        lock (_matchesLock)
        {
            _joinedMatches.TryGetValue(matchId, out var count);
            _joinedMatches[matchId] = count + 1;
            firstWatcher = count == 0;
        }

        if (firstWatcher)
        {
            await InvokeSafelyAsync("JoinMatch", matchId);
        }
    }

    /// <summary>Leaves a match group once the last screen watching it goes away.</summary>
    public async Task LeaveMatchAsync(Guid matchId)
    {
        bool lastWatcher;
        lock (_matchesLock)
        {
            if (!_joinedMatches.TryGetValue(matchId, out var count))
            {
                return;
            }

            if (count <= 1)
            {
                _joinedMatches.Remove(matchId);
                lastWatcher = true;
            }
            else
            {
                _joinedMatches[matchId] = count - 1;
                lastWatcher = false;
            }
        }

        if (lastWatcher)
        {
            await InvokeSafelyAsync("LeaveMatch", matchId);
        }
    }

    private HubConnection BuildConnection()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(AppConfig.HubBaseUrl, options =>
            {
                // The hub authenticates via the access_token query string (see the API's
                // JwtBearerEvents). Resolve the token lazily so a refresh is always picked up.
                options.AccessTokenProvider = async () =>
                {
                    await _session.EnsureLoadedAsync();
                    return _session.AccessToken;
                };
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<RealtimeUserEvent>("UserEvent", e => Dispatch(() => UserEvent?.Invoke(e)));
        connection.On<Guid>("MatchUpdated", id => Dispatch(() => MatchUpdated?.Invoke(id)));
        connection.On<Guid>("PaymentUpdated", id => Dispatch(() => PaymentUpdated?.Invoke(id)));
        connection.On<Guid>("WaitingListUpdated", id => Dispatch(() => WaitingListUpdated?.Invoke(id)));
        connection.On<ChatMessageDto>("ChatMessagePosted", m => Dispatch(() => ChatMessagePosted?.Invoke(m.MatchId, m)));

        // A dropped connection loses its group memberships; re-join everything on reconnect.
        connection.Reconnected += async _ =>
        {
            List<Guid> matches;
            lock (_matchesLock)
            {
                matches = _joinedMatches.Keys.ToList();
            }

            foreach (var matchId in matches)
            {
                await InvokeSafelyAsync("JoinMatch", matchId);
            }
        };

        return connection;
    }

    private async Task InvokeSafelyAsync(string method, Guid matchId)
    {
        var connection = _connection;
        if (connection is not { State: HubConnectionState.Connected })
        {
            return;
        }

        try
        {
            await connection.InvokeAsync(method, matchId);
        }
        catch
        {
            // Group membership is best effort; a full reload on the next screen visit recovers.
        }
    }

    private static void Dispatch(Action action)
    {
        if (MainThread.IsMainThread)
        {
            action();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _gate.Dispose();
    }
}
