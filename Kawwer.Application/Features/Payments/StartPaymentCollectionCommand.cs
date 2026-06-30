using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Payments;

public sealed record StartPaymentCollectionCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class StartPaymentCollectionCommandHandler : IRequestHandler<StartPaymentCollectionCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IChatRepository _chat;
    private readonly INotificationService _notifications;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public StartPaymentCollectionCommandHandler(
        IMatchRepository matches,
        IChatRepository chat,
        INotificationService notifications,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _chat = chat;
        _notifications = notifications;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(StartPaymentCollectionCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can collect money.");
        }

        match.StartPaymentCollection();
        _chat.Add(ChatMessage.CreateSystemMessage(match.Id, "Payment collection has started."));

        var accepted = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .Select(p => p.UserId);

        await _notifications.NotifyManyAsync(
            accepted,
            NotificationCategory.Payment,
            "Payment collection started",
            $"Your share for \"{match.Title}\" is {match.SharePerPlayer} TND.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.PaymentUpdatedAsync(match.Id, cancellationToken);
        return Unit.Value;
    }
}
