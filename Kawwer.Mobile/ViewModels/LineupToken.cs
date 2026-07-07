using CommunityToolkit.Mvvm.ComponentModel;
using Kawwer.Mobile.Models;

namespace Kawwer.Mobile.ViewModels;

/// <summary>
/// One draggable person on the tactical board. Positions are normalized 0..1 within the token's own
/// team half: <see cref="PositionX"/> is depth (0 = own goal line, 1 = halfway line) and
/// <see cref="PositionY"/> is width across the pitch.
/// </summary>
public sealed partial class LineupToken : ObservableObject
{
    public LineupToken(LineupSlotDto slot)
    {
        Kind = slot.Kind;
        Id = slot.Id;
        DisplayName = slot.DisplayName;
        ProfilePictureUrl = slot.ProfilePictureUrl;
        IsGuest = slot.IsGuest;
        SkillLevel = slot.SkillLevel;
        Team = slot.Team;
        PositionX = slot.PositionX ?? 0.5;
        PositionY = slot.PositionY ?? 0.5;
    }

    public LineupSlotKind Kind { get; }
    public Guid Id { get; }
    public string DisplayName { get; }
    public string? ProfilePictureUrl { get; }
    public bool IsGuest { get; }
    public int? SkillLevel { get; }

    [ObservableProperty] private TeamSide _team;
    [ObservableProperty] private double _positionX;
    [ObservableProperty] private double _positionY;

    /// <summary>Short name for the bench chip: first name + last initial, or the single guest name.</summary>
    public string ShortName
    {
        get
        {
            var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return DisplayName;
            }

            return parts.Length == 1 ? parts[0] : $"{parts[0]} {parts[1][..1].ToUpperInvariant()}.";
        }
    }

    public string Initials
    {
        get
        {
            var parts = DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return "?";
            }

            return parts.Length == 1
                ? parts[0][..1].ToUpperInvariant()
                : $"{parts[0][0]}{parts[1][0]}".ToUpperInvariant();
        }
    }
}
