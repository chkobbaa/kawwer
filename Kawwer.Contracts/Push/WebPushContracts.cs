namespace Kawwer.Contracts.Push;

/// <summary>The client keys the browser produces for a subscription (from <c>PushSubscription.toJSON()</c>).</summary>
public sealed record WebPushKeysDto(string P256dh, string Auth);

/// <summary>
/// A Web Push subscription registration. Mirrors the JSON a browser returns from
/// <c>pushManager.subscribe(...).toJSON()</c>: an endpoint URL plus the two encryption keys.
/// </summary>
public sealed record WebPushSubscriptionRequest(string Endpoint, WebPushKeysDto Keys);

/// <summary>Removes a Web Push subscription by its endpoint.</summary>
public sealed record UnsubscribeWebPushRequest(string Endpoint);

/// <summary>The VAPID public key the browser needs to create a subscription.</summary>
public sealed record VapidPublicKeyResponse(string PublicKey);
