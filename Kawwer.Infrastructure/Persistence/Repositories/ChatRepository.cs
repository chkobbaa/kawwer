using Kawwer.Application.Common.Interfaces;
using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence.Repositories;

public sealed class ChatRepository : IChatRepository
{
    private readonly KawwerDbContext _context;

    public ChatRepository(KawwerDbContext context) => _context = context;

    public void Add(ChatMessage message) => _context.ChatMessages.Add(message);

    public Task<ChatMessage?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<ChatMessage> Items, int Total)> GetForMatchAsync(
        Guid matchId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.ChatMessages.Where(m => m.MatchId == matchId);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Return in chronological order for display.
        items.Reverse();
        return (items, total);
    }
}
