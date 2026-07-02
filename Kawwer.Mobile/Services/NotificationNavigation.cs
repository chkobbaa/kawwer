namespace Kawwer.Mobile.Services;

/// <summary>
/// Routes notification taps to the right screen. Android stores the target here when a push
/// is tapped (cold start or resume); the Shell flushes it as soon as the main tabs are ready.
/// </summary>
public static class NotificationNavigation
{
    private static string? _pendingRoute;

    /// <summary>Maps a notification's category + related match to a Shell route.</summary>
    public static string BuildRoute(string? category, string? matchId)
    {
        var hasMatch = Guid.TryParse(matchId, out _);
        return (category, hasMatch) switch
        {
            ("LiveMatch", true) => $"livematch?matchId={matchId}",
            ("Payment", true) => $"payments?matchId={matchId}",
            (_, true) => $"matchdetails?matchId={matchId}",
            ("Friend", _) => "//main/friendstab",
            _ => "notifications"
        };
    }

    /// <summary>Remembers where a tapped notification should lead and navigates when possible.</summary>
    public static void SetPending(string? category, string? matchId)
    {
        _pendingRoute = BuildRoute(category, matchId);
        MainThread.BeginInvokeOnMainThread(() => _ = TryNavigateAsync());
    }

    /// <summary>
    /// Navigates to the pending route when the Shell is up and the user is past the login
    /// screen. Safe to call repeatedly; it no-ops when there is nothing pending.
    /// </summary>
    public static async Task TryNavigateAsync()
    {
        var route = _pendingRoute;
        if (route is null || Shell.Current is not { } shell)
        {
            return;
        }

        // Don't hijack the login flow; AppShell flushes again once the tabs are shown.
        if (shell.CurrentItem?.Route != "main")
        {
            return;
        }

        _pendingRoute = null;
        try
        {
            await shell.GoToAsync(route);
        }
        catch
        {
            // Navigation can fail mid-startup; keep the route for the next attempt.
            _pendingRoute = route;
        }
    }
}
