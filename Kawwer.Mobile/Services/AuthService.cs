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
    }
}
