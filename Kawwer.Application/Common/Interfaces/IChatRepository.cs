using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Interfaces;

public interface IChatRepository
{
    void Add(ChatMessage message);
    Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ChatMessage> Items, int Total)> GetForMatchAsync(
        Guid matchId, int page, int pageSize, CancellationToken cancellationToken = default);
}
