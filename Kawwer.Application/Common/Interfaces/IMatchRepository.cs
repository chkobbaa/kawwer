using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface IMatchRepository
{
    /// <summary>Loads a match including its participants.</summary>
    Task<Match?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetForOrganizerAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetUpcomingForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Match>> GetForUserParticipationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Match> Items, int Total)> GetPublicAsync(
        DateOnly? dateFrom, DateOnly? dateTo, int page, int pageSize, CancellationToken cancellationToken = default);
    void Add(Match match);
}
