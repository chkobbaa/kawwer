using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface INotificationRepository
{
    void Add(Notification notification);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Notification> Items, int Total)> GetForUserAsync(
        Guid userId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
    void Remove(Notification notification);

    /// <summary>Removes all notifications of a category linked to a match (e.g. stale invitations after a cancellation).</summary>
    Task RemoveForMatchAsync(Guid matchId, Domain.Enums.NotificationCategory category, CancellationToken cancellationToken = default);
}
