using Kawwer.Application.Features.Push;
using Kawwer.Contracts.Push;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kawwer.Api.Controllers;

/// <summary>
/// Web Push (PWA) subscription management. The native app keeps using the FCM device-token
/// endpoint on <see cref="UsersController"/>; these endpoints power push in the installable web app.
/// </summary>
[Authorize]
[Route("api/v1/push")]
public sealed class PushController : ApiControllerBase
{
    /// <summary>The VAPID public key the browser needs before it can create a subscription.</summary>
    [AllowAnonymous]
    [HttpGet("vapid-public-key")]
    public async Task<IActionResult> GetVapidPublicKey(CancellationToken cancellationToken)
    {
        var publicKey = await Dispatcher.SendAsync(new GetVapidPublicKeyQuery(), cancellationToken);
        return Ok(new VapidPublicKeyResponse(publicKey));
    }

    [HttpPost("web/subscribe")]
    public async Task<IActionResult> Subscribe(WebPushSubscriptionRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(
            new SubscribeWebPushCommand(CurrentUserId, request.Endpoint, request.Keys.P256dh, request.Keys.Auth),
            cancellationToken);
        return OkMessage("Push subscription registered.");
    }

    [HttpPost("web/unsubscribe")]
    public async Task<IActionResult> Unsubscribe(UnsubscribeWebPushRequest request, CancellationToken cancellationToken)
    {
        await Dispatcher.SendAsync(new UnsubscribeWebPushCommand(CurrentUserId, request.Endpoint), cancellationToken);
        return OkMessage("Push subscription removed.");
    }
}
