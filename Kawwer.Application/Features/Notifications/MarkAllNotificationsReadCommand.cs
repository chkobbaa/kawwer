using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Notifications;

public sealed record MarkAllNotificationsReadCommand(Guid UserId) : IRequest<Unit>;

public sealed class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public MarkAllNotificationsReadCommandHandler(INotificationRepository notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        await _notifications.MarkAllReadAsync(request.UserId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
