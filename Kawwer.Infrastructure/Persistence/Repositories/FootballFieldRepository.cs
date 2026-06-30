using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class FootballFieldRepository : IFootballFieldRepository
{
    private readonly KawwerDbContext _context;

    public FootballFieldRepository(KawwerDbContext context) => _context = context;

    public Task<FootballField?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.FootballFields.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<FootballField> Items, int Total)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.FootballFields.AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var pattern = $"%{term.Trim()}%";
            query = query.Where(f => EF.Functions.ILike(f.Name, pattern) || EF.Functions.ILike(f.Address, pattern));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(f => f.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<FootballField>> GetNearbyAsync(
        decimal latitude, decimal longitude, double radiusKm, CancellationToken cancellationToken = default)
    {
        // Filtering by exact distance is done in memory; PostGIS could optimise this later.
        var all = await _context.FootballFields.ToListAsync(cancellationToken);
        return all
            .Where(f => GeoUtils.DistanceKm(latitude, longitude, f.Latitude, f.Longitude) <= radiusKm)
            .OrderBy(f => GeoUtils.DistanceKm(latitude, longitude, f.Latitude, f.Longitude))
            .ToList();
    }

    public void Add(FootballField field) => _context.FootballFields.Add(field);
}
