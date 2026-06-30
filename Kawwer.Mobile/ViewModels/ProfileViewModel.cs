using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class ProfileViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;
    private readonly AuthService _auth;

    public ProfileViewModel(KawwerApiClient api, AuthService auth)
    {
        _api = api;
        _auth = auth;
        Title = "Profile";
    }

    [ObservableProperty] private UserDto? _user;
    [ObservableProperty] private PlayerStatisticsDto? _statistics;
    [ObservableProperty] private string _badge = string.Empty;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        User = await _api.GetMeAsync();
        _auth.Session.CurrentUser = User;
        Statistics = await _api.GetMyStatisticsAsync();
        Badge = FormatBadge(User.ReliabilityBadge);
    });

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }

    private static string FormatBadge(ReliabilityBadge badge) => badge switch
    {
        ReliabilityBadge.VeryReliable => "🟢 Very Reliable",
        ReliabilityBadge.Reliable => "🟢 Reliable",
        ReliabilityBadge.OccasionallyCancels => "🟡 Occasionally Cancels",
        ReliabilityBadge.OftenLate => "🟠 Often Late",
        _ => "🔴 Frequent No-Show"
    };
}
