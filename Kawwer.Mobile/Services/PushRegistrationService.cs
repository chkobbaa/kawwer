namespace Kawwer.Mobile.Services;

/// <summary>
/// Registers the device for push notifications: asks for the notification permission
/// (Android 13+), fetches the FCM token and uploads it to the API. Safe to call often;
/// it only does the work once per app run. No-ops when Firebase is not configured.
/// </summary>
public sealed class PushRegistrationService
{
    private readonly KawwerApiClient _api;
    private bool _registered;

    public PushRegistrationService(KawwerApiClient api) => _api = api;

    public async Task TryRegisterAsync()
    {
        if (_registered)
        {
            return;
        }

        try
        {
#if ANDROID
            var status = await Permissions.RequestAsync<Permissions.PostNotifications>();
            if (status != PermissionStatus.Granted)
            {
                return;
            }

            var token = await kawwer.PushNotifications.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            await _api.UpdateDeviceTokenAsync(token);
            _registered = true;
#else
            await Task.CompletedTask;
#endif
        }
        catch
        {
            // Push is best effort; in-app notifications still work.
        }
    }

    /// <summary>Detaches this device on logout so pushes stop reaching it.</summary>
    public async Task UnregisterAsync()
    {
        try
        {
            await _api.UpdateDeviceTokenAsync(null);
        }
        catch
        {
            // Best effort.
        }

        _registered = false;
    }
}
