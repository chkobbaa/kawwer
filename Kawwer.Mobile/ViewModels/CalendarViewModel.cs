using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class CalendarViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public CalendarViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Calendar";
    }

    public ObservableCollection<MatchDto> Matches { get; } = new();

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var upcoming = await _api.GetUpcomingAsync();
        Matches.Clear();
        foreach (var match in upcoming.OrderBy(m => m.MatchDate).ThenBy(m => m.StartTime))
        {
            Matches.Add(match);
        }
    });

    [RelayCommand]
    private Task OpenMatchAsync(Guid matchId) => Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");
}
