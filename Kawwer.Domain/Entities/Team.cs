using Kawwer.Domain.Common;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A private, owner-scoped collection of friends used to speed up match invitations
/// and to line up as an opponent in an in-app match.
/// </summary>
public class Team : AggregateRoot
{
    private readonly List<TeamMember> _members = new();

    private Team()
    {
        Name = string.Empty;
    }

    public Team(Guid ownerId, string name, string? description = null)
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

    public IReadOnlyCollection<TeamMember> Members => _members.AsReadOnly();

    public void Rename(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void AddMember(Guid userId)
    {
        if (_members.Any(m => m.UserId == userId))
        {
            throw new DomainException("The user is already a member of this team.");
        }

        _members.Add(new TeamMember(Id, userId));
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
