using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class SettingsViewModel : BaseViewModel
{
    private const string NotifyMatchesKey = "pref_notify_matches";
    private const string NotifyPaymentsKey = "pref_notify_payments";
    private const string NotifyFriendsKey = "pref_notify_friends";

    private readonly KawwerApiClient _api;
    private readonly AuthService _auth;
    private readonly PushRegistrationService _push;

    public SettingsViewModel(KawwerApiClient api, AuthService auth, PushRegistrationService push)
    {
        _api = api;
        _auth = auth;
        _push = push;
        Title = "Settings";

        NotifyMatches = Preferences.Default.Get(NotifyMatchesKey, true);
        NotifyPayments = Preferences.Default.Get(NotifyPaymentsKey, true);
        NotifyFriends = Preferences.Default.Get(NotifyFriendsKey, true);

        // Pre-fill from the cached session user so the form is never empty while loading.
        if (auth.Session.CurrentUser is { } cached)
        {
            ApplyUser(cached);
        }
    }

    public ObservableCollection<string> PositionOptions { get; } = new() { "Not set", "Goalkeeper", "Defender", "Midfielder", "Forward" };
    public ObservableCollection<string> FootOptions { get; } = new() { "Not set", "Left", "Right", "Both" };
    public ObservableCollection<string> VisibilityOptions { get; } = new() { "Public", "Friends only", "Private" };

    // ----- Profile -----
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _selectedPosition = "Not set";
    [ObservableProperty] private string _selectedFoot = "Not set";
    [ObservableProperty] private string _selectedVisibility = "Public";

    // ----- Notification preferences (stored on device) -----
    [ObservableProperty] private bool _notifyMatches;
    [ObservableProperty] private bool _notifyPayments;
    [ObservableProperty] private bool _notifyFriends;

    partial void OnNotifyMatchesChanged(bool value) => Preferences.Default.Set(NotifyMatchesKey, value);
    partial void OnNotifyPaymentsChanged(bool value) => Preferences.Default.Set(NotifyPaymentsKey, value);
    partial void OnNotifyFriendsChanged(bool value) => Preferences.Default.Set(NotifyFriendsKey, value);

    private DateOnly? _birthDate;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var user = await _api.GetMeAsync();
        _auth.Session.CurrentUser = user;
        ApplyUser(user);
    });

    private void ApplyUser(UserDto user)
    {
        FirstName = user.FirstName;
        LastName = user.LastName;
        PhoneNumber = user.PhoneNumber ?? string.Empty;
        _birthDate = user.BirthDate;
        SelectedPosition = user.PreferredPosition?.ToString() ?? "Not set";
        SelectedFoot = user.PreferredFoot?.ToString() ?? "Not set";
        SelectedVisibility = user.Visibility switch
        {
            ProfileVisibility.FriendsOnly => "Friends only",
            ProfileVisibility.Private => "Private",
            _ => "Public"
        };
    }

    [RelayCommand]
    private Task SaveProfileAsync() => RunAsync(async () =>
    {
        if (FirstName.Trim().Length == 0 || LastName.Trim().Length == 0)
        {
            ErrorMessage = "First and last name are required.";
            return;
        }

        PreferredPosition? position = SelectedPosition == "Not set" ? null : Enum.Parse<PreferredPosition>(SelectedPosition);
        PreferredFoot? foot = SelectedFoot == "Not set" ? null : Enum.Parse<PreferredFoot>(SelectedFoot);
        var visibility = SelectedVisibility switch
        {
            "Friends only" => ProfileVisibility.FriendsOnly,
            "Private" => ProfileVisibility.Private,
            _ => ProfileVisibility.Public
        };

        var updated = await _api.UpdateProfileAsync(new
        {
            firstName = NameFormat.Capitalize(FirstName),
            lastName = NameFormat.Capitalize(LastName),
            phoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            birthDate = _birthDate,
            preferredPosition = position,
            preferredFoot = foot,
            skillLevel = (int?)null,
            visibility
        });

        _auth.Session.CurrentUser = updated;
        await Shell.Current.DisplayAlertAsync("Settings", "Profile updated.", "OK");
    });

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Log out", "Are you sure you want to log out?", "Log out", "Cancel");
        if (!confirm)
        {
            return;
        }

        // Detach this device from pushes, then end the session.
        await _push.UnregisterAsync();
        await _auth.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
