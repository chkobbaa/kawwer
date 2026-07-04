using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly KawwerDbContext _context;

    public PushSubscriptionRepository(KawwerDbContext context) => _context = context;

    public void Add(PushSubscription subscription) => _context.PushSubscriptions.Add(subscription);

    public void Remove(PushSubscription subscription) => _context.PushSubscriptions.Remove(subscription);

    public Task<PushSubscription?> GetByEndpointAsync(string endpoint, CancellationToken cancellationToken = default)
        => _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint, cancellationToken);

    public async Task<IReadOnlyList<PushSubscription>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.PushSubscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);
}
