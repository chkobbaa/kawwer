using kawwer;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.Services;

/// <summary>Coordinates authentication and session lifecycle for the app.</summary>
public sealed class AuthService
{
    private readonly KawwerApiClient _api;
    private readonly SessionState _session;
    private readonly RealtimeService _realtime;

    public AuthService(KawwerApiClient api, SessionState session, RealtimeService realtime)
    {
        _api = api;
        _session = session;
        _realtime = realtime;
    }

    public SessionState Session => _session;

    public Task InitializeAsync() => _session.EnsureLoadedAsync();

    public async Task LoginAsync(string usernameOrEmail, string password, bool rememberMe = true)
    {
        var auth = await _api.LoginAsync(usernameOrEmail, password);
        await _session.SetAsync(auth, persist: rememberMe);

        // Open the real-time connection right away so the first screen is already live.
        _ = _realtime.StartAsync();
    }

    public async Task RegisterAsync(object body)
    {
        var auth = await _api.RegisterAsync(body);
        await _session.SetAsync(auth);
        _ = _realtime.StartAsync();
    }

    public async Task LogoutAsync()
    {
        if (!string.IsNullOrEmpty(_session.RefreshToken))
        {
            try
            {
                await _api.LogoutAsync(_session.RefreshToken);
            }
            catch
            {
                // Best effort; clear the local session regardless.
            }
        }

        // Drop the real-time connection so no pushes leak into the next session.
        await _realtime.StopAsync();

        _session.Clear();

        // Rebuild the shell from scratch. This tears down the entire navigation stack and every
        // transient view model, so logging back in starts on a clean Login screen instead of
        // whatever page (and cached data) was last open.
        AppShell.ResetToLogin(this);
    }
}
