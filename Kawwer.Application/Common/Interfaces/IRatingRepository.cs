using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Common.Interfaces;

public interface IRatingRepository
{
    void Add(Rating rating);
    Task<bool> ExistsAsync(Guid matchId, Guid raterId, Guid rateeId, RatingType type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Rating>> GetForRateeAsync(Guid rateeId, CancellationToken cancellationToken = default);
}
