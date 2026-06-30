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

    public ObservableCollection<FootballFieldDto> Fields { get; } = new();
    public ObservableCollection<SelectableUser> Friends { get; } = new();

    [ObservableProperty] private FootballFieldDto? _selectedField;
    [ObservableProperty] private string _matchTitle = "Football Match";
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private DateTime _matchDate;
    [ObservableProperty] private TimeSpan _startTime;
    [ObservableProperty] private int _maxPlayers;
    [ObservableProperty] private bool _isPublic;

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
    private Task CreateAsync() => RunAsync(async () =>
    {
        if (SelectedField is null)
        {
            ErrorMessage = "Select a football field first.";
            return;
        }

        var invited = Friends.Where(f => f.IsSelected).Select(f => f.User.Id).ToList();
        if (!IsPublic && invited.Count == 0)
        {
            ErrorMessage = "Invite at least one friend, or make the match public.";
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
            visibility = IsPublic ? (int)MatchVisibility.Public : (int)MatchVisibility.Private,
            autoAcceptPublic = false,
            invitedUserIds = invited,
            invitedGroupIds = Array.Empty<Guid>()
        });

        await Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");
    });
}
