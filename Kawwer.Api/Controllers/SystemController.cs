using Kawwer.Contracts.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

/// <summary>
/// System/metadata endpoints that don't require authentication. Currently powers the custom
/// in-app APK update flow.
/// </summary>
[AllowAnonymous]
[Route("api/v1/system")]
public sealed class SystemController : ApiControllerBase
{
    private readonly IConfiguration _configuration;

    public SystemController(IConfiguration configuration) => _configuration = configuration;

    /// <summary>
    /// Returns the latest published app version and its download URL. Values come from the
    /// "AppUpdate" configuration section so a new build can be announced without redeploying code.
    /// </summary>
    [HttpGet("version")]
    public IActionResult GetVersion()
    {
        var section = _configuration.GetSection("AppUpdate");
        var dto = new AppVersionDto(
            LatestVersion: section["LatestVersion"] ?? "1.0.0",
            DownloadUrl: section["DownloadUrl"] ?? string.Empty,
            Mandatory: section.GetValue("Mandatory", false));

        return Ok(dto);
    }
}
