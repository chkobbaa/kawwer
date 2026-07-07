namespace Kawwer.Mobile.Models;

/// <summary>A guest player (someone without the app) added to a match by name.</summary>
public sealed class GuestPlayerDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? SkillLevel { get; set; }
    public TeamSide Team { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }

    /// <summary>First letter of the name, for the avatar placeholder.</summary>
    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name.Trim()[..1].ToUpperInvariant();
}

/// <summary>A single person on the tactical board: organizer, accepted player, or guest.</summary>
public sealed class LineupSlotDto
{
    public LineupSlotKind Kind { get; set; }
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public decimal? Reputation { get; set; }
    public int? SkillLevel { get; set; }
    public bool IsGuest { get; set; }
    public TeamSide Team { get; set; }
    public double? PositionX { get; set; }
    public double? PositionY { get; set; }
}

public sealed class LineupDto
{
    public Guid MatchId { get; set; }
    public List<LineupSlotDto> Slots { get; set; } = new();
}
