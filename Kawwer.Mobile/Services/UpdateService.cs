using System.Diagnostics;

namespace Kawwer.Mobile.Services;

/// <summary>
/// Custom (out-of-store) update checker. Asks the backend for the latest published version and, if
/// the installed build is older, prompts the user to download the new APK via the system browser.
/// </summary>
public sealed class UpdateService
{
    private readonly KawwerApiClient _api;
    private readonly IDialogService _dialog;

    public UpdateService(KawwerApiClient api, IDialogService dialog)
    {
        _api = api;
        _dialog = dialog;
    }

    /// <summary>
    /// Checks for a newer version. When <paramref name="announceUpToDate"/> is true (the manual
    /// "Check for updates" button), it also tells the user when they're already current.
    /// </summary>
    public async Task CheckForUpdateAsync(bool announceUpToDate = false)
    {
        Models.AppVersionDto info;
        try
        {
            info = await _api.GetAppVersionAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Update check failed: {ex.Message}");
            if (announceUpToDate)
            {
                await _dialog.ShowAlertAsync("Updates", "Couldn't check for updates right now. Try again later.");
            }

            return;
        }

        var current = AppInfo.Current.VersionString;
        var updateAvailable = IsNewer(info.LatestVersion, current) && !string.IsNullOrWhiteSpace(info.DownloadUrl);

        if (updateAvailable)
        {
            var download = await _dialog.ConfirmAsync(
                "Update available",
                $"A new version ({info.LatestVersion}) is available. You're on {current}.",
                "Download",
                info.Mandatory ? "Later" : "Not now");

            if (download)
            {
                await OpenDownloadAsync(info.DownloadUrl);
            }
        }
        else if (announceUpToDate)
        {
            await _dialog.ShowAlertAsync("Updates", $"You're up to date (v{current}).");
        }
    }

    private static async Task OpenDownloadAsync(string url)
    {
        try
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to open update URL: {ex.Message}");
        }
    }

    /// <summary>Returns true when <paramref name="latest"/> is a higher version than <paramref name="current"/>.</summary>
    internal static bool IsNewer(string? latest, string? current)
    {
        if (Version.TryParse(Normalize(latest), out var latestVersion)
            && Version.TryParse(Normalize(current), out var currentVersion))
        {
            return latestVersion > currentVersion;
        }

        // Fall back to a plain string comparison when either value isn't a dotted version.
        return !string.IsNullOrWhiteSpace(latest)
               && !string.Equals(latest.Trim(), current?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string? value)
    {
        // Normalize to major.minor.build so "1.0" and "1.0.0" compare as equal, and AppInfo's
        // "1.0" compares sensibly against a server value like "1.0.5".
        var parts = (value ?? string.Empty).Trim().Split('.');
        var numbers = new List<string>();
        foreach (var part in parts)
        {
            numbers.Add(int.TryParse(part, out var n) ? n.ToString() : "0");
        }

        while (numbers.Count < 3)
        {
            numbers.Add("0");
        }

        return string.Join('.', numbers.Take(3));
    }
}
