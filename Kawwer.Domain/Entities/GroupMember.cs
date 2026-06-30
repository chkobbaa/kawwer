using Kawwer.Domain.Common;

namespace Kawwer.Domain.Entities;

/// <summary>
/// Membership of a user inside a group.
/// </summary>
public class GroupMember : Entity
{
    private GroupMember()
    {
    }

    public GroupMember(Guid groupId, Guid userId)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        UserId = userId;
        AddedAt = DateTime.UtcNow;
    }

    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AddedAt { get; private set; }
}
