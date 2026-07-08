using Kawwer.Contracts.Notifications;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Mappings;

public static class NotificationMappings
{
    public static NotificationDto ToDto(this Notification notification) => new(
        notification.Id,
        notification.Category,
        notification.Title,
        notification.Message,
        notification.RelatedMatchId,
        notification.IsRead,
        notification.CreatedAt,
        notification.Type,
        notification.RelatedFriendshipId,
        notification.Important);
}
