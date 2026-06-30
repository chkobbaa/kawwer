using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record InvitePlayersCommand(
    Guid OrganizerId,
    Guid MatchId,
    IReadOnlyList<Guid> UserIds,
    IReadOnlyList<Guid> GroupIds) : IRequest<Unit>;

public sealed class InvitePlayersCommandHandler : IRequestHandler<InvitePlayersCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly IGroupRepository _groups;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public InvitePlayersCommandHandler(
        IMatchRepository matches,
        IGroupRepository groups,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _groups = groups;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(InvitePlayersCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can invite players.");
        }

        var inviteeIds = new HashSet<Guid>(request.UserIds);
        foreach (var groupId in request.GroupIds)
        {
            var group = await _groups.GetByIdAsync(groupId, cancellationToken);
            if (group is null || group.OwnerId != request.OrganizerId)
            {
                continue;
            }

            foreach (var member in group.Members)
            {
                inviteeIds.Add(member.UserId);
            }
        }

        inviteeIds.Remove(match.OrganizerId);

        // Skip anyone already linked to the match to keep invitations unique.
        var existing = match.Participants
            .Where(p => p.Status != ParticipantStatus.Removed)
            .Select(p => p.UserId)
            .ToHashSet();

        var newInvitees = inviteeIds.Where(id => !existing.Contains(id)).ToList();
        foreach (var userId in newInvitees)
        {
            match.Invite(userId);
        }

        await _notifications.NotifyManyAsync(
            newInvitees,
            NotificationCategory.Invitation,
            "New match invitation",
            $"You've been invited to \"{match.Title}\" on {match.MatchDate:dd MMM} at {match.StartTime:HH\\:mm}.",
            match.Id,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
