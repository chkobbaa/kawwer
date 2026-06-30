using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class FriendshipRepository : IFriendshipRepository
{
    private readonly KawwerDbContext _context;

    public FriendshipRepository(KawwerDbContext context) => _context = context;

    public Task<Friendship?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Friendships.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public Task<Friendship?> GetBetweenAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
        => _context.Friendships.FirstOrDefaultAsync(
            f => (f.UserId == userA && f.FriendId == userB) || (f.UserId == userB && f.FriendId == userA),
            cancellationToken);

    public async Task<IReadOnlyList<Friendship>> GetAcceptedForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Friendships
            .Where(f => f.Status == FriendshipStatus.Accepted && (f.UserId == userId || f.FriendId == userId))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Friendship>> GetPendingIncomingAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Friendships
            .Where(f => f.Status == FriendshipStatus.Pending && f.FriendId == userId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Friendship>> GetPendingOutgoingAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Friendships
            .Where(f => f.Status == FriendshipStatus.Pending && f.UserId == userId)
            .ToListAsync(cancellationToken);

    public Task<bool> AreFriendsAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default)
        => _context.Friendships.AnyAsync(
            f => f.Status == FriendshipStatus.Accepted
                 && ((f.UserId == userA && f.FriendId == userB) || (f.UserId == userB && f.FriendId == userA)),
            cancellationToken);

    public Task<bool> IsBlockedAsync(Guid blocker, Guid blocked, CancellationToken cancellationToken = default)
        => _context.Friendships.AnyAsync(
            f => f.Status == FriendshipStatus.Blocked && f.UserId == blocker && f.FriendId == blocked,
            cancellationToken);

    public void Add(Friendship friendship) => _context.Friendships.Add(friendship);

    public void Remove(Friendship friendship) => _context.Friendships.Remove(friendship);
}
