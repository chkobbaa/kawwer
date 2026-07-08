namespace Kawwer.Application.Common.Interfaces;

/// <summary>Abstraction over the system clock to keep handlers testable.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    /// <summary>
    /// The wall-clock time zone the app operates in. Matches are scheduled and displayed in this
    /// zone (a match created for "20:00" means 20:00 local), so any conversion between a match's
    /// stored <c>MatchDate</c>/<c>StartTime</c> and a real UTC instant must go through it. Keeps
    /// reminders and auto-expiry aligned with the user's actual clock instead of drifting by the
    /// UTC offset.
    /// </summary>
    TimeZoneInfo AppTimeZone { get; }
}
