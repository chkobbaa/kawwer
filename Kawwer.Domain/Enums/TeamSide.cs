namespace Kawwer.Domain.Enums;

/// <summary>
/// Which side of the tactical lineup board a player belongs to. <see cref="Unassigned"/> means the
/// player is in the match but has not been placed on either team yet (e.g. before auto-balance).
/// </summary>
public enum TeamSide
{
    Unassigned = 0,
    TeamA = 1,
    TeamB = 2
}
