using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class HomeViewModel : BaseViewModel
{
    private static bool _checkedForUpdate;

    private readonly KawwerApiClient _api;
    private readonly SessionState _session;
    private readonly PushRegistrationService _push;
    private readonly MatchReminderService _reminders;
    private readonly UpdateService _update;
    private readonly RealtimeService _realtime;

    public HomeViewModel(
        KawwerApiClient api,
        SessionState session,
        PushRegistrationService push,
        MatchReminderService reminders,
        UpdateService update,
        RealtimeService realtime)
    {
        _api = api;
        _session = session;
        _push = push;
        _reminders = reminders;
        _update = update;
        _realtime = realtime;
        Title = "Home";
    }

    /// <summary>Keeps the home dashboard and unread badge live without polling.</summary>
    public void SubscribeRealtime()
    {
        _realtime.UserEvent += OnUserEvent;
        _ = _realtime.StartAsync();
    }

    public void UnsubscribeRealtime() => _realtime.UserEvent -= OnUserEvent;

    // Any invitation, match change or waiting-list move affects the upcoming list and/or the
    // unread badge, so refresh the dashboard whenever a user-scoped signal arrives.
    private void OnUserEvent(RealtimeUserEvent e) => LoadCommand.Execute(null);

    public ObservableCollection<MatchDto> Upcoming { get; } = new();
    public ObservableCollection<OrganizerDashboardItemDto> Dashboard { get; } = new();

    [ObservableProperty] private MatchDto? _nextMatch;
    [ObservableProperty] private string _greeting = "Welcome";
    [ObservableProperty] private int _unreadNotifications;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        // Register this device for push notifications (runs once per app start, best effort).
        // Awaited on purpose: it requests the Android 13+ notification permission, and the
        // persistent match-countdown notification below can only be shown once that
        // permission has been granted. Firing and forgetting made the reminder silently
        // skip its first update.
        await _push.TryRegisterAsync();

        // After a cold start the session only has tokens; fetch the profile for the greeting.
        if (_session.CurrentUser is null)
        {
            try
            {
                _session.CurrentUser = await _api.GetMeAsync();
            }
            catch
            {
                // Greeting is cosmetic; the rest of the page still loads.
            }
        }

        Greeting = _session.CurrentUser is { } user ? $"Hi {user.DisplayFirstName} 👋" : "Welcome";

        var upcoming = await _api.GetUpcomingAsync();
        Upcoming.Clear();
        foreach (var match in upcoming)
        {
            Upcoming.Add(match);
        }

        NextMatch = upcoming.FirstOrDefault();

        // Permanent countdown notification when a match starts within 24 hours.
        try
        {
            _reminders.Update(upcoming);
        }
        catch
        {
            // The reminder is cosmetic; never break the home screen for it.
        }

        var dashboard = await _api.GetDashboardAsync();
        Dashboard.Clear();
        foreach (var item in dashboard)
        {
            Dashboard.Add(item);
        }

        try
        {
            UnreadNotifications = await _api.GetUnreadCountAsync();
        }
        catch
        {
            UnreadNotifications = 0;
        }

        // Check for a new APK once per app session (best effort; never blocks the home screen).
        if (!_checkedForUpdate)
        {
            _checkedForUpdate = true;
            _ = _update.CheckForUpdateAsync();
        }
    });

    [RelayCommand]
    private Task CreateMatchAsync() => Shell.Current.GoToAsync("creatematch");

    [RelayCommand]
    private Task OpenMatchAsync(Guid matchId) => Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");

    [RelayCommand]
    private Task OpenNotificationsAsync() => Shell.Current.GoToAsync("notifications");

    [RelayCommand]
    private Task OpenDiscoverAsync() => Shell.Current.GoToAsync("//main/discovertab");

    [RelayCommand]
    private Task OpenCalendarAsync() => Shell.Current.GoToAsync("//main/calendartab");
}
