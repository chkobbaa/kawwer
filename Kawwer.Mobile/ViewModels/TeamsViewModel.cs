using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class TeamsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public TeamsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Teams";
    }

    public ObservableCollection<TeamDto> Teams { get; } = new();

    [ObservableProperty] private string _newTeamName = string.Empty;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var teams = await _api.GetTeamsAsync();
        Teams.Clear();
        foreach (var t in teams)
        {
            Teams.Add(t);
        }
    });

    [RelayCommand]
    private Task CreateAsync() => RunAsync(async () =>
    {
        if (NewTeamName.Trim().Length < 2)
        {
            ErrorMessage = "Team name must be at least 2 characters.";
            return;
        }

        await _api.CreateTeamAsync(NewTeamName.Trim(), null);
        NewTeamName = string.Empty;
        await LoadAsync();
    });

    [RelayCommand]
    private Task DeleteAsync(Guid id) => RunAsync(async () =>
    {
        await _api.DeleteTeamAsync(id);
        await LoadAsync();
    });
}
