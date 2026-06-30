namespace Kawwer.Mobile.Services;

/// <summary>Central client configuration. 10.0.2.2 is the host loopback as seen from the Android emulator.</summary>
public static class AppConfig
{
    public static string ApiBaseUrl { get; set; } = "https://10.0.2.2:5001/api/v1";
    public static string HubBaseUrl { get; set; } = "https://10.0.2.2:5001/hubs/match";
}
