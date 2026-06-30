using Kawwer.Contracts.Chat;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Mappings;

public static class ChatMappings
{
    public static ChatMessageDto ToDto(this ChatMessage message, string? senderName, bool isPinned) => new(
        message.Id,
        message.MatchId,
        message.SenderId,
        senderName,
        message.Type,
        message.IsDeleted ? "[deleted]" : message.Content,
        message.IsEdited,
        isPinned,
        message.CreatedAt);
}
