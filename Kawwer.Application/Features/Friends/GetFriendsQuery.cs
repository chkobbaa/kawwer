using Kawwer.Application.Common.Interfaces;
using Kawwer.Application.Common.Mappings;
using Kawwer.Application.Common.Messaging;
using Kawwer.Contracts.Friends;
using Kawwer.Domain.Enums;

namespace Kawwer.Application.Features.Friends;

public sealed record GetFriendsQuery(Guid UserId) : IRequest<IReadOnlyList<FriendDto>>;

public sealed class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, IReadOnlyList<FriendDto>>
{
    private readonly IFriendshipRepository _friendships;
    private readonly IUserRepository _users;

    public GetFriendsQueryHandler(IFriendshipRepository friendships, IUserRepository users)
    {
        _friendships = friendships;
        _users = users;
    }

    public async Task<IReadOnlyList<FriendDto>> HandleAsync(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var friendships = await _friendships.GetAcceptedForUserAsync(request.UserId, cancellationToken);

        var otherIds = friendships
            .Select(f => f.UserId == request.UserId ? f.FriendId : f.UserId)
            .ToList();

        var users = (await _users.GetByIdsAsync(otherIds, cancellationToken)).ToDictionary(u => u.Id);

        var result = new List<FriendDto>();
        foreach (var friendship in friendships)
        {
            var otherId = friendship.UserId == request.UserId ? friendship.FriendId : friendship.UserId;
            if (users.TryGetValue(otherId, out var user))
            {
                result.Add(new FriendDto(friendship.Id, user.ToSummaryDto(), friendship.RespondedAt ?? friendship.CreatedAt));
            }
        }

        return result;
    }
}
