using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Statistics;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Statistics;

public sealed record GetOrganizerStatisticsQuery(Guid UserId) : IRequest<OrganizerStatisticsDto>;

public sealed class GetOrganizerStatisticsQueryHandler
    : IRequestHandler<GetOrganizerStatisticsQuery, OrganizerStatisticsDto>
{
    private readonly IUserRepository _users;
    private readonly IMatchRepository _matches;
    private readonly IRatingRepository _ratings;

    public GetOrganizerStatisticsQueryHandler(IUserRepository users, IMatchRepository matches, IRatingRepository ratings)
    {
        _users = users;
        _matches = matches;
        _ratings = ratings;
    }

    public async Task<OrganizerStatisticsDto> HandleAsync(GetOrganizerStatisticsQuery request, CancellationToken cancellationToken)
    {
        _ = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw NotFoundException.For("User", request.UserId);

        var organized = await _matches.GetForOrganizerAsync(request.UserId, cancellationToken);
        var ratings = await _ratings.GetForRateeAsync(request.UserId, cancellationToken);

        var completed = organized.Where(m => m.Status == MatchStatus.Finished).ToList();
        var cancelled = organized.Count(m => m.Status == MatchStatus.Cancelled);

        double averageAttendance = 0d;
        if (completed.Count > 0)
        {
            var rates = completed.Select(m =>
            {
                var accepted = m.Participants.Count(p => p.Status == ParticipantStatus.Accepted);
                if (accepted == 0)
                {
                    return 0d;
                }

                var present = m.Participants.Count(p =>
                    p.Status == ParticipantStatus.Accepted &&
                    p.Attendance is AttendanceStatus.Present or AttendanceStatus.Late);
                return (double)present / accepted;
            });
            averageAttendance = rates.Average();
        }

        var organizerRating = Average(ratings.Where(r => r.Type == RatingType.Organizer).Select(r => r.Stars));
        var playerRating = Average(ratings.Where(r => r.Type == RatingType.Player).Select(r => r.Stars));

        return new OrganizerStatisticsDto(
            request.UserId,
            MatchesOrganized: organized.Count,
            MatchesCompleted: completed.Count,
            MatchesCancelled: cancelled,
            AverageAttendance: averageAttendance,
            AveragePlayerRating: playerRating,
            AverageOrganizerRating: organizerRating);
    }

    private static decimal Average(IEnumerable<int> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? 0m : Math.Round((decimal)list.Average(), 2);
    }
}
