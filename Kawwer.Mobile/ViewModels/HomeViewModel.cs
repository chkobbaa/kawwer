using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class HomeViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;
    private readonly SessionState _session;

    public HomeViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Home";
    }

    public ObservableCollection<MatchDto> Upcoming { get; } = new();
    public ObservableCollection<OrganizerDashboardItemDto> Dashboard { get; } = new();

    [ObservableProperty] private MatchDto? _nextMatch;
    [ObservableProperty] private string _greeting = "Welcome";
    [ObservableProperty] private int _unreadNotifications;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        Greeting = _session.CurrentUser is { } user ? $"Hi {user.FirstName} 👋" : "Welcome";

        var upcoming = await _api.GetUpcomingAsync();
        Upcoming.Clear();
        foreach (var match in upcoming)
        {
            Upcoming.Add(match);
        }

        NextMatch = upcoming.FirstOrDefault();

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
    });

    [RelayCommand]
    private Task CreateMatchAsync() => Shell.Current.GoToAsync("creatematch");

    [RelayCommand]
    private Task OpenMatchAsync(Guid matchId) => Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");

    [RelayCommand]
    private Task OpenNotificationsAsync() => Shell.Current.GoToAsync("notifications");
}
