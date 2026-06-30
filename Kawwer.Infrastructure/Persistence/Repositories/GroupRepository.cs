using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class GroupRepository : IGroupRepository
{
    private readonly KawwerDbContext _context;

    public GroupRepository(KawwerDbContext context) => _context = context;

    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Group>> GetForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
        => await _context.Groups
            .Include(g => g.Members)
            .Where(g => g.OwnerId == ownerId)
            .OrderBy(g => g.Name)
            .ToListAsync(cancellationToken);

    public void Add(Group group) => _context.Groups.Add(group);

    public void Remove(Group group) => _context.Groups.Remove(group);
}
