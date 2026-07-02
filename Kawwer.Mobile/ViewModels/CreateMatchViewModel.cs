using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class CreateMatchViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public CreateMatchViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Create match";
        MatchDate = DateTime.Today.AddDays(1);
        StartTime = new TimeSpan(20, 0, 0);
        MaxPlayers = 14;
    }

    public const string VisibilityEveryone = "Everyone";
    public const string VisibilityFriends = "Only friends";
    public const string VisibilityInvitations = "Invitations only";

    public ObservableCollection<FootballFieldDto> Fields { get; } = new();
    public ObservableCollection<SelectableUser> Friends { get; } = new();

    public ObservableCollection<string> VisibilityOptions { get; } =
        new() { VisibilityEveryone, VisibilityFriends, VisibilityInvitations };

    [ObservableProperty] private FootballFieldDto? _selectedField;
    [ObservableProperty] private string _matchTitle = "Football Match";
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private DateTime _matchDate;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private int _maxPlayers;
    [ObservableProperty] private string _selectedVisibility = VisibilityInvitations;

    public string VisibilityHint => SelectedVisibility switch
    {
        VisibilityEveryone => "Anyone nearby can find the match on Discover and join instantly.",
        VisibilityFriends => "Only your friends can find the match on Discover and join.",
        _ => "Hidden from Discover. Only the players you invite can join."
    };

    partial void OnSelectedVisibilityChanged(string value) => OnPropertyChanged(nameof(VisibilityHint));

    private MatchVisibility Visibility => SelectedVisibility switch
    {
        VisibilityEveryone => MatchVisibility.Public,
        VisibilityFriends => MatchVisibility.FriendsOnly,
        _ => MatchVisibility.Private
    };

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var fields = await _api.SearchFieldsAsync(null);
        Fields.Clear();
        foreach (var f in fields.Items)
        {
            Fields.Add(f);
        }

        SelectedField ??= Fields.FirstOrDefault();

        var friends = await _api.GetFriendsAsync();
        Friends.Clear();
        foreach (var f in friends)
        {
            Friends.Add(new SelectableUser(f.User));
        }
    });

    [RelayCommand]
    private Task OpenCreateFieldAsync() => Shell.Current.GoToAsync("createfield");

    [RelayCommand]
    private Task CreateAsync() => RunAsync(async () =>
    {
        if (SelectedField is null)
        {
            ErrorMessage = "Select a football field first.";
            return;
        }

        var invited = Friends.Where(f => f.IsSelected).Select(f => f.User.Id).ToList();
        if (Visibility == MatchVisibility.Private && invited.Count == 0)
        {
            ErrorMessage = "Invite at least one friend, or open the match to everyone or your friends.";
            return;
        }

        var matchId = await _api.CreateMatchAsync(new
        {
            footballFieldId = SelectedField.Id,
            title = MatchTitle,
            description = string.IsNullOrWhiteSpace(Description) ? null : Description,
            matchDate = MatchDate.ToString("yyyy-MM-dd"),
            startTime = StartTime.ToString(@"hh\:mm\:ss"),
            maxPlayers = MaxPlayers,
            totalFieldPrice = (decimal?)null,
            visibility = (int)Visibility,
            // Discoverable matches are first come, first served: players who tap Join get in
            // instantly (or land on the waiting list when the match is full). For friends-only
            // matches the API already restricts joining to the organizer's friends.
            autoAcceptPublic = Visibility != MatchVisibility.Private,
            invitedUserIds = invited,
            invitedGroupIds = Array.Empty<Guid>()
        });

        await Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");
    });
}
