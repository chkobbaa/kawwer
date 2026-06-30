using Kawwer.Application.Features.Payments;
using Kawwer.Contracts.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

[Authorize]
[Route("api/v1/matches/{matchId:guid}/payments")]
public sealed class PaymentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Summary(Guid matchId, CancellationToken cancellationToken)
    {
        var result = await Dispatcher.SendAsync(new GetPaymentSummaryQuery(matchId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start(Guid matchId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new StartPaymentCollectionCommand(CurrentUserId, matchId), cancellationToken);
        return OkMessage("Payment collection started.");
    }

    [HttpPost("record")]
    public async Task<IActionResult> Record(Guid matchId, RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new RecordPaymentCommand(CurrentUserId, matchId, request.UserId, request.Amount), cancellationToken);
        return OkMessage("Payment recorded.");
    }

    [HttpPost("mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid matchId, MarkPaidRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new MarkPaidCommand(CurrentUserId, matchId, request.UserId), cancellationToken);
        return OkMessage("Player marked as paid.");
    }

    [HttpPost("undo")]
    public async Task<IActionResult> Undo(Guid matchId, UndoPaymentRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new UndoPaymentCommand(CurrentUserId, matchId, request.UserId), cancellationToken);
        return OkMessage("Payment undone.");
    }

    [HttpPost("split")]
    public async Task<IActionResult> Split(Guid matchId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new SplitRemainingBalanceCommand(CurrentUserId, matchId), cancellationToken);
        return OkMessage("Remaining balance split.");
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish(Guid matchId, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new FinishPaymentCollectionCommand(CurrentUserId, matchId), cancellationToken);
        return OkMessage("Collection completed.");
    }
}
