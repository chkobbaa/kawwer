using System.Diagnostics;
using Kawwer.Mobile.Models;
using Microsoft.Maui.Storage;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Holds the authenticated session in memory and persists tokens to secure storage so the user
/// stays signed in across launches.
/// </summary>
public sealed class SessionState
{
    private const string AccessKey = "kawwer_access_token";
    private const string RefreshKey = "kawwer_refresh_token";
    private const string UserIdKey = "kawwer_user_id";
    private const string HasSessionKey = "kawwer_has_session";

    private Task? _loadTask;

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? UserId { get; private set; }
    public UserDto? CurrentUser { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    /// <summary>
    /// Fast, synchronous check used at startup to route straight to the main tabs without
    /// waiting for secure storage. The actual tokens are loaded asynchronously.
    /// </summary>
    public bool HasPersistedSession
    {
        get
        {
            try
            {
                return Preferences.Default.Get(HasSessionKey, false);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>Loads the tokens exactly once; callers can await this before using them.</summary>
    public Task EnsureLoadedAsync() => _loadTask ??= LoadAsync();

    public async Task LoadAsync()
    {
        try
        {
            AccessToken = await SecureStorage.Default.GetAsync(AccessKey);
            RefreshToken = await SecureStorage.Default.GetAsync(RefreshKey);
            var id = await SecureStorage.Default.GetAsync(UserIdKey);
            UserId = Guid.TryParse(id, out var parsed) ? parsed : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Secure storage load failed: {ex}");
            AccessToken = null;
            RefreshToken = null;
            UserId = null;
        }
    }

    /// <summary>False when the user unticked "Remember me": tokens live in memory only.</summary>
    private bool _persistTokens = true;

    public async Task SetAsync(AuthResponse auth, bool persist = true)
    {
        AccessToken = auth.AccessToken;
        RefreshToken = auth.RefreshToken;
        UserId = auth.User.Id;
        CurrentUser = auth.User;
        _persistTokens = persist;

        try
        {
            if (persist)
            {
                await SecureStorage.Default.SetAsync(AccessKey, auth.AccessToken);
                await SecureStorage.Default.SetAsync(RefreshKey, auth.RefreshToken);
                await SecureStorage.Default.SetAsync(UserIdKey, auth.User.Id.ToString());
                Preferences.Default.Set(HasSessionKey, true);
            }
            else
            {
                // "Remember me" off: make sure nothing from a previous session lingers.
                SecureStorage.Default.Remove(AccessKey);
                SecureStorage.Default.Remove(RefreshKey);
                SecureStorage.Default.Remove(UserIdKey);
                Preferences.Default.Remove(HasSessionKey);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Secure storage save failed: {ex}");
        }
    }

    public async Task UpdateTokensAsync(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;

        if (!_persistTokens)
        {
            return;
        }

        try
        {
            // Awaited on purpose: the refresh token was just rotated (the old one is revoked
            // server-side), so losing the new one because the process died mid-write would
            // log the user out on the next launch.
            await SecureStorage.Default.SetAsync(AccessKey, accessToken);
            await SecureStorage.Default.SetAsync(RefreshKey, refreshToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Secure storage update failed: {ex}");
        }
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        UserId = null;
        CurrentUser = null;

        try
        {
            SecureStorage.Default.Remove(AccessKey);
            SecureStorage.Default.Remove(RefreshKey);
            SecureStorage.Default.Remove(UserIdKey);
            Preferences.Default.Remove(HasSessionKey);

            // Drop cached profile/home data so the next user never sees the previous user's info.
            foreach (var key in JsonCache.Keys.All)
            {
                JsonCache.Remove(key);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Secure storage clear failed: {ex}");
        }
    }
}
