using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface IFootballFieldRepository
{
    Task<FootballField?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<FootballField> Items, int Total)> SearchAsync(
        string? term, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FootballField>> GetNearbyAsync(
        decimal latitude, decimal longitude, double radiusKm, CancellationToken cancellationToken = default);
    void Add(FootballField field);
}
