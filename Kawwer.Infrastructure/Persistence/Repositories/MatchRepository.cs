using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class MatchRepository : IMatchRepository
{
    private readonly KawwerDbContext _context;

    public MatchRepository(KawwerDbContext context) => _context = context;

    public Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Matches
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Match>> GetForOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default)
        => await _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.OrganizerId == organizerId)
            .OrderByDescending(m => m.MatchDate)
            .ThenByDescending(m => m.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Match>> GetUpcomingForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.MatchDate >= today
                        && m.Status != MatchStatus.Cancelled
                        && m.Status != MatchStatus.Finished
                        && (m.OrganizerId == userId
                            || m.Participants.Any(p => p.UserId == userId && p.Status == ParticipantStatus.Accepted)))
            .OrderBy(m => m.MatchDate)
            .ThenBy(m => m.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Match>> GetForUserParticipationAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Matches
            .Include(m => m.Participants)
            .Where(m => m.Participants.Any(p => p.UserId == userId))
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<Match> Items, int Total)> GetPublicAsync(
        DateOnly? dateFrom, DateOnly? dateTo, IReadOnlyCollection<Guid> friendOrganizerIds,
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var from = dateFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var friendIds = friendOrganizerIds.ToList();

        var query = _context.Matches
            .Include(m => m.Participants)
            .Where(m => (m.Visibility == MatchVisibility.Public
                         || (m.Visibility == MatchVisibility.FriendsOnly && friendIds.Contains(m.OrganizerId)))
                        && (m.Status == MatchStatus.Published || m.Status == MatchStatus.Full)
                        && m.MatchDate >= from);

        if (dateTo.HasValue)
        {
            query = query.Where(m => m.MatchDate <= dateTo.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(m => m.MatchDate)
            .ThenBy(m => m.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public void Add(Match match) => _context.Matches.Add(match);
}
