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
