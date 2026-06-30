using Kawwer.Application.Features.Chat;
using Kawwer.Contracts.Chat;
using Kawwer.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/matches/{matchId:guid}/chat")]
public sealed class ChatController : ApiControllerBase
{
    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(Guid matchId, [FromQuery] PaginationQuery pagination, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetMessagesQuery(matchId, pagination.Page, pagination.PageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("messages")]
    public async Task<IActionResult> Send(Guid matchId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new SendMessageCommand(CurrentUserId, matchId, request.Content), cancellationToken);
        return CreatedResponse(result);
    }

    [HttpPut("messages/{messageId:guid}")]
    public async Task<IActionResult> Edit(Guid matchId, Guid messageId, EditMessageRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new EditMessageCommand(CurrentUserId, messageId, request.Content), cancellationToken);
        return OkMessage("Message edited.");
    }

    [HttpDelete("messages/{messageId:guid}")]
    public async Task<IActionResult> Delete(Guid matchId, Guid messageId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new DeleteMessageCommand(CurrentUserId, messageId), cancellationToken);
        return OkMessage("Message deleted.");
    }

    [HttpPost("messages/{messageId:guid}/pin")]
    public async Task<IActionResult> Pin(Guid matchId, Guid messageId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new PinMessageCommand(CurrentUserId, matchId, messageId), cancellationToken);
        return OkMessage("Message pinned.");
    }
}
