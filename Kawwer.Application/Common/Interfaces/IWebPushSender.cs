namespace Kawwer.Application.Common.Interfaces;

/// <summary>The client keys needed to deliver an encrypted Web Push message to one subscription.</summary>
public sealed record WebPushRecipient(string Endpoint, string P256dh, string Auth);

/// <summary>The outcome of a single Web Push send, so callers can prune dead subscriptions.</summary>
public enum WebPushResult
{
    /// <summary>The push service accepted the message.</summary>
    Delivered,

    /// <summary>The push service reported the subscription is gone (404/410); it should be removed.</summary>
    Expired,

    /// <summary>A transient error occurred; keep the subscription and try again next time.</summary>
    Failed
}

/// <summary>
/// Sends a Web Push notification to a single browser subscription using the VAPID protocol
/// (W3C Push API). This is the channel that powers push in the installable iOS/Android PWA, and
/// runs alongside the existing FCM sender used by the native app.
/// </summary>
public interface IWebPushSender
{
    /// <summary>True when VAPID keys are configured. When false, sends are skipped (no-op).</summary>
    bool IsConfigured { get; }

    /// <summary>The VAPID public key (base64url) the browser needs to create a subscription.</summary>
    string? PublicKey { get; }

    Task<WebPushResult> SendAsync(
        WebPushRecipient recipient,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
