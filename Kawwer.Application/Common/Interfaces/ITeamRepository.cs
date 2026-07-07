using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface ITeamRepository
{
    Task<Team?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Team>> GetForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    void Add(Team team);
    void Remove(Team team);
}
