using Kawwer.Contracts.Matches;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Common.Mappings;

public static class LineupMappings
{
    public static GuestPlayerDto ToDto(this GuestPlayer guest) => new(
        guest.Id,
        guest.MatchId,
        guest.Name,
        guest.SkillLevel,
        guest.Team,
        guest.PositionX,
        guest.PositionY);

    /// <summary>
    /// Builds the full board for a match: the organizer (implicit player), every accepted player and
    /// every guest, each with their team and normalized position. <paramref name="users"/> must
    /// contain the organizer and every accepted participant.
    /// </summary>
    public static LineupDto ToLineupDto(this Match match, IReadOnlyDictionary<Guid, User> users)
    {
        var slots = new List<LineupSlotDto>();

        if (users.TryGetValue(match.OrganizerId, out var organizer))
        {
            slots.Add(new LineupSlotDto(
                LineupSlotKind.Organizer,
                organizer.Id,
                organizer.FullName,
                organizer.ProfilePictureUrl,
                organizer.Reputation,
                organizer.SkillLevel,
                IsGuest: false,
                match.OrganizerTeam,
                match.OrganizerPositionX,
                match.OrganizerPositionY));
        }

        foreach (var participant in match.Participants.Where(p => p.Status == ParticipantStatus.Accepted))
        {
            if (!users.TryGetValue(participant.UserId, out var user))
            {
                continue;
            }

            slots.Add(new LineupSlotDto(
                LineupSlotKind.Participant,
                user.Id,
                user.FullName,
                user.ProfilePictureUrl,
                user.Reputation,
                user.SkillLevel,
                IsGuest: false,
                participant.Team,
                participant.PositionX,
                participant.PositionY));
        }

        foreach (var guest in match.Guests)
        {
            slots.Add(new LineupSlotDto(
                LineupSlotKind.Guest,
                guest.Id,
                guest.Name,
                ProfilePictureUrl: null,
                Reputation: null,
                guest.SkillLevel,
                IsGuest: true,
                guest.Team,
                guest.PositionX,
                guest.PositionY));
        }

        return new LineupDto(match.Id, slots);
    }
}
