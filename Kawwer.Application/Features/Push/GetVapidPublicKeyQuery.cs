using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Push;

/// <summary>
/// Returns the server's VAPID public key so the browser can create a push subscription bound to
/// this application server. Empty when Web Push is not configured.
/// </summary>
public sealed record GetVapidPublicKeyQuery : IRequest<string>;

public sealed class GetVapidPublicKeyQueryHandler : IRequestHandler<GetVapidPublicKeyQuery, string>
{
    private readonly IWebPushSender _webPush;

    public GetVapidPublicKeyQueryHandler(IWebPushSender webPush) => _webPush = webPush;

    public Task<string> HandleAsync(GetVapidPublicKeyQuery request, CancellationToken cancellationToken)
        => Task.FromResult(_webPush.PublicKey ?? string.Empty);
}
