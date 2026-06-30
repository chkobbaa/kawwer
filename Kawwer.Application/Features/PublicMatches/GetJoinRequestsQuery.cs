using Kawwer.Application.Common.Exceptions;
using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.PublicMatches;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.PublicMatches;

/// <summary>Pending public join requests awaiting the organizer's decision.</summary>
public sealed record GetJoinRequestsQuery(Guid OrganizerId, Guid MatchId) : IRequest<IReadOnlyList<JoinMatchRequestDto>>;

public sealed class GetJoinRequestsQueryHandler : IRequestHandler<GetJoinRequestsQuery, IReadOnlyList<JoinMatchRequestDto>>
{
    private readonly IMatchRepository _matches;
    private readonly IUserRepository _users;

    public GetJoinRequestsQueryHandler(IMatchRepository matches, IUserRepository users)
    {
        _matches = matches;
        _users = users;
    }

    public async Task<IReadOnlyList<JoinMatchRequestDto>> HandleAsync(GetJoinRequestsQuery request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken)
                    ?? throw NotFoundException.For("Match", request.MatchId);

        if (match.OrganizerId != request.OrganizerId)
        {
            throw new ForbiddenException("Only the organizer can view join requests.");
        }

        var pending = match.Participants
            .Where(p => p.IsJoinRequest && p.Status is ParticipantStatus.Invited or ParticipantStatus.Seen or ParticipantStatus.Thinking)
            .ToList();

        var users = (await _users.GetByIdsAsync(pending.Select(p => p.UserId).ToList(), cancellationToken))
            .ToDictionary(u => u.Id);

        return pending
            .Where(p => users.ContainsKey(p.UserId))
            .Select(p => new JoinMatchRequestDto(match.Id, users[p.UserId].ToSummaryDto(), p.InvitedAt))
            .ToList();
    }
}
