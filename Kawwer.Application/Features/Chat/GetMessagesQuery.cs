using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Chat;
using Kawwer.Contracts.Common;

namespace Kawwer.Application.Features.Chat;

public sealed record GetMessagesQuery(Guid MatchId, int Page, int PageSize) : IRequest<PagedResult<ChatMessageDto>>;

public sealed class GetMessagesQueryHandler : IRequestHandler<GetMessagesQuery, PagedResult<ChatMessageDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly IUserRepository _users;

    public GetMessagesQueryHandler(IMatchRepository matches, IChatRepository chat, IUserRepository users)
    {
        _matches = matches;
        _chat = chat;
        _users = users;
    }

    public async Task<PagedResult<ChatMessageDto>> HandleAsync(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var (items, total) = await _chat.GetForMatchAsync(request.MatchId, request.Page, request.PageSize, cancellationToken);

        var senderIds = items.Where(m => m.SenderId.HasValue).Select(m => m.SenderId!.Value).Distinct().ToList();
        var users = (await _users.GetByIdsAsync(senderIds, cancellationToken)).ToDictionary(u => u.Id);

        var dtos = items
            .Select(m =>
            {
                var name = m.SenderId.HasValue && users.TryGetValue(m.SenderId.Value, out var u) ? u.FullName : null;
                return m.ToDto(name, isPinned: match.PinnedMessageId == m.Id);
            })
            .ToList();

        return new PagedResult<ChatMessageDto>(dtos, request.Page, request.PageSize, total);
    }
}
