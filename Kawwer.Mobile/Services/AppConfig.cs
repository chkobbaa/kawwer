namespace Kawwer.Mobile.Services;

/// <summary>
/// Central client configuration. Change <see cref="Host"/> before each CodeMagic build:
/// LAN IP while testing against a PC on your Wi‑Fi, or your public HTTPS URL once deployed.
/// </summary>
public static class AppConfig
{
    // Local dev:  http://192.168.x.x:5251  (your PC's LAN address — phone must be on same Wi‑Fi)
    // Production: https://api.yourdomain.com  (see docs/AzureDeployment.md)
    private const string Host = "https://api.bahroun.com";

    public static string ApiBaseUrl { get; set; } = $"{Host}/api/v1";
    public static string HubBaseUrl { get; set; } = $"{Host}/hubs/match";
}
