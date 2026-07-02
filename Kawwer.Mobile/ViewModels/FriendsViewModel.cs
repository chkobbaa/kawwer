using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class FriendsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public FriendsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Friends";
    }

    public ObservableCollection<FriendDto> Friends { get; } = new();
    public ObservableCollection<FriendRequestDto> Requests { get; } = new();
    public ObservableCollection<UserSummaryDto> SearchResults { get; } = new();

    [ObservableProperty] private string _searchTerm = string.Empty;

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
        await Shell.Current.DisplayAlertAsync("Friends", "Friend request sent.", "OK");
    });

    [RelayCommand]
    private Task AcceptAsync(Guid friendshipId) => RunAsync(async () =>
    {
        await _api.AcceptFriendRequestAsync(friendshipId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task RejectAsync(Guid friendshipId) => RunAsync(async () =>
    {
        await _api.RejectFriendRequestAsync(friendshipId);
        await LoadCoreAsync();
    });

    [RelayCommand]
    private Task RemoveFriendAsync(Guid userId) => RunAsync(async () =>
    {
        var confirm = await Shell.Current.DisplayAlert("Remove Friend", "Are you sure you want to remove this friend?", "Yes", "Cancel");
        if (confirm)
        {
            await _api.RemoveFriendAsync(userId);
            await LoadCoreAsync();
        }
    });

    [RelayCommand]
    private Task OpenGroupsAsync() => Shell.Current.GoToAsync("groups");

    /// <summary>Tapping any player row (search result or friend) opens their profile.</summary>
    [RelayCommand]
    private Task OpenProfileAsync(Guid userId) => Shell.Current.GoToAsync($"playerprofile?userId={userId}");
}
