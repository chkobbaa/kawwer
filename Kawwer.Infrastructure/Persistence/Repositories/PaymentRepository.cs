using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly KawwerDbContext _context;

    public PaymentRepository(KawwerDbContext context) => _context = context;

    public void Add(PaymentRecord record) => _context.PaymentRecords.Add(record);

    public async Task<IReadOnlyList<PaymentRecord>> GetForMatchAsync(Guid matchId, CancellationToken cancellationToken = default)
        => await _context.PaymentRecords
            .Where(p => p.MatchId == matchId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
}
