using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Group>> GetForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
    void Add(Group group);
    void Remove(Group group);
}
