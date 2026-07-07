using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Services;

namespace Kawwer.Application.Features.Lineup;

/// <summary>
/// Splits every accepted player, guest and the organizer into two balanced teams and gives each a
/// starting position, weighing skill level and reputation. Organizer only. Returns the fresh board so
/// the caller can render it immediately; the organizer can then drag players to fine-tune.
/// </summary>
public sealed record AutoBalanceLineupCommand(Guid RequesterId, Guid MatchId) : IRequest<LineupDto>;

public sealed class AutoBalanceLineupCommandHandler : IRequestHandler<AutoBalanceLineupCommand, LineupDto>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;
    private readonly IRealtimeNotifier _realtime;
    private readonly IUnitOfWork _unitOfWork;

    public AutoBalanceLineupCommandHandler(
        IMatchRepository matches,
        IUserRepository users,
        IRealtimeNotifier realtime,
        IUnitOfWork unitOfWork)
    {
        _matches = matches;
        _users = users;
        _realtime = realtime;
        _unitOfWork = unitOfWork;
    }

    public async Task<LineupDto> HandleAsync(AutoBalanceLineupCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.RequesterId)
        {
            throw new ForbiddenException("Only the organizer can auto-balance the teams.");
        }

        var acceptedParticipants = match.Participants
            .Where(p => p.Status == ParticipantStatus.Accepted)
            .ToList();

        var userIds = acceptedParticipants
            .Select(p => p.UserId)
            .Append(match.OrganizerId)
            .Distinct()
            .ToList();
        var users = (await _users.GetByIdsAsync(userIds, cancellationToken)).ToDictionary(u => u.Id);

        // Build candidates in a fixed order so placements can be mapped back by index.
        var entries = new List<(LineupSlotKind Kind, Guid Id)>();
        var candidates = new List<BalanceCandidate>();

        if (users.TryGetValue(match.OrganizerId, out var organizer))
        {
            entries.Add((LineupSlotKind.Organizer, organizer.Id));
            candidates.Add(new BalanceCandidate(organizer.SkillLevel, organizer.Reputation));
        }

        foreach (var participant in acceptedParticipants)
        {
            if (!users.TryGetValue(participant.UserId, out var user))
            {
                continue;
            }

            entries.Add((LineupSlotKind.Participant, user.Id));
            candidates.Add(new BalanceCandidate(user.SkillLevel, user.Reputation));
        }

        foreach (var guest in match.Guests)
        {
            entries.Add((LineupSlotKind.Guest, guest.Id));
            // Guests have no reputation of their own, so they balance on skill around a neutral value.
            candidates.Add(new BalanceCandidate(guest.SkillLevel, LineupBalancer.NeutralReputation));
        }

        var placements = LineupBalancer.Balance(candidates);
        foreach (var placement in placements)
        {
            var (kind, id) = entries[placement.Index];
            switch (kind)
            {
                case LineupSlotKind.Organizer:
                    match.PlaceOrganizerInLineup(placement.Team, placement.PositionX, placement.PositionY);
                    break;
                case LineupSlotKind.Participant:
                    match.PlaceParticipantInLineup(id, placement.Team, placement.PositionX, placement.PositionY);
                    break;
                case LineupSlotKind.Guest:
                    match.PlaceGuestInLineup(id, placement.Team, placement.PositionX, placement.PositionY);
                    break;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _realtime.MatchUpdatedAsync(match.Id, cancellationToken);
        return match.ToLineupDto(users);
    }
}
