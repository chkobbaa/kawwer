using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class TeamRepository : ITeamRepository
{
    private readonly KawwerDbContext _context;

    public TeamRepository(KawwerDbContext context) => _context = context;

    public Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Team>> GetForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.Teams
            .Include(t => t.Members)
            .Where(t => t.OwnerId == ownerId)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

    public void Add(Team team) => _context.Teams.Add(team);

    public void Remove(Team team) => _context.Teams.Remove(team);
}
