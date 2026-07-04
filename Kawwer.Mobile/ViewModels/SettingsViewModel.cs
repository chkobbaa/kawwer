using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class SettingsViewModel : BaseViewModel
{
    private const string NotifyMatchesKey = "pref_notify_matches";
    private const string NotifyPaymentsKey = "pref_notify_payments";
    private const string NotifyFriendsKey = "pref_notify_friends";

    // Largest edge (px) we upload; keeps avatars small and fast on mobile data.
    private const int MaxAvatarDimension = 1024;

    private readonly KawwerApiClient _api;
    private readonly AuthService _auth;
    private readonly PushRegistrationService _push;
    private readonly UpdateService _update;

    public SettingsViewModel(KawwerApiClient api, AuthService auth, PushRegistrationService push, UpdateService update)
    {
        _api = api;
        _auth = auth;
        _push = push;
        _update = update;
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
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    private string _firstName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Initials))]
    private string _lastName = string.Empty;

    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _selectedPosition = "Not set";
    [ObservableProperty] private string _selectedFoot = "Not set";
    [ObservableProperty] private string _selectedVisibility = "Public";
    [ObservableProperty] private string? _profilePictureUrl;

    public string Initials =>
        $"{(FirstName.Length > 0 ? char.ToUpperInvariant(FirstName[0]) : ' ')}{(LastName.Length > 0 ? char.ToUpperInvariant(LastName[0]) : ' ')}".Trim();

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
        ProfilePictureUrl = user.ProfilePictureUrl;
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
        await Dialog.ShowSuccessAsync("Profile updated.");
    });

    /// <summary>Pick a photo, downscale/compress it, and upload it as the profile picture.</summary>
    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        FileResult? photo;
        try
        {
            photo = await MediaPicker.Default.PickPhotoAsync();
        }
        catch (FeatureNotSupportedException)
        {
            ErrorMessage = "Picking photos isn't supported on this device.";
            return;
        }
        catch (PermissionException)
        {
            ErrorMessage = "Permission to access photos was denied.";
            return;
        }

        if (photo is null)
        {
            return; // user cancelled
        }

        await RunAsync(async () =>
        {
            byte[] jpeg;
            using (var source = await photo.OpenReadAsync())
            {
                // Standard MAUI image pipeline: decode, downscale to a max edge, re-encode as JPEG.
                var image = PlatformImage.FromStream(source);
                using var resized = image.Downsize(MaxAvatarDimension, disposeOriginal: true);
                using var buffer = new MemoryStream();
                resized.Save(buffer, ImageFormat.Jpeg, quality: 0.8f);
                jpeg = buffer.ToArray();
            }

            using var upload = new MemoryStream(jpeg);
            var updated = await _api.UploadProfilePhotoAsync(upload, "avatar.jpg", "image/jpeg");

            _auth.Session.CurrentUser = updated;
            ProfilePictureUrl = updated.ProfilePictureUrl;
            await Dialog.ShowSuccessAsync("Profile picture updated.");
        });
    }

    [RelayCommand]
    private Task CheckForUpdatesAsync() => _update.CheckForUpdateAsync(announceUpToDate: true);

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirm = await Dialog.ConfirmAsync("Log out", "Are you sure you want to log out?", "Log out", "Cancel");
        if (!confirm)
        {
            return;
        }

        // Detach this device from pushes, then end the session. AuthService rebuilds the shell,
        // which returns us to a clean Login screen.
        await _push.UnregisterAsync();
        await _auth.LogoutAsync();
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        var confirm = await Dialog.ConfirmAsync(
            "Delete account",
            "This deactivates your account and signs you out on every device. This cannot be undone. Continue?",
            "Delete",
            "Cancel");
        if (!confirm)
        {
            return;
        }

        var ok = false;
        await RunAsync(async () =>
        {
            await _api.DeleteAccountAsync();
            ok = true;
        });

        if (ok)
        {
            await _push.UnregisterAsync();
            await _auth.LogoutAsync();
        }
    }
}
