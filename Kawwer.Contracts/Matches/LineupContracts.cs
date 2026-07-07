using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.Matches;

/// <summary>Adds a guest player (someone without the app) to a match by name.</summary>
public sealed record AddGuestPlayerRequest(string Name, int? SkillLevel);

/// <summary>
/// Moves one board slot to a team and a normalized position. <paramref name="TargetId"/> is the
/// organizer's/participant's user id, or the guest's id, depending on <paramref name="Kind"/>.
/// Coordinates are 0..1 within the slot's own team half (0 = own goal line, 1 = halfway line).
/// </summary>
public sealed record UpdateLineupSlotRequest(
    LineupSlotKind Kind,
    Guid TargetId,
    TeamSide Team,
    double PositionX,
    double PositionY);

public sealed record GuestPlayerDto(
    Guid Id,
    Guid MatchId,
    string Name,
    int? SkillLevel,
    TeamSide Team,
    double? PositionX,
    double? PositionY);

/// <summary>A single person on the tactical board: the organizer, an accepted player, or a guest.</summary>
public sealed record LineupSlotDto(
    LineupSlotKind Kind,
    Guid Id,
    string DisplayName,
    string? ProfilePictureUrl,
    decimal? Reputation,
    int? SkillLevel,
    bool IsGuest,
    TeamSide Team,
    double? PositionX,
    double? PositionY);

/// <summary>The full lineup board for a match: everyone who can be placed, with their team + position.</summary>
public sealed record LineupDto(
    Guid MatchId,
    IReadOnlyList<LineupSlotDto> Slots);
