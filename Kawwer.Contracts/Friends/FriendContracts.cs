using Kawwer.Contracts.Users;
using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Friends;

public sealed record SendFriendRequestRequest(Guid TargetUserId);

public sealed record FriendRequestDto(
    Guid FriendshipId,
    UserSummaryDto User,
    FriendshipStatus Status,
    bool IsIncoming,
    DateTime CreatedAt);

public sealed record FriendDto(
    Guid FriendshipId,
    UserSummaryDto User,
    DateTime FriendsSince);
