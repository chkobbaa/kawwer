using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Chat;

public sealed record SendMessageRequest(string Content);

public sealed record EditMessageRequest(string Content);

public sealed record ChatMessageDto(
    Guid Id,
    Guid MatchId,
    Guid? SenderId,
    string? SenderName,
    ChatMessageType Type,
    string Content,
    bool IsEdited,
    bool IsPinned,
    DateTime CreatedAt);
