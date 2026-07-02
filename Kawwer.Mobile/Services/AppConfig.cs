namespace Kawwer.Mobile.Services;

/// <summary>Central client configuration. 10.0.2.2 is the host loopback as seen from the Android emulator.</summary>
public static class AppConfig
{
    public static string ApiBaseUrl { get; set; } = "http://192.168.1.6:5251/api/v1";
    public static string HubBaseUrl { get; set; } = "http://192.168.1.6:5251/hubs/match";
}
