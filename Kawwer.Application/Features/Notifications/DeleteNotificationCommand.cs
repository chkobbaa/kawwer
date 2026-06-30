using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Notifications;

public sealed record DeleteNotificationCommand(Guid UserId, Guid NotificationId) : IRequest<Unit>;

public sealed class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Unit>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNotificationCommandHandler(INotificationRepository notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.NotificationId, cancellationToken)
                           ?? throw NotFoundException.For("Notification", request.NotificationId);

        if (notification.UserId != request.UserId)
        {
            throw new ForbiddenException("You cannot delete this notification.");
        }

        _notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
