using Kawwer.Domain.Common;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A single cash payment recorded by an organizer against a match participant.
/// Provides the immutable payment history.
/// </summary>
public class PaymentRecord : Entity
{
    private PaymentRecord()
    {
    }

    public PaymentRecord(Guid matchId, Guid payerId, Guid recordedBy, decimal amount)
    {
        if (amount < 0m)
        {
            throw new DomainException("Payment amount cannot be negative.");
        }

        Id = Guid.NewGuid();
        MatchId = matchId;
        PayerId = payerId;
        RecordedBy = recordedBy;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid MatchId { get; private set; }
    public Guid PayerId { get; private set; }
    public Guid RecordedBy { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
