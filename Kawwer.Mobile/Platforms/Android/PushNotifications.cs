using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using AndroidX.Core.App;
using Firebase;
using Firebase.Messaging;

namespace kawwer;

/// <summary>
/// Firebase Cloud Messaging plumbing. Everything here degrades gracefully when
/// Platforms/Android/google-services.json is absent: the app runs normally,
/// push notifications just stay off.
/// </summary>
public static class PushNotifications
{
    /// <summary>Must match the channel id the API sends in AndroidConfig.</summary>
    public const string ChannelId = "kawwer_default";

    /// <summary>Returns the FCM device token, or null when Firebase is not configured.</summary>
    public static async Task<string?> GetTokenAsync()
    {
        try
        {
            // Returns null when no google-services.json was compiled into the app.
            if (FirebaseApp.InitializeApp(Platform.AppContext) is null)
            {
                return null;
            }

            EnsureChannel();
            var token = await FirebaseMessaging.Instance.GetToken().AsAsync<Java.Lang.String>();
            return token?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Creates the notification channel used for all Kawwer pushes (Android 8+).</summary>
    public static void EnsureChannel()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
        {
            return;
        }

        var channel = new NotificationChannel(ChannelId, "Kawwer", NotificationImportance.High)
        {
            Description = "Match invitations, payments and live updates"
        };
        var manager = (NotificationManager?)Platform.AppContext.GetSystemService(Context.NotificationService);
        manager?.CreateNotificationChannel(channel);
    }
}

/// <summary>
/// Receives FCM messages. Notification-type messages sent while the app is killed or in the
/// background are shown by the system automatically; this service covers foreground messages
/// and token rotation.
/// </summary>
[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class KawwerFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnNewToken(string token)
    {
        base.OnNewToken(token);
        _ = UploadTokenAsync(token);
    }

    private static async Task UploadTokenAsync(string token)
    {
        try
        {
            var services = IPlatformApplication.Current?.Services;
            var api = services?.GetService<Kawwer.Mobile.Services.KawwerApiClient>();
            var session = services?.GetService<Kawwer.Mobile.Services.SessionState>();
            if (api is null || session is null)
            {
                return;
            }

            await session.EnsureLoadedAsync();
            if (session.IsAuthenticated)
            {
                await api.UpdateDeviceTokenAsync(token);
            }
        }
        catch
        {
            // Best effort; the next PushRegistrationService run re-uploads the token.
        }
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        base.OnMessageReceived(message);

        var notification = message.GetNotification();
        var data = message.Data;
        var title = notification?.Title ?? Get(data, "title") ?? "Kawwer";
        var body = notification?.Body ?? Get(data, "body") ?? string.Empty;
        if (string.IsNullOrEmpty(body) && notification is null)
        {
            return;
        }

        ShowNotification(
            title,
            body,
            Get(data, "category"),
            Get(data, "matchId"),
            Get(data, "type"),
            Get(data, "friendshipId"));
    }

    private static string? Get(IDictionary<string, string> data, string key)
        => data.TryGetValue(key, out var value) ? value : null;

    private void ShowNotification(string title, string body, string? category, string? matchId, string? type, string? friendshipId)
    {
        PushNotifications.EnsureChannel();

        var notificationId = (int)(Java.Lang.JavaSystem.CurrentTimeMillis() & 0x7FFFFFFF);

        // Tap: open the app on the screen matching this notification (MainActivity reads the extras).
        var intent = PackageManager?.GetLaunchIntentForPackage(PackageName!);
        PendingIntent? contentIntent = null;
        if (intent is not null)
        {
            intent.PutExtra("category", category);
            intent.PutExtra("matchId", matchId);
            // AddFlags, not SetFlags: the launch intent already carries NEW_TASK, which is
            // required to start an activity from the notification shade. Overwriting the
            // flags removed it and taps silently did nothing.
            intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.SingleTop | ActivityFlags.ClearTop);
            contentIntent = PendingIntent.GetActivity(
                this, notificationId, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        }

        var builder = new NotificationCompat.Builder(this, PushNotifications.ChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(title)
            .SetContentText(body)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(body))
            .SetAutoCancel(true)
            .SetPriority(NotificationCompat.PriorityHigh);

        if (contentIntent is not null)
        {
            builder.SetContentIntent(contentIntent);
        }

        // Inline Accept/Decline buttons for invitations.
        if (type == "match_invitation" && !string.IsNullOrEmpty(matchId))
        {
            builder.AddAction(BuildAction(notificationId, NotificationActionReceiver.ActionAcceptMatch, "Accept", "matchId", matchId, 1));
            builder.AddAction(BuildAction(notificationId, NotificationActionReceiver.ActionDeclineMatch, "Decline", "matchId", matchId, 2));
        }
        else if (type == "friend_request" && !string.IsNullOrEmpty(friendshipId))
        {
            builder.AddAction(BuildAction(notificationId, NotificationActionReceiver.ActionAcceptFriend, "Accept", "friendshipId", friendshipId, 1));
            builder.AddAction(BuildAction(notificationId, NotificationActionReceiver.ActionRejectFriend, "Decline", "friendshipId", friendshipId, 2));
        }

        NotificationManagerCompat.From(this).Notify(notificationId, builder.Build());
    }

    private NotificationCompat.Action BuildAction(int notificationId, string action, string label, string extraKey, string extraValue, int requestOffset)
    {
        var intent = new Intent(this, typeof(NotificationActionReceiver));
        intent.SetAction(action);
        intent.PutExtra(extraKey, extraValue);
        intent.PutExtra(NotificationActionReceiver.ExtraNotificationId, notificationId);

        var pending = PendingIntent.GetBroadcast(
            this,
            notificationId + requestOffset,
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)!;

        return new NotificationCompat.Action(0, label, pending);
    }
}
