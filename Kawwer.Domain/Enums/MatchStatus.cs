namespace Kawwer.Domain.Enums;

public enum MatchStatus
{
    Draft = 1,
    Published = 2,
    Full = 3,
    Playing = 4,
    Finished = 5,
    Cancelled = 6,

    /// <summary>
    /// The match's scheduled end passed without it being started or finished by the organizer.
    /// A terminal state, like <see cref="Finished"/> and <see cref="Cancelled"/>: it accepts no
    /// responses, invitations or edits. Set automatically by the lifecycle sweep.
    /// </summary>
    Expired = 7
}
