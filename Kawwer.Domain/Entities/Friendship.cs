using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A directed friendship record. The pair (UserId -> FriendId) is unique.
/// A mutual friendship is represented by the single record reaching Accepted status.
/// </summary>
public class Friendship : Entity
{
    private Friendship()
    {
    }

    public Friendship(Guid userId, Guid friendId)
    {
        if (userId == friendId)
        {
            throw new DomainException("A user cannot befriend themselves.");
        }

        Id = Guid.NewGuid();
        UserId = userId;
        FriendId = friendId;
        Status = FriendshipStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid FriendId { get; private set; }
    public FriendshipStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    public void Accept()
    {
        if (Status != FriendshipStatus.Pending)
        {
            throw new DomainException("Only pending friend requests can be accepted.");
        }

        Status = FriendshipStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        Status = FriendshipStatus.Blocked;
        RespondedAt = DateTime.UtcNow;
    }

    public bool Involves(Guid userId) => UserId == userId || FriendId == userId;
}
