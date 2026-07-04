using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Matches;
using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Matches;

public sealed record GetMatchParticipantsQuery(Guid MatchId) : IRequest<IReadOnlyList<MatchParticipantDto>>;

public sealed class GetMatchParticipantsQueryHandler
    : IRequestHandler<GetMatchParticipantsQuery, IReadOnlyList<MatchParticipantDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;

    public GetMatchParticipantsQueryHandler(IMatchRepository matches, IUserRepository users)
    {
        _matches = matches;
        _users = users;
    }

    public async Task<IReadOnlyList<MatchParticipantDto>> HandleAsync(GetMatchParticipantsQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        // Load the organizer alongside the participants so we can surface them in the list even
        // though the organizer is modelled implicitly (they never hold a MatchParticipant row).
        var userIds = match.Participants.Select(p => p.UserId).ToList();
        if (!userIds.Contains(match.OrganizerId))
        {
            userIds.Add(match.OrganizerId);
        }

        var users = (await _users.GetByIdsAsync(userIds, cancellationToken)).ToDictionary(u => u.Id);

        var result = new List<MatchParticipantDto>();

        // The organizer is always "in" and heads the players list. We synthesize the row here in the
        // read model instead of persisting a real participant, because a real Accepted participant
        // would double-count against capacity and pollute payment collection, attendance and the
        // player-statistics/reputation pipelines (all of which treat the organizer as implicit).
        if (users.TryGetValue(match.OrganizerId, out var organizer)
            && match.Participants.All(p => p.UserId != match.OrganizerId))
        {
            result.Add(new MatchParticipantDto(
                Id: Guid.Empty,
                User: organizer.ToSummaryDto(),
                Status: ParticipantStatus.Accepted,
                IsJoinRequest: false,
                WaitingListPosition: null,
                PaidAmount: 0m,
                PaymentStatus: PaymentStatus.NotPaid,
                Attendance: AttendanceStatus.Unknown,
                SharedLocation: false,
                Latitude: null,
                Longitude: null,
                RespondedAt: match.CreatedAt,
                InvitedAt: match.CreatedAt));
        }

        result.AddRange(match.Participants
            .Where(p => users.ContainsKey(p.UserId))
            .OrderBy(p => p.WaitingListPosition ?? 0)
            .Select(p => p.ToDto(users[p.UserId])));

        return result;
    }
}
