using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Friends;

namespace Kawwer.Application.Features.Friends;

/// <summary>Returns both incoming and outgoing pending friend requests for the user.</summary>
public sealed record GetFriendRequestsQuery(Guid UserId) : IRequest<IReadOnlyList<FriendRequestDto>>;

public sealed class GetFriendRequestsQueryHandler : IRequestHandler<GetFriendRequestsQuery, IReadOnlyList<FriendRequestDto>>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUserRepository _users;

    public GetFriendRequestsQueryHandler(IFriendshipRepository friendships, IUserRepository users)
    {
        _friendships = friendships;
        _users = users;
    }

    public async Task<IReadOnlyList<FriendRequestDto>> HandleAsync(GetFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var incoming = await _friendships.GetPendingIncomingAsync(request.UserId, cancellationToken);
        var outgoing = await _friendships.GetPendingOutgoingAsync(request.UserId, cancellationToken);

        var otherIds = incoming.Select(f => f.UserId)
            .Concat(outgoing.Select(f => f.FriendId))
            .Distinct()
            .ToList();

        var users = (await _users.GetByIdsAsync(otherIds, cancellationToken)).ToDictionary(u => u.Id);
        var result = new List<FriendRequestDto>();

        foreach (var f in incoming)
        {
            if (users.TryGetValue(f.UserId, out var user))
            {
                result.Add(new FriendRequestDto(f.Id, user.ToSummaryDto(), f.Status, IsIncoming: true, f.CreatedAt));
            }
        }

        foreach (var f in outgoing)
        {
            if (users.TryGetValue(f.FriendId, out var user))
            {
                result.Add(new FriendRequestDto(f.Id, user.ToSummaryDto(), f.Status, IsIncoming: false, f.CreatedAt));
            }
        }

        return result;
    }
}
