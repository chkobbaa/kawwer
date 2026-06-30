using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record UpdateMatchCommand(
    Guid OrganizerId,
    Guid MatchId,
    DateOnly MatchDate,
    TimeOnly StartTime,
    int DurationMinutes,
    string? Description,
    MatchVisibility Visibility) : IRequest<Unit>;

public sealed class UpdateMatchCommandHandler : IRequestHandler<UpdateMatchCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMatchCommandHandler(
        IMatchRepository matches,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(UpdateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can edit this match.");
        }

        match.Edit(request.MatchDate, request.StartTime, request.DurationMinutes, request.Description, request.Visibility);

        var affected = match.Participants
            .Where(p => p.Status is ParticipantStatus.Accepted or ParticipantStatus.WaitingList)
            .Select(p => p.UserId);

        await _notifications.NotifyManyAsync(
            affected,
            NotificationCategory.Match,
            "Match updated",
            $"\"{match.Title}\" has been updated. Check the new details.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
