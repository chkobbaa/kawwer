using Kawwer.Application.Common.Interfaces;

namespace Kawwer.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly KawwerDbContext _context;

    public UnitOfWork(KawwerDbContext context) => _context = context;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
