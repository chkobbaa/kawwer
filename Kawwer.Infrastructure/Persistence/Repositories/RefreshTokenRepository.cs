using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly KawwerDbContext _context;

    public RefreshTokenRepository(KawwerDbContext context) => _context = context;

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        => _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public void Add(RefreshToken token) => _context.RefreshTokens.Add(token);
}
