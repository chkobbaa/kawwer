using Kawwer.Mobile.Models;

#if ANDROID
using Android.App;
using Android.Content;
using AndroidX.Core.App;
#endif

namespace Kawwer.Mobile.Services;

/// <summary>
/// Shows a permanent (ongoing) status-bar notification when the user's next match starts in
/// less than 24 hours. The notification carries the match info plus a live countdown to
/// kick-off, cannot be swiped away, and opens the match details screen when tapped.
/// </summary>
public sealed class MatchReminderService
{
    private const int ReminderNotificationId = 424242;

#if ANDROID
    private const string ReminderChannelId = "kawwer_match_reminder";
#endif

    /// <summary>Call with the freshly loaded upcoming matches; shows, updates or clears the reminder.</summary>
    public void Update(IEnumerable<MatchDto> upcomingMatches)
    {
        var now = DateTime.UtcNow;

        // The next match that kicks off within the coming 24 hours (dates are stored as UTC).
        var next = upcomingMatches
            .Where(m => m.Status is not (MatchStatus.Cancelled or MatchStatus.Finished))
            .Select(m => (Match: m, KickoffUtc: m.MatchDate.ToDateTime(m.StartTime, DateTimeKind.Utc)))
            .Where(x => x.KickoffUtc > now && x.KickoffUtc - now <= TimeSpan.FromHours(24))
            .OrderBy(x => x.KickoffUtc)
            .FirstOrDefault();

        if (next.Match is null)
        {
            Clear();
            return;
        }

        Show(next.Match, next.KickoffUtc);
    }

    public void Clear()
    {
#if ANDROID
        NotificationManagerCompat.From(Platform.AppContext).Cancel(ReminderNotificationId);
#endif
    }

    private static void Show(MatchDto match, DateTime kickoffUtc)
    {
#if ANDROID
        var context = Platform.AppContext;
        if (!NotificationManagerCompat.From(context).AreNotificationsEnabled())
        {
            return;
        }

        EnsureChannel(context);

        var remaining = kickoffUtc - DateTime.UtcNow;
        var remainingLabel = remaining.TotalHours >= 1
            ? $"{(int)remaining.TotalHours}h {remaining.Minutes:00}m"
            : $"{remaining.Minutes}m";

        // Tapping the reminder opens the match details screen.
        var intent = context.PackageManager?.GetLaunchIntentForPackage(context.PackageName!);
        PendingIntent? contentIntent = null;
        if (intent is not null)
        {
            intent.PutExtra("category", "Match");
            intent.PutExtra("matchId", match.Id.ToString());
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            contentIntent = PendingIntent.GetActivity(
                context, ReminderNotificationId, intent,
                PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        }

        var builder = new NotificationCompat.Builder(context, ReminderChannelId)
            .SetSmallIcon(kawwer.Resource.Mipmap.appicon)
            .SetContentTitle($"⚽ {match.Title} · starts in {remainingLabel}")
            .SetContentText($"{match.Field.Name} · kick-off {match.WhenLabel} · {match.PlayersLabel}")
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(
                $"{match.Field.Name}\nKick-off {match.WhenLabel}\n{match.PlayersLabel}"))
            .SetOngoing(true)              // Permanent: cannot be swiped away while active.
            .SetOnlyAlertOnce(true)        // Silent refreshes; no repeated sounds.
            .SetShowWhen(true)
            .SetWhen(new DateTimeOffset(kickoffUtc).ToUnixTimeMilliseconds())
            .SetUsesChronometer(true)      // Live ticker in the notification header.
            .SetCategory(NotificationCompat.CategoryEvent)
            .SetPriority(NotificationCompat.PriorityDefault);

        if (OperatingSystem.IsAndroidVersionAtLeast(24))
        {
            builder.SetChronometerCountDown(true); // Counts DOWN to kick-off.
        }

        if (contentIntent is not null)
        {
            builder.SetContentIntent(contentIntent);
        }

        NotificationManagerCompat.From(context).Notify(ReminderNotificationId, builder.Build());
#endif
    }

#if ANDROID
    private static void EnsureChannel(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            return;
        }

        var channel = new NotificationChannel(ReminderChannelId, "Match reminders", NotificationImportance.Low)
        {
            Description = "Persistent countdown for matches starting within 24 hours"
        };
        channel.SetShowBadge(false);
        var manager = (NotificationManager?)context.GetSystemService(Context.NotificationService);
        manager?.CreateNotificationChannel(channel);
    }
#endif
}
