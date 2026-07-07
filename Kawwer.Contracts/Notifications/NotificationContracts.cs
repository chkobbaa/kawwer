using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Notifications;

public sealed record NotificationDto(
    Guid Id,
    NotificationCategory Category,
    string Title,
    string Message,
    Guid? RelatedMatchId,
    bool IsRead,
    DateTime CreatedAt,
    string? Type = null,
    Guid? RelatedFriendshipId = null,
    bool Important = false);
