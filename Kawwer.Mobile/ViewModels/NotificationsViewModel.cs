using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class NotificationsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;
    private readonly RealtimeService _realtime;

    public NotificationsViewModel(KawwerApiClient api, RealtimeService realtime)
    {
        _api = api;
        _realtime = realtime;
        Title = "Notifications";
    }

    public ObservableCollection<NotificationDto> Notifications { get; } = new();

    /// <summary>Live-refresh the list as new notifications land.</summary>
    public void SubscribeRealtime()
    {
        _realtime.UserEvent += OnUserEvent;
        _ = _realtime.StartAsync();
    }

    public void UnsubscribeRealtime() => _realtime.UserEvent -= OnUserEvent;

    private void OnUserEvent(RealtimeUserEvent e) => LoadCommand.Execute(null);

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    // Unguarded core so mutations (open/accept/decline) can reload while RunAsync holds IsBusy.
    private async Task LoadCoreAsync()
    {
        var result = await _api.GetNotificationsAsync();
        Notifications.Clear();
        foreach (var n in result.Items)
        {
            Notifications.Add(n);
        }
    }

    /// <summary>Tapping a notification marks it read and opens the screen it refers to.</summary>
    [RelayCommand]
    private Task OpenAsync(NotificationDto notification) => RunAsync(async () =>
    {
        if (!notification.IsRead)
        {
            try
            {
                await _api.MarkNotificationReadAsync(notification.Id);
            }
            catch
            {
                // Navigation matters more than the read flag.
            }
        }

        var route = NotificationNavigation.BuildRoute(
            notification.Category.ToString(),
            notification.RelatedMatchId?.ToString());

        if (route == "notifications")
        {
            // Nothing to navigate to; just refresh the read state.
            await LoadCoreAsync();
            return;
        }

        await Shell.Current.GoToAsync(route);
    });

    [RelayCommand]
    private Task AcceptInvitationAsync(NotificationDto notification) => RespondAsync(notification, accept: true);

    [RelayCommand]
    private Task DeclineInvitationAsync(NotificationDto notification) => RespondAsync(notification, accept: false);

    [RelayCommand]
    private Task AcceptFriendRequestAsync(NotificationDto notification) => RespondToFriendAsync(notification, accept: true);

    [RelayCommand]
    private Task DeclineFriendRequestAsync(NotificationDto notification) => RespondToFriendAsync(notification, accept: false);

    private Task RespondToFriendAsync(NotificationDto notification, bool accept) => RunAsync(async () =>
    {
        if (notification.RelatedFriendshipId is not { } friendshipId)
        {
            return;
        }

        try
        {
            if (accept)
            {
                await _api.AcceptFriendRequestAsync(friendshipId);
            }
            else
            {
                await _api.RejectFriendRequestAsync(friendshipId);
            }
        }
        catch (ApiException)
        {
            // The request may have been withdrawn/handled elsewhere; reload so it disappears.
            await LoadCoreAsync();
            throw;
        }

        try
        {
            await _api.MarkNotificationReadAsync(notification.Id);
        }
        catch
        {
            // Best effort.
        }

        await LoadCoreAsync();

        if (accept)
        {
            await Dialog.ShowSuccessAsync("Friend request accepted.");
        }
    });

    private Task RespondAsync(NotificationDto notification, bool accept) => RunAsync(async () =>
    {
        if (notification.RelatedMatchId is not { } matchId)
        {
            return;
        }

        try
        {
            await _api.RespondAsync(matchId, accept);
        }
        catch (ApiException)
        {
            // The match may have been cancelled or the invitation withdrawn in the meantime.
            // Reload so stale invitations (deleted server-side) disappear, then surface the error.
            await LoadCoreAsync();
            throw;
        }

        try
        {
            await _api.MarkNotificationReadAsync(notification.Id);
        }
        catch
        {
            // Best effort.
        }

        await LoadCoreAsync();

        if (accept)
        {
            await Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");
        }
    });

    [RelayCommand]
    private Task MarkAllReadAsync() => RunAsync(async () =>
    {
        await _api.MarkAllReadAsync();
        await LoadCoreAsync();
    });
}
