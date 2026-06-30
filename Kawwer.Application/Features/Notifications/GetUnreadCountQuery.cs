using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Notifications;

public sealed record GetUnreadCountQuery(Guid UserId) : IRequest<int>;

public sealed class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notifications;

    public GetUnreadCountQueryHandler(INotificationRepository notifications) => _notifications = notifications;

    public Task<int> HandleAsync(GetUnreadCountQuery request, CancellationToken cancellationToken)
        => _notifications.GetUnreadCountAsync(request.UserId, cancellationToken);
}
