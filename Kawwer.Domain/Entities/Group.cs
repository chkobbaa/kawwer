using Kawwer.Domain.Common;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A private, owner-scoped collection of friends used to speed up match invitations.
/// </summary>
public class Group : AggregateRoot
{
    private readonly List<GroupMember> _members = new();

    private Group()
    {
        Name = string.Empty;
    }

    public Group(Guid ownerId, string name, string? description = null)
    {
        Id = Guid.NewGuid();
        OwnerId = ownerId;
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<GroupMember> Members => _members.AsReadOnly();

    public void Rename(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddMember(Guid userId)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new DomainException("The user is already a member of this group.");
        }

        _members.Add(new GroupMember(Id, userId));
    }

    public void RemoveMember(Guid userId)
    {
        var member = _members.FirstOrDefault(m => m.UserId == userId);
        if (member is not null)
        {
            _members.Remove(member);
        }
    }
}
