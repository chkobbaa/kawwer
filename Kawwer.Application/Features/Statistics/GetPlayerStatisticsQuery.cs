using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Statistics;
using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Statistics;

public sealed record GetPlayerStatisticsQuery(Guid UserId) : IRequest<PlayerStatisticsDto>;

public sealed class GetPlayerStatisticsQueryHandler : IRequestHandler<GetPlayerStatisticsQuery, PlayerStatisticsDto>
{
    private readonly IUserRepository _users;
    private readonly IMatchRepository _matches;
    private readonly IRatingRepository _ratings;
    private readonly IFriendshipRepository _friendships;
    private readonly IGroupRepository _groups;

    public GetPlayerStatisticsQueryHandler(
        IUserRepository users,
        IMatchRepository matches,
        IRatingRepository ratings,
        IFriendshipRepository friendships,
        IGroupRepository groups)
    {
        _users = users;
        _matches = matches;
        _ratings = ratings;
        _friendships = friendships;
        _groups = groups;
    }

    public async Task<PlayerStatisticsDto> HandleAsync(GetPlayerStatisticsQuery request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
                   ?? throw NotFoundException.For("User", request.UserId);

        var participations = await _matches.GetForUserParticipationAsync(request.UserId, cancellationToken);
        var organized = await _matches.GetForOrganizerAsync(request.UserId, cancellationToken);
        var ratings = await _ratings.GetForRateeAsync(request.UserId, cancellationToken);
        var friends = await _friendships.GetAcceptedForUserAsync(request.UserId, cancellationToken);
        var groups = await _groups.GetForOwnerAsync(request.UserId, cancellationToken);

        var myParticipant = participations
            .Select(m => m.Participants.FirstOrDefault(p => p.UserId == request.UserId))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();

        var invitationsReceived = myParticipant.Count;
        var invitationsAccepted = myParticipant.Count(p => p.Status is ParticipantStatus.Accepted or ParticipantStatus.WaitingList);
        var invitationsDeclined = myParticipant.Count(p => p.Status == ParticipantStatus.Declined);
        var publicJoined = myParticipant.Count(p => p.IsJoinRequest && p.Status == ParticipantStatus.Accepted);

        var finishedAccepted = participations
            .Where(m => m.Status == MatchStatus.Finished)
            .Select(m => m.Participants.FirstOrDefault(p => p.UserId == request.UserId && p.Status == ParticipantStatus.Accepted))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();

        var onTime = finishedAccepted.Count(p => p.Attendance == AttendanceStatus.Present);
        var late = finishedAccepted.Count(p => p.Attendance == AttendanceStatus.Late);
        var noShows = finishedAccepted.Count(p => p.Attendance == AttendanceStatus.NoShow);
        var attended = onTime + late;
        var attendanceRate = finishedAccepted.Count > 0 ? (double)attended / finishedAccepted.Count : 0d;

        var paymentsCompleted = finishedAccepted.Count(p => p.PaymentCompleted);
        var paymentReliability = finishedAccepted.Count > 0 ? (double)paymentsCompleted / finishedAccepted.Count : 0d;

        var lateCancellations = myParticipant.Count(p => p.Status == ParticipantStatus.Cancelled);

        var averageRating = Average(ratings.Select(r => r.Stars));
        var organizerRating = Average(ratings.Where(r => r.Type == RatingType.Organizer).Select(r => r.Stars));
        var playerRating = Average(ratings.Where(r => r.Type == RatingType.Player).Select(r => r.Stars));

        return new PlayerStatisticsDto(
            user.Id,
            MatchesPlayed: attended,
            MatchesOrganized: organized.Count,
            InvitationsReceived: invitationsReceived,
            InvitationsAccepted: invitationsAccepted,
            InvitationsDeclined: invitationsDeclined,
            PublicMatchesJoined: publicJoined,
            AttendanceRate: attendanceRate,
            OnTimeArrivals: onTime,
            LateArrivals: late,
            NoShows: noShows,
            LateCancellations: lateCancellations,
            PaymentReliability: paymentReliability,
            PaymentsCompleted: paymentsCompleted,
            AverageRating: averageRating,
            OrganizerRating: organizerRating,
            PlayerRating: playerRating,
            Reputation: user.Reputation,
            ReliabilityBadge: user.GetReliabilityBadge(),
            Friends: friends.Count,
            GroupsCreated: groups.Count);
    }

    private static decimal Average(IEnumerable<int> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0m : Math.Round((decimal)list.Average(), 2);
    }
}
