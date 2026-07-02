using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class NotificationsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public NotificationsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Notifications";
    }

    public ObservableCollection<NotificationDto> Notifications { get; } = new();

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
