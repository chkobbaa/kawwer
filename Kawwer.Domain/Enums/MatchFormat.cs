namespace Kawwer.Domain.Enums;

/// <summary>
/// How a match is contested. Drives who the accepted players line up against.
/// </summary>
public enum MatchFormat
{
    /// <summary>The default: everyone who joins is pooled into one game (no designated opponent).</summary>
    Pickup = 1,

    /// <summary>The match is played against a generic external team that does not use the app.</summary>
    VsExternalTeam = 2,

    /// <summary>The match is played against another Team registered in the app.</summary>
    VsAppTeam = 3
}
