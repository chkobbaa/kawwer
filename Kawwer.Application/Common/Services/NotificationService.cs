using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Common.Services;

/// <summary>
/// Default notification orchestration: persists an in-app <see cref="Notification"/> and pushes to
/// every channel the user has registered — the native app's FCM device token and any web-push
/// (PWA) subscriptions. Push failures are swallowed so a transient error never breaks the
/// originating use case. Web-push subscriptions the push service reports as gone are pruned.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IPushNotificationSender _push;
    private readonly IWebPushSender _webPush;
    private readonly IPushSubscriptionRepository _webPushSubscriptions;

    public NotificationService(
        INotificationRepository notifications,
        IUserRepository users,
        IPushNotificationSender push,
        IWebPushSender webPush,
        IPushSubscriptionRepository webPushSubscriptions)
    {
        _notifications = notifications;
        _users = users;
        _push = push;
        _webPush = webPush;
        _webPushSubscriptions = webPushSubscriptions;
    }

    public async Task NotifyAsync(
        Guid userId,
        NotificationCategory category,
        string title,
        string message,
        Guid? relatedMatchId = null,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? data = null)
    {
        _notifications.Add(new Notification(userId, category, title, message, relatedMatchId));

        var payload = new Dictionary<string, string> { ["category"] = category.ToString() };
        if (relatedMatchId is not null)
        {
            payload["matchId"] = relatedMatchId.Value.ToString();
        }

        if (data is not null)
        {
            foreach (var (key, value) in data)
            {
                payload[key] = value;
            }
        }

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user?.DeviceToken is { Length: > 0 } token)
        {
            try
            {
                await _push.SendAsync(token, title, message, payload, cancellationToken);
            }
            catch
            {
                // A failed push must not roll back the use case; the in-app notification persists.
            }
        }

        await SendWebPushAsync(userId, title, message, payload, cancellationToken);
    }

    public async Task NotifyManyAsync(
        IEnumerable<Guid> userIds,
        NotificationCategory category,
        string title,
        string message,
        Guid? relatedMatchId = null,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? data = null)
    {
        foreach (var userId in userIds.Distinct())
        {
            await NotifyAsync(userId, category, title, message, relatedMatchId, cancellationToken, data);
        }
    }

    /// <summary>Fans a notification out to the user's PWA subscriptions, pruning any that expired.</summary>
    private async Task SendWebPushAsync(
        Guid userId,
        string title,
        string message,
        IReadOnlyDictionary<string, string> payload,
        CancellationToken cancellationToken)
    {
        if (!_webPush.IsConfigured)
        {
            return;
        }

        var subscriptions = await _webPushSubscriptions.GetForUserAsync(userId, cancellationToken);
        foreach (var subscription in subscriptions)
        {
            try
            {
                var recipient = new WebPushRecipient(subscription.Endpoint, subscription.P256dh, subscription.Auth);
                var result = await _webPush.SendAsync(recipient, title, message, payload, cancellationToken);
                if (result == WebPushResult.Expired)
                {
                    // The browser unsubscribed or the endpoint died; drop it so we stop trying.
                    _webPushSubscriptions.Remove(subscription);
                }
            }
            catch
            {
                // As with FCM, a push failure must never break the originating use case.
            }
        }
    }
}
