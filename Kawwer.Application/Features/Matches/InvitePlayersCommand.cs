using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

/// <summary>
/// Invites players to an existing match. When the requester is the organizer the players are
/// invited directly; when the requester is an accepted member the players are only SUGGESTED
/// and land in the organizer's pending list for confirmation.
/// </summary>
public sealed record InvitePlayersCommand(
    Guid RequesterId,
    Guid MatchId,
    IReadOnlyList<Guid> UserIds,
    IReadOnlyList<Guid> TeamIds) : IRequest<Unit>;

public sealed class InvitePlayersCommandHandler : IRequestHandler<InvitePlayersCommand, Unit>
{
    private readonly IMatchRepository _matches;
    private readonly ITeamRepository _teams;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;
    private readonly IUnitOfWork _unitOfWork;

    public InvitePlayersCommandHandler(
        IMatchRepository matches,
        ITeamRepository teams,
        IUserRepository users,
        INotificationService notifications,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _teams = teams;
        _users = users;
        _notifications = notifications;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> HandleAsync(InvitePlayersCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        var isOrganizer = match.OrganizerId == request.RequesterId;
        var isMember = match.Participants.Any(p => p.UserId == request.RequesterId
                                                   && p.Status == ParticipantStatus.Accepted);
        if (!isOrganizer && !isMember)
        {
            throw new ForbiddenException("Only the organizer or a player in the match can invite players.");
        }

        var inviteeIds = new HashSet<Guid>(request.UserIds);
        foreach (var teamId in request.TeamIds)
        {
            var team = await _teams.GetByIdAsync(teamId, cancellationToken);
            if (team is null || team.OwnerId != request.RequesterId)
            {
                continue;
            }

            foreach (var member in team.Members)
            {
                inviteeIds.Add(member.UserId);
            }
        }

        inviteeIds.Remove(match.OrganizerId);
        inviteeIds.Remove(request.RequesterId);

        // Skip anyone with an ACTIVE link to the match. Players who declined, left or were
        // removed can be re-invited (Match.Invite/Suggest resets their participation).
        var existing = match.Participants
            .Where(p => p.Status is ParticipantStatus.Invited
                        or ParticipantStatus.Seen
                        or ParticipantStatus.Thinking
                        or ParticipantStatus.Accepted
                        or ParticipantStatus.WaitingList)
            .Select(p => p.UserId)
            .ToHashSet();

        var newInvitees = inviteeIds.Where(id => !existing.Contains(id)).ToList();
        if (newInvitees.Count == 0)
        {
            return Unit.Value;
        }

        if (isOrganizer)
        {
            foreach (var userId in newInvitees)
            {
                match.Invite(userId);
            }
        }
        else
        {
            // Members only suggest: the players land in the organizer's PENDING list and are
            // added to the match once the organizer approves them.
            foreach (var userId in newInvitees)
            {
                match.Suggest(userId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notifications are best effort; the invitations above are already committed.
        try
        {
            if (isOrganizer)
            {
                await _notifications.NotifyManyAsync(
                    newInvitees,
                    NotificationCategory.Invitation,
                    "New match invitation",
                    $"You've been invited to \"{match.Title}\" on {match.MatchDate:dd MMM} at {match.StartTime:HH\\:mm}.",
                    match.Id,
                    cancellationToken,
                    // Tells the mobile app to render Accept/Decline action buttons on the push.
                    new Dictionary<string, string> { ["type"] = "match_invitation" });
            }
            else
            {
                var requester = await _users.GetByIdAsync(request.RequesterId, cancellationToken);
                var requesterName = requester?.FullName ?? "A player";
                var label = newInvitees.Count == 1 ? "a player" : $"{newInvitees.Count} players";
                await _notifications.NotifyAsync(
                    match.OrganizerId,
                    NotificationCategory.Invitation,
                    "Players suggested",
                    $"{requesterName} suggested {label} for \"{match.Title}\". Review them in the match.",
                    match.Id,
                    cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Never break committed invitations because of a notification problem.
        }

        return Unit.Value;
    }
}
