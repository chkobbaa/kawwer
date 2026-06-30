using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

/// <summary>
/// Organizer marks the match finished. Attendance feeds reputation (present rewards, late/no-show
/// penalties) and ratings open for participants.
/// </summary>
public sealed record FinishMatchCommand(Guid OrganizerId, Guid MatchId) : IRequest<Unit>;

public sealed class FinishMatchCommandHandler : IRequestHandler<FinishMatchCommand, Unit>
{
    private const decimal PresentReward = 1m;
    private const decimal LatePenalty = -3m;
    private const decimal NoShowPenalty = -10m;

    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public FinishMatchCommandHandler(
        IMatchRepository matches,
        IUserRepository users,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _users = users;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(FinishMatchCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can finish this match.");
        }

        match.Finish();

        var accepted = match.Participants.Where(p => p.Status == ParticipantStatus.Accepted).ToList();
        var ids = accepted.Select(p => p.UserId).ToList();
        var users = (await _users.GetByIdsAsync(ids, cancellationToken)).ToDictionary(u => u.Id);

        foreach (var participant in accepted)
        {
            if (!users.TryGetValue(participant.UserId, out var user))
            {
                continue;
            }

            var delta = participant.Attendance switch
            {
                AttendanceStatus.Present => PresentReward,
                AttendanceStatus.Late => LatePenalty,
                AttendanceStatus.NoShow => NoShowPenalty,
                _ => 0m
            };

            if (delta != 0m)
            {
                user.AdjustReputation(delta);
            }
        }

        await _notifications.NotifyManyAsync(
            ids,
            NotificationCategory.Match,
            "Rate your match",
            $"\"{match.Title}\" has finished. Rate the organizer and players within 7 days.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
