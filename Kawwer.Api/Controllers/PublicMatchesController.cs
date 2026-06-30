using Kawwer.Application.Features.PublicMatches;
using Kawwer.Contracts.Common;
using Kawwer.Contracts.PublicMatches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/public-matches")]
public sealed class PublicMatchesController : ApiControllerBase
{
    [HttpGet("discover")]
    public async Task<IActionResult> Discover(
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] decimal? latitude,
        [FromQuery] decimal? longitude,
        [FromQuery] double? radiusKm,
        [FromQuery] PaginationQuery pagination,
        CancellationToken cancellationToken)
    {
        var query = new DiscoverMatchesQuery(dateFrom, dateTo, latitude, longitude, radiusKm, pagination.Page, pagination.PageSize);
        var result = await Dispatcher.SendAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/join")]
    public async Task<IActionResult> Join(Guid id, CancellationToken cancellationToken)
    {
        var accepted = await Dispatcher.SendAsync(new RequestToJoinMatchCommand(CurrentUserId, id), cancellationToken);
        return Ok(new { accepted }, accepted ? "You joined the match." : "Join request sent for approval.");
    }

    [HttpGet("{id:guid}/join-requests")]
    public async Task<IActionResult> JoinRequests(Guid id, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetJoinRequestsQuery(CurrentUserId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/join-requests/{userId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RespondToJoinRequestCommand(CurrentUserId, id, userId, Approve: true), cancellationToken);
        return OkMessage("Join request approved.");
    }

    [HttpPost("{id:guid}/join-requests/{userId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RespondToJoinRequestCommand(CurrentUserId, id, userId, Approve: false), cancellationToken);
        return OkMessage("Join request rejected.");
    }
}
