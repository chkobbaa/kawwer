using System.Net;
using System.Text.Json;
using Kawwer.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace Kawwer.Infrastructure.Notifications;

/// <summary>
/// Sends Web Push notifications to browser/PWA subscriptions using the VAPID protocol
/// (<see href="https://datatracker.ietf.org/doc/html/rfc8030">RFC 8030</see>). This is the channel
/// iOS 16.4+ Home-Screen PWAs use. The encrypted payload is a small JSON document that the app's
/// service worker reads to build and display the notification.
/// </summary>
public sealed class WebPushNotificationSender : IWebPushSender
{
    private static readonly JsonSerializerOptions PayloadJsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly WebPushOptions _options;
    private readonly ILogger<WebPushNotificationSender> _logger;
    private readonly WebPushClient _client = new();
    private readonly VapidDetails? _vapid;

    public WebPushNotificationSender(
        IOptions<WebPushOptions> options,
        ILogger<WebPushNotificationSender> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (_options.IsConfigured)
        {
            _vapid = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
        }
    }

    public bool IsConfigured => _vapid is not null;

    public string? PublicKey => _options.IsConfigured ? _options.PublicKey : null;

    public async Task<WebPushResult> SendAsync(
        WebPushRecipient recipient,
        string title,
        string body,
        IReadOnlyDictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        if (_vapid is null)
        {
            _logger.LogInformation("Web Push not configured; skipping push: {Title}", title);
            return WebPushResult.Failed;
        }

        // The service worker's "push" handler reads exactly this shape to show the notification.
        var payload = JsonSerializer.Serialize(
            new { title, body, data = data ?? new Dictionary<string, string>() },
            PayloadJsonOptions);

        var subscription = new PushSubscription(recipient.Endpoint, recipient.P256dh, recipient.Auth);

        try
        {
            await _client.SendNotificationAsync(subscription, payload, _vapid, cancellationToken);
            return WebPushResult.Delivered;
        }
        catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
        {
            // 404/410 means the browser unsubscribed or the endpoint is dead — tell the caller to prune it.
            _logger.LogInformation("Web Push subscription expired ({Status}); it will be pruned.", ex.StatusCode);
            return WebPushResult.Expired;
        }
        catch (Exception ex)
        {
            // A transient failure must never break the calling use case; keep the subscription.
            _logger.LogWarning(ex, "Web Push send failed for {Title}", title);
            return WebPushResult.Failed;
        }
    }
}
