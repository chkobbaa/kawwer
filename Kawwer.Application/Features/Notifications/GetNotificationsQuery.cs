using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Common;
using Kawwer.Contracts.Notifications;

namespace Kawwer.Application.Features.Notifications;

public sealed record GetNotificationsQuery(Guid UserId, bool UnreadOnly, int Page, int PageSize)
    : IRequest<PagedResult<NotificationDto>>;

public sealed class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly INotificationRepository _notifications;

    public GetNotificationsQueryHandler(INotificationRepository notifications) => _notifications = notifications;

    public async Task<PagedResult<NotificationDto>> HandleAsync(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _notifications.GetForUserAsync(
            request.UserId, request.UnreadOnly, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<NotificationDto>(
            items.Select(n => n.ToDto()).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
