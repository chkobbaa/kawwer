using Kawwer.Contracts.Matches;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Common.Mappings;

public static class MatchMappings
{
    public static MatchDto ToDto(this Match match, FootballField field, User organizer) => new(
        match.Id,
        match.OrganizerId,
        match.Title,
        match.Description,
        match.Visibility,
        match.Status,
        match.Format,
        match.Sport,
        match.OpponentName,
        match.OpponentTeamId,
        match.MatchDate,
        match.StartTime,
        match.EndTime,
        match.DurationMinutes,
        match.MaxPlayers,
        match.AcceptedCount,
        match.WaitingCount,
        match.TotalFieldPrice,
        match.ReservationPaid,
        match.RemainingAmount,
        match.SharePerPlayer,
        match.CollectedAmount,
        match.MissingAmount,
        match.PaymentCollectionStarted,
        match.PaymentCompleted,
        match.LiveMatchStarted,
        field.ToDto(),
        organizer.ToSummaryDto(),
        match.CreatedAt);

    public static MatchParticipantDto ToDto(this MatchParticipant participant, User user) => new(
        participant.Id,
        user.ToSummaryDto(),
        participant.Status,
        participant.IsJoinRequest,
        participant.WaitingListPosition,
        participant.PaidAmount,
        participant.PaymentStatus,
        participant.Attendance,
        participant.SharedLocation,
        participant.Latitude,
        participant.Longitude,
        participant.RespondedAt,
        participant.InvitedAt);

    public static OrganizerDashboardItemDto ToDashboardItem(this Match match, string fieldName) => new(
        match.Id,
        match.Title,
        match.MatchDate,
        match.StartTime,
        fieldName,
        match.AcceptedCount,
        match.WaitingCount,
        match.Participants.Count(p => p.Status == ParticipantStatus.Thinking),
        match.Participants.Count(p => p.Status == ParticipantStatus.Declined),
        match.MissingAmount,
        match.Status);
}
