namespace Kawwer.Application.Common.Interfaces;

/// <summary>Commits all pending changes tracked across repositories in a single transaction.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
