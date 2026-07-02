using Kawwer.Domain.Enums;

namespace Kawwer.Application.Common.Interfaces;

/// <summary>
/// Creates a persistent in-app notification and dispatches the matching push notification.
/// The caller is responsible for committing the unit of work.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(
        Guid userId,
        NotificationCategory category,
        string title,
        string message,
        Guid? relatedMatchId = null,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? data = null);

    Task NotifyManyAsync(
        IEnumerable<Guid> userIds,
        NotificationCategory category,
        string title,
        string message,
        Guid? relatedMatchId = null,
        CancellationToken cancellationToken = default,
        IReadOnlyDictionary<string, string>? data = null);
}
