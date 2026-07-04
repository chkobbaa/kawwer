namespace Kawwer.Infrastructure.Notifications;

/// <summary>
/// VAPID settings for the Web Push channel (the PWA), bound from configuration (section "WebPush").
/// Generate a key pair once (e.g. <c>npx web-push generate-vapid-keys</c>) and keep it stable — the
/// public key is baked into every browser subscription, so rotating it invalidates them all.
/// When the keys are empty, Web Push is disabled (in-app + FCM notifications still work).
/// </summary>
public sealed class WebPushOptions
{
    public const string SectionName = "WebPush";

    /// <summary>VAPID subject: a <c>mailto:</c> address or an HTTPS site URL identifying the sender.</summary>
    public string Subject { get; set; } = "mailto:notifications@kawwer.com";

    /// <summary>VAPID public key (base64url). Served to the browser to create a subscription.</summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>VAPID private key (base64url). Secret — set via configuration/environment in production.</summary>
    public string PrivateKey { get; set; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PublicKey)
        && !string.IsNullOrWhiteSpace(PrivateKey)
        && !string.IsNullOrWhiteSpace(Subject);
}
