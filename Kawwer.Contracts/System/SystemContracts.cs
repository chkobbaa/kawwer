namespace Kawwer.Contracts.System;

/// <summary>
/// The latest published app version and where to download it. Consumed by the mobile update
/// checker, which compares <see cref="LatestVersion"/> against the installed version.
/// </summary>
public sealed record AppVersionDto(
    string LatestVersion,
    string DownloadUrl,
    bool Mandatory);
