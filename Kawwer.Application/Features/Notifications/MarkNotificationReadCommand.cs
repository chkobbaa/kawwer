using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;

namespace Kawwer.Application.Features.Notifications;

public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId) : IRequest<Unit>;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(INotificationRepository notifications, IUnitOfWork unitOfWork)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notifications.GetByIdAsync(request.NotificationId, cancellationToken)
                           ?? throw NotFoundException.For("Notification", request.NotificationId);

        if (notification.UserId != request.UserId)
        {
            throw new ForbiddenException("You cannot modify this notification.");
        }

        notification.MarkRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
