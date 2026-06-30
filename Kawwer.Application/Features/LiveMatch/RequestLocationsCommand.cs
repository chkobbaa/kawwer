using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.LiveMatch;

/// <summary>Organizer asks accepted players to share their live location.</summary>
public sealed record RequestLocationsCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class RequestLocationsCommandHandler : IRequestHandler<RequestLocationsCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public RequestLocationsCommandHandler(
        IMatchRepository matches,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(RequestLocationsCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can request live locations.");
        }

        var accepted = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .Select(p => p.UserId);

        await _notifications.NotifyManyAsync(
            accepted,
            NotificationCategory.LiveMatch,
            "Location requested",
            $"The organizer of \"{match.Title}\" asked you to share your live location.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
