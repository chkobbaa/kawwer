using Kawwer.Domain.Common;

namespace Kawwer.Domain.Entities;

/// <summary>
/// Membership of a user inside a team.
/// </summary>
public class TeamMember : Entity
{
    private TeamMember()
    {
    }

    public TeamMember(Guid teamId, Guid userId)
    {
        Id = Guid.NewGuid();
        TeamId = teamId;
        UserId = userId;
        AddedAt = DateTime.UtcNow;
    }

    public Guid TeamId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AddedAt { get; private set; }
}
