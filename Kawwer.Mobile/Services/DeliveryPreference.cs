using Microsoft.Maui.Storage;

namespace Kawwer.Mobile.Services;

/// <summary>
/// The user's chosen delivery mode for important, easy-to-miss updates (like a match reschedule).
///
/// <list type="bullet">
/// <item><b>Notify</b> (default): important updates arrive as normal notifications.</item>
/// <item><b>Call</b>: the app first simulates a short incoming "call" so the update is far harder
/// to ignore, then the normal notification is delivered too.</item>
/// </list>
///
/// Stored on-device (a per-install preference), so it is read/written from both the settings toggle
/// and the background call-simulation service without a round trip.
/// </summary>
public static class DeliveryPreference
{
    private const string Key = "pref_delivery_mode_call";

    public static bool CallMode
    {
        get => Preferences.Default.Get(Key, false);
        set => Preferences.Default.Set(Key, value);
    }
}
