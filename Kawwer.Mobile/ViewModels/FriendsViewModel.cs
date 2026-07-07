using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class FriendsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;
    private readonly RealtimeService _realtime;

    public FriendsViewModel(KawwerApiClient api, RealtimeService realtime)
    {
        _api = api;
        _realtime = realtime;
        Title = "Friends";
    }

    public ObservableCollection<FriendDto> Friends { get; } = new();
    public ObservableCollection<FriendRequestDto> Requests { get; } = new();
    public ObservableCollection<UserSummaryDto> SearchResults { get; } = new();

    [ObservableProperty] private string _searchTerm = string.Empty;

    /// <summary>Live-refresh the list the instant a friend request/acceptance arrives.</summary>
    public void SubscribeRealtime()
    {
        _realtime.UserEvent += OnUserEvent;
        _ = _realtime.StartAsync();
    }

    public void UnsubscribeRealtime() => _realtime.UserEvent -= OnUserEvent;

    private void OnUserEvent(RealtimeUserEvent e)
    {
        if (e.Category == nameof(NotificationCategory.Friend))
        {
            LoadCommand.Execute(null);
        }
    }

    [RelayCommand]
    public Task LoadAsync() => RunAsync(LoadCoreAsync);

    // Unguarded core so mutations (accept/reject) can reload while RunAsync holds IsBusy.
    private async Task LoadCoreAsync()
    {
        var friends = await _api.GetFriendsAsync();
        Friends.Clear();
        foreach (var f in friends)
        {
            Friends.Add(f);
        }

        var requests = await _api.GetFriendRequestsAsync();
        Requests.Clear();
        foreach (var r in requests)
        {
            Requests.Add(r);
        }
    }

    [RelayCommand]
    private Task SearchAsync() => RunAsync(async () =>
    {
        SearchResults.Clear();
        if (SearchTerm.Trim().Length < 2)
        {
            return;
        }

        var results = await _api.SearchUsersAsync(SearchTerm.Trim());
        foreach (var u in results)
        {
            SearchResults.Add(u);
        }
    });

    [RelayCommand]
    private Task SendRequestAsync(Guid userId) => RunAsync(async () =>
    {
        await _api.SendFriendRequestAsync(userId);

        // Reflect it right away: drop the person from the search results so the "Add" button
        // doesn't invite a second time, then confirm with a non-intrusive popup.
        var target = SearchResults.FirstOrDefault(u => u.Id == userId);
        if (target is not null)
        {
            SearchResults.Remove(target);
        }

        await Dialog.ShowSuccessAsync("Friend request sent.");
    });

    [RelayCommand]
    private Task AcceptAsync(Guid friendshipId) => RunAsync(async () =>
    {
        await _api.AcceptFriendRequestAsync(friendshipId);

        // Move the request straight into the friends list instead of waiting for a full reload.
        var request = Requests.FirstOrDefault(r => r.FriendshipId == friendshipId);
        if (request is not null)
        {
            Requests.Remove(request);
            Friends.Add(new FriendDto
            {
                FriendshipId = request.FriendshipId,
                User = request.User,
                FriendsSince = DateTime.UtcNow
            });
        }
    });

    [RelayCommand]
    private Task RejectAsync(Guid friendshipId) => RunAsync(async () =>
    {
        await _api.RejectFriendRequestAsync(friendshipId);

        var request = Requests.FirstOrDefault(r => r.FriendshipId == friendshipId);
        if (request is not null)
        {
            Requests.Remove(request);
        }
    });

    [RelayCommand]
    private async Task RemoveFriendAsync(Guid userId)
    {
        var confirm = await Dialog.ConfirmAsync("Remove Friend", "Are you sure you want to remove this friend?", "Yes", "Cancel");
        if (!confirm)
        {
            return;
        }

        await RunAsync(async () =>
        {
            await _api.RemoveFriendAsync(userId);

            var friend = Friends.FirstOrDefault(f => f.User.Id == userId);
            if (friend is not null)
            {
                Friends.Remove(friend);
            }
        });
    }

    [RelayCommand]
    private Task OpenTeamsAsync() => Shell.Current.GoToAsync("teams");

    /// <summary>Tapping any player row (search result or friend) opens their profile.</summary>
    [RelayCommand]
    private Task OpenProfileAsync(Guid userId) => Shell.Current.GoToAsync($"playerprofile?userId={userId}");
}
