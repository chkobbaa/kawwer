using Kawwer.Application.Features.Friends;
using Kawwer.Contracts.Friends;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/friends")]
public sealed class FriendsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFriends(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetFriendsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetRequests(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetFriendRequestsQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("requests")]
    public async Task<IActionResult> SendRequest(SendFriendRequestRequest request, CancellationToken cancellationToken)
    {
        var id = await Dispatcher.SendAsync(new SendFriendRequestCommand(CurrentUserId, request.TargetUserId), cancellationToken);
        return Ok(id, "Friend request sent.");
    }

    [HttpPost("requests/{friendshipId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid friendshipId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new AcceptFriendRequestCommand(CurrentUserId, friendshipId), cancellationToken);
        return OkMessage("Friend request accepted.");
    }

    [HttpPost("requests/{friendshipId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid friendshipId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RejectFriendRequestCommand(CurrentUserId, friendshipId), cancellationToken);
        return OkMessage("Friend request rejected.");
    }

    [HttpDelete("{friendUserId:guid}")]
    public async Task<IActionResult> Remove(Guid friendUserId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RemoveFriendCommand(CurrentUserId, friendUserId), cancellationToken);
        return OkMessage("Friend removed.");
    }

    [HttpPost("block/{targetUserId:guid}")]
    public async Task<IActionResult> Block(Guid targetUserId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new BlockUserCommand(CurrentUserId, targetUserId), cancellationToken);
        return OkMessage("User blocked.");
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new SearchUsersQuery(CurrentUserId, term), cancellationToken);
        return Ok(result);
    }
}
