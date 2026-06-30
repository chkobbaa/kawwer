using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Friendship?> GetBetweenAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Friendship>> GetAcceptedForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Friendship>> GetPendingIncomingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Friendship>> GetPendingOutgoingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AreFriendsAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);
    Task<bool> IsBlockedAsync(Guid blocker, Guid blocked, CancellationToken cancellationToken = default);
    void Add(Friendship friendship);
    void Remove(Friendship friendship);
}
