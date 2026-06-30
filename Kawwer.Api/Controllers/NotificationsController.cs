using Kawwer.Application.Features.Notifications;
using Kawwer.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool unreadOnly, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(
            new GetNotificationsQuery(CurrentUserId, unreadOnly, pagination.Page, pagination.PageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetUnreadCountQuery(CurrentUserId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new MarkNotificationReadCommand(CurrentUserId, id), cancellationToken);
        return OkMessage();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new MarkAllNotificationsReadCommand(CurrentUserId), cancellationToken);
        return OkMessage("All notifications marked as read.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new DeleteNotificationCommand(CurrentUserId, id), cancellationToken);
        return OkMessage("Notification deleted.");
    }
}
