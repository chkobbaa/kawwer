using Kawwer.Application.Features.Statistics;
using Kawwer.Application.Features.Users;
using Kawwer.Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/users")]
public sealed class UsersController : ApiControllerBase
{
    // Profile pictures are capped to keep uploads snappy on mobile data; the client already
    // compresses before sending.
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetProfileQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateProfileCommand(
            CurrentUserId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.BirthDate,
            request.PreferredPosition,
            request.PreferredFoot,
            request.SkillLevel,
            request.Visibility);

        var result = await Dispatcher.SendAsync(command, cancellationToken);
        return Ok(result, "Profile updated.");
    }

    /// <summary>Deletes (deactivates) the caller's own account and ends the session.</summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMe(CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new DeleteAccountCommand(CurrentUserId), cancellationToken);
        return OkMessage("Account deleted.");
    }

    /// <summary>Uploads a new profile picture (multipart form field named "file").</summary>
    [HttpPost("me/photo")]
    [RequestSizeLimit(MaxPhotoBytes)]
    public async Task<IActionResult> UploadPhoto(
        IFormFile? file,
        [FromServices] IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(Contracts.Common.ApiResponse.Fail("No image was uploaded."));
        }

        if (file.Length > MaxPhotoBytes)
        {
            return BadRequest(Contracts.Common.ApiResponse.Fail("The image is too large (max 5 MB)."));
        }

        if (!AllowedImageTypes.TryGetValue(file.ContentType, out var extension))
        {
            return BadRequest(Contracts.Common.ApiResponse.Fail("Only JPEG, PNG or WebP images are allowed."));
        }

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "avatars");
        Directory.CreateDirectory(folder);

        // One file per user keeps storage bounded; the query string busts the client image cache.
        var fileName = $"{CurrentUserId}{extension}";
        var absolutePath = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(absolutePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{fileName}?v={DateTime.UtcNow.Ticks}";
        var result = await Dispatcher.SendAsync(new UpdateProfilePictureCommand(CurrentUserId, url), cancellationToken);
        return Ok(result, "Profile picture updated.");
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetProfileQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPut("me/device-token")]
    public async Task<IActionResult> UpdateDeviceToken(UpdateDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new UpdateDeviceTokenCommand(CurrentUserId, request.DeviceToken), cancellationToken);
        return OkMessage("Device token updated.");
    }

    [HttpGet("me/statistics")]
    public async Task<IActionResult> GetMyPlayerStatistics(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetPlayerStatisticsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("me/statistics/organizer")]
    public async Task<IActionResult> GetMyOrganizerStatistics(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetOrganizerStatisticsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/statistics")]
    public async Task<IActionResult> GetPlayerStatistics(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetPlayerStatisticsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/organizing")]
    public async Task<IActionResult> GetOrganizing(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetUserOrganizingMatchesQuery(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }
}
