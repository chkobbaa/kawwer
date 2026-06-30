using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class RatingRepository : IRatingRepository
{
    private readonly KawwerDbContext _context;

    public RatingRepository(KawwerDbContext context) => _context = context;

    public void Add(Rating rating) => _context.Ratings.Add(rating);

    public Task<bool> ExistsAsync(Guid matchId, Guid raterId, Guid rateeId, RatingType type, CancellationToken cancellationToken = default)
        => _context.Ratings.AnyAsync(
            r => r.MatchId == matchId && r.RaterId == raterId && r.RateeId == rateeId && r.Type == type,
            cancellationToken);

    public async Task<IReadOnlyList<Rating>> GetForRateeAsync(Guid rateeId, CancellationToken cancellationToken = default)
        => await _context.Ratings.Where(r => r.RateeId == rateeId).ToListAsync(cancellationToken);
}
