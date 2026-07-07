namespace Kawwer.Domain.Enums;

/// <summary>
/// Identifies what kind of person occupies a slot on the lineup board. The organizer is modelled
/// implicitly on the match (they never hold a <c>MatchParticipant</c> row), accepted players hold a
/// participant row, and guests are people without the app that an organizer added by name.
/// </summary>
public enum LineupSlotKind
{
    Organizer = 1,
    Participant = 2,
    Guest = 3
}
