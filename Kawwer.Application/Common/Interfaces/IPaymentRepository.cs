using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface IPaymentRepository
{
    void Add(PaymentRecord record);
    Task<IReadOnlyList<PaymentRecord>> GetForMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
}
