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

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public Guid? UserId { get; private set; }
    public UserDto? CurrentUser { get; set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

    public async Task LoadAsync()
    {
        AccessToken = await SecureStorage.Default.GetAsync(AccessKey);
        RefreshToken = await SecureStorage.Default.GetAsync(RefreshKey);
        var id = await SecureStorage.Default.GetAsync(UserIdKey);
        UserId = Guid.TryParse(id, out var parsed) ? parsed : null;
    }

    public async Task SetAsync(AuthResponse auth)
    {
        AccessToken = auth.AccessToken;
        RefreshToken = auth.RefreshToken;
        UserId = auth.User.Id;
        CurrentUser = auth.User;

        await SecureStorage.Default.SetAsync(AccessKey, auth.AccessToken);
        await SecureStorage.Default.SetAsync(RefreshKey, auth.RefreshToken);
        await SecureStorage.Default.SetAsync(UserIdKey, auth.User.Id.ToString());
    }

    public void UpdateTokens(string accessToken, string refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        _ = SecureStorage.Default.SetAsync(AccessKey, accessToken);
        _ = SecureStorage.Default.SetAsync(RefreshKey, refreshToken);
    }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        UserId = null;
        CurrentUser = null;
        SecureStorage.Default.Remove(AccessKey);
        SecureStorage.Default.Remove(RefreshKey);
        SecureStorage.Default.Remove(UserIdKey);
    }
}
