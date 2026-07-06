using kawwer;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.Services;

/// <summary>Coordinates authentication and session lifecycle for the app.</summary>
public sealed class AuthService
{
    private readonly KawwerApiClient _api;
    private readonly SessionState _session;

    public AuthService(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
    }

    public SessionState Session => _session;

    /// <summary>
    /// True when a signed-in user still needs to go through the first-run onboarding flow. Prefers
    /// the freshest loaded profile and falls back to the fast persisted flag used at cold start, so
    /// startup routing can decide between onboarding and the main tabs without a network round-trip.
    /// </summary>
    public bool RequiresOnboarding =>
        _session.IsAuthenticated
        && !(_session.CurrentUser?.OnboardingCompleted ?? _session.OnboardingCompleted);

    /// <summary>
    /// Restores any persisted tokens. The access token itself is refreshed lazily: the first
    /// authenticated request that gets a 401 transparently rotates it via <c>AuthHeaderHandler</c>,
    /// which keeps the user signed in across restarts until they explicitly log out.
    /// </summary>
    public Task InitializeAsync() => _session.EnsureLoadedAsync();

    public async Task LoginAsync(string usernameOrEmail, string password, bool rememberMe = true)
    {
        var auth = await _api.LoginAsync(usernameOrEmail, password);
        await _session.SetAsync(auth, persist: rememberMe);
    }

    public async Task RegisterAsync(object body)
    {
        var auth = await _api.RegisterAsync(body);
        await _session.SetAsync(auth);
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

        _session.Clear();

        // Rebuild the shell from scratch. This tears down the entire navigation stack and every
        // transient view model, so logging back in starts on a clean Login screen instead of
        // whatever page (and cached data) was last open.
        AppShell.ResetToLogin(this);
    }
}
