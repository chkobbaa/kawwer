using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Common.Services;

/// <summary>
/// Default notification orchestration: persists an in-app <see cref="Notification"/> and pushes
/// to the user's registered device when a token is available. Push failures are swallowed so a
/// transient FCM error never breaks the originating use case.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;
    private readonly IUserRepository _users;
    private readonly IPushNotificationSender _push;

    public NotificationService(
        INotificationRepository notifications,
        IUserRepository users,
        IPushNotificationSender push)
    {
        _notifications = notifications;
        _users = users;
        _push = push;
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

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user?.DeviceToken is { Length: > 0 } token)
        {
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

            try
            {
                await _push.SendAsync(token, title, message, payload, cancellationToken);
            }
            catch
            {
                // A failed push must not roll back the use case; the in-app notification persists.
            }
        }
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
}
