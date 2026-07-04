using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class ProfileViewModel : BaseViewModel
{
    private const string UserCacheKey = JsonCache.Keys.ProfileUser;
    private const string StatsCacheKey = JsonCache.Keys.ProfileStats;

    private readonly KawwerApiClient _api;
    private readonly AuthService _auth;

    public ProfileViewModel(KawwerApiClient api, AuthService auth)
    {
        _api = api;
        _auth = auth;
        Title = "Profile";

        // Cold-start instant paint: show the in-memory session user if present, otherwise the last
        // profile we cached on disk. The network refresh in LoadAsync then overwrites both.
        var cachedUser = _auth.Session.CurrentUser ?? JsonCache.Load<UserDto>(UserCacheKey);
        if (cachedUser is not null)
        {
            User = cachedUser;
            Badge = FormatBadge(cachedUser.ReliabilityBadge);
        }

        Statistics = JsonCache.Load<PlayerStatisticsDto>(StatsCacheKey);
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

        // Overwrite the cache so the next cold start paints the freshest values.
        JsonCache.Save(UserCacheKey, User);
        JsonCache.Save(StatsCacheKey, Statistics);
    });

    [RelayCommand]
    private Task OpenSettingsAsync() => Shell.Current.GoToAsync("settings");

    [RelayCommand]
    private Task OpenFieldsAsync() => Shell.Current.GoToAsync("fields");

    private static string FormatBadge(ReliabilityBadge badge) => badge switch
    {
        ReliabilityBadge.VeryReliable => "🟢 Very Reliable",
        ReliabilityBadge.Reliable => "🟢 Reliable",
        ReliabilityBadge.OccasionallyCancels => "🟡 Occasionally Cancels",
        ReliabilityBadge.OftenLate => "🟠 Often Late",
        _ => "🔴 Frequent No-Show"
    };
}
