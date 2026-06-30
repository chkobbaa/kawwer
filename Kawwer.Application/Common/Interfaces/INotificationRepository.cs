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
}
