using Android.App;
using Android.Content;
using AndroidX.Core.App;
using Kawwer.Mobile.Services;

namespace kawwer;

/// <summary>
/// Handles the Accept/Decline buttons on push notifications without opening the app.
/// Calls the API in the background and replaces the notification with a short confirmation.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class NotificationActionReceiver : BroadcastReceiver
{
    public const string ActionAcceptMatch = "com.kawwer.action.ACCEPT_MATCH";
    public const string ActionDeclineMatch = "com.kawwer.action.DECLINE_MATCH";
    public const string ActionAcceptFriend = "com.kawwer.action.ACCEPT_FRIEND";
    public const string ActionRejectFriend = "com.kawwer.action.REJECT_FRIEND";
    public const string ExtraNotificationId = "notificationId";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent?.Action is null)
        {
            return;
        }

        // Keep the process alive while the API call runs (broadcast receivers are short-lived).
        var pendingResult = GoAsync();
        _ = HandleAsync(context, intent, pendingResult);
    }

    private static async Task HandleAsync(Context context, Intent intent, PendingResult? pendingResult)
    {
        var notificationId = intent.GetIntExtra(ExtraNotificationId, -1);
        try
        {
            var services = IPlatformApplication.Current?.Services;
            var api = services?.GetService<KawwerApiClient>();
            var session = services?.GetService<SessionState>();
            if (api is null || session is null)
            {
                return;
            }

            await session.EnsureLoadedAsync();
            if (!session.IsAuthenticated)
            {
                return;
            }

            string confirmation;
            switch (intent.Action)
            {
                case ActionAcceptMatch when Guid.TryParse(intent.GetStringExtra("matchId"), out var matchId):
                    await api.RespondAsync(matchId, accept: true);
                    confirmation = "Invitation accepted. See you on the pitch! ⚽";
                    break;

                case ActionDeclineMatch when Guid.TryParse(intent.GetStringExtra("matchId"), out var matchId):
                    await api.RespondAsync(matchId, accept: false);
                    confirmation = "Invitation declined.";
                    break;

                case ActionAcceptFriend when Guid.TryParse(intent.GetStringExtra("friendshipId"), out var friendshipId):
                    await api.AcceptFriendRequestAsync(friendshipId);
                    confirmation = "Friend request accepted. 🤝";
                    break;

                case ActionRejectFriend when Guid.TryParse(intent.GetStringExtra("friendshipId"), out var friendshipId):
                    await api.RejectFriendRequestAsync(friendshipId);
                    confirmation = "Friend request declined.";
                    break;

                default:
                    return;
            }

            ReplaceWithConfirmation(context, notificationId, confirmation);
        }
        catch (Exception ex)
        {
            // Surface a failure so the user knows to retry from inside the app.
            ReplaceWithConfirmation(context, notificationId, $"That didn't work: {ex.Message}");
        }
        finally
        {
            pendingResult?.Finish();
        }
    }

    private static void ReplaceWithConfirmation(Context context, int notificationId, string message)
    {
        if (notificationId < 0)
        {
            return;
        }

        PushNotifications.EnsureChannel();
        var builder = new NotificationCompat.Builder(context, PushNotifications.ChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle("Kawwer")
            .SetContentText(message)
            .SetAutoCancel(true)
            .SetOnlyAlertOnce(true)
            .SetPriority(NotificationCompat.PriorityDefault);

        NotificationManagerCompat.From(context).Notify(notificationId, builder.Build());
    }
}
