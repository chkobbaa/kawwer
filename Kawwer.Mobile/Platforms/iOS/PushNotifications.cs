using Kawwer.Mobile.Services;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.CloudMessaging.EventArgs;

namespace kawwer;

/// <summary>
/// Firebase Cloud Messaging on iOS. Degrades gracefully when GoogleService-Info.plist
/// is absent or push entitlements are not available (e.g. free sideload profiles).
/// </summary>
public static class PushNotifications
{
    private static bool _eventsWired;

    public static void WireEvents()
    {
        if (_eventsWired)
        {
            return;
        }

        _eventsWired = true;
        var messaging = CrossFirebaseCloudMessaging.Current;

        messaging.TokenChanged += (_, e) => _ = UploadTokenAsync(e.Token);
        messaging.NotificationTapped += (_, e) =>
        {
            var data = e.Notification?.Data;
            NotificationNavigation.SetPending(
                Get(data, "category"),
                Get(data, "matchId"));
        };
    }

    public static async Task<string?> GetTokenAsync()
    {
        try
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            return await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        }
        catch
        {
            return null;
        }
    }

    private static async Task UploadTokenAsync(string token)
    {
        try
        {
            var services = IPlatformApplication.Current?.Services;
            var api = services?.GetService<KawwerApiClient>();
            var session = services?.GetService<SessionState>();
            if (api is null || session is null)
            {
                return;
            }

            await session.EnsureLoadedAsync();
            if (session.IsAuthenticated)
            {
                await api.UpdateDeviceTokenAsync(token);
            }
        }
        catch
        {
            // Best effort; PushRegistrationService re-uploads on next login.
        }
    }

    private static string? Get(IDictionary<string, string>? data, string key)
        => data is not null && data.TryGetValue(key, out var value) ? value : null;
}
