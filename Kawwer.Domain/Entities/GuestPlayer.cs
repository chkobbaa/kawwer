using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A player who takes part in a match without having a Kawwer account. Guests are added by name by
/// the organizer (or an accepted player) so they can appear in the roster and on the tactical lineup
/// board. They own the same lineup fields as a real participant — a team and a normalized position —
/// but carry no identity, reputation, payment or attendance state.
/// </summary>
public class GuestPlayer : Entity
{
    private GuestPlayer()
    {
        Name = string.Empty;
    }

    public GuestPlayer(Guid matchId, string name, Guid addedByUserId, int? skillLevel = null)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new DomainException("A guest player must have a name.");
        }

        if (trimmed.Length > 60)
        {
            throw new DomainException("A guest player's name can be at most 60 characters.");
        }

        if (skillLevel is < 1 or > 5)
        {
            throw new DomainException("Skill level must be between 1 and 5.");
        }

        Id = Guid.NewGuid();
        MatchId = matchId;
        Name = trimmed;
        AddedByUserId = addedByUserId;
        SkillLevel = skillLevel;
        Team = TeamSide.Unassigned;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid MatchId { get; private set; }
    public string Name { get; private set; }

    /// <summary>The user (organizer or accepted player) who added this guest.</summary>
    public Guid AddedByUserId { get; private set; }

    /// <summary>Optional 1..5 skill hint used to weight the guest during auto-balance.</summary>
    public int? SkillLevel { get; private set; }

    public TeamSide Team { get; private set; }

    /// <summary>Normalized 0..1 position within the guest's own team half (0 = own goal, 1 = halfway).</summary>
    public double? PositionX { get; private set; }
    public double? PositionY { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public void Rename(string name)
    {
        var trimmed = name?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            throw new DomainException("A guest player must have a name.");
        }

        Name = trimmed;
    }

    /// <summary>Places the guest on a team at a normalized board position, clamping coordinates to 0..1.</summary>
    public void PlaceInLineup(TeamSide team, double positionX, double positionY)
    {
        Team = team;
        PositionX = Math.Clamp(positionX, 0d, 1d);
        PositionY = Math.Clamp(positionY, 0d, 1d);
    }

    public void RemoveFromLineup()
    {
        Team = TeamSide.Unassigned;
        PositionX = null;
        PositionY = null;
    }
}
