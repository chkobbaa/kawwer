using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

/// <summary>
/// Read-only profile of another player: identity, reliability badge, statistics, and -
/// when the viewer is a friend - the matches that player is currently organizing.
/// </summary>
[QueryProperty(nameof(UserIdQuery), "userId")]
public sealed partial class PlayerProfileViewModel : BaseViewModel
{
    /// <summary>Shell query values arrive as strings; parse instead of casting to Guid.</summary>
    public string UserIdQuery
    {
        set
        {
            if (Guid.TryParse(value, out var id))
            {
                UserId = id;
            }
        }
    }

    private readonly KawwerApiClient _api;
    private readonly SessionState _session;

    public PlayerProfileViewModel(KawwerApiClient api, SessionState session)
    {
        _api = api;
        _session = session;
        Title = "Player";
    }

    public ObservableCollection<MatchDto> Organizing { get; } = new();

    [ObservableProperty] private Guid _userId;
    [ObservableProperty] private UserDto? _user;
    [ObservableProperty] private PlayerStatisticsDto? _statistics;
    [ObservableProperty] private string _badge = string.Empty;
    [ObservableProperty] private bool _isFriend;
    [ObservableProperty] private bool _isSelf;
    [ObservableProperty] private bool _canAddFriend;
    [ObservableProperty] private bool _requestPending;
    [ObservableProperty] private bool _hasOrganizing;
    [ObservableProperty] private string _positionLabel = string.Empty;

    partial void OnUserIdChanged(Guid value) => _ = LoadAsync();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    // Unguarded core so SendRequestAsync can reload while RunAsync holds IsBusy.
    private async Task LoadCoreAsync()
    {
        if (UserId == Guid.Empty)
        {
            return;
        }

        User = await _api.GetUserAsync(UserId);
        Badge = FormatBadge(User.ReliabilityBadge);
        PositionLabel = FormatPosition(User);
        IsSelf = _session.UserId == UserId;

        // Friendship state drives the header action and the "organizing" section.
        IsFriend = false;
        RequestPending = false;
        if (!IsSelf)
        {
            try
            {
                var friends = await _api.GetFriendsAsync();
                IsFriend = friends.Any(f => f.User.Id == UserId);

                if (!IsFriend)
                {
                    var requests = await _api.GetFriendRequestsAsync();
                    RequestPending = requests.Any(r => r.User.Id == UserId);
                }
            }
            catch
            {
                // Friendship state is cosmetic here; the profile still loads.
            }
        }

        CanAddFriend = !IsSelf && !IsFriend && !RequestPending;

        try
        {
            Statistics = await _api.GetUserStatisticsAsync(UserId);
        }
        catch
        {
            Statistics = null;
        }

        // Matches they organize: the API returns an empty list unless we're friends.
        Organizing.Clear();
        try
        {
            foreach (var match in await _api.GetUserOrganizingAsync(UserId))
            {
                Organizing.Add(match);
            }
        }
        catch
        {
            // Optional section; ignore failures.
        }

        HasOrganizing = Organizing.Count > 0;
    }

    [RelayCommand]
    private Task SendRequestAsync() => RunAsync(async () =>
    {
        await _api.SendFriendRequestAsync(UserId);
        RequestPending = true;
        CanAddFriend = false;
        await Shell.Current.DisplayAlertAsync("Friends", "Friend request sent.", "OK");
    });

    [RelayCommand]
    private Task OpenMatchAsync(Guid matchId) => Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");

    private static string FormatBadge(ReliabilityBadge badge) => badge switch
    {
        ReliabilityBadge.VeryReliable => "🟢 Very Reliable",
        ReliabilityBadge.Reliable => "🟢 Reliable",
        ReliabilityBadge.OccasionallyCancels => "🟡 Occasionally Cancels",
        ReliabilityBadge.OftenLate => "🟠 Often Late",
        _ => "🔴 Frequent No-Show"
    };

    private static string FormatPosition(UserDto user)
    {
        var parts = new List<string>();
        if (user.PreferredPosition is { } position)
        {
            parts.Add(position.ToString());
        }

        if (user.PreferredFoot is { } foot)
        {
            parts.Add($"{foot}-footed");
        }

        if (user.SkillLevel is { } skill)
        {
            parts.Add($"Skill {skill}/10");
        }

        return string.Join(" · ", parts);
    }
}
