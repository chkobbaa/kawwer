using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class DiscoverViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public DiscoverViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Discover";
    }

    public ObservableCollection<DiscoverMatchDto> Matches { get; } = new();

    [ObservableProperty] private bool _isEmpty;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        double? lat = null, lng = null;
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location is not null)
            {
                lat = location.Latitude;
                lng = location.Longitude;
            }
        }
        catch
        {
            // Location is optional; fall back to an unfiltered search.
        }

        var result = await _api.DiscoverAsync(lat, lng, 25);
        Matches.Clear();
        foreach (var match in result.Items)
        {
            Matches.Add(match);
        }

        IsEmpty = Matches.Count == 0;
    });

    [RelayCommand]
    private async Task JoinAsync(Guid matchId)
    {
        await RunAsync(async () =>
        {
            var joined = await _api.JoinPublicMatchAsync(matchId);
            await Shell.Current.DisplayAlertAsync(
                "Join request",
                joined ? "You joined the match!" : "Your request was sent for approval.",
                "OK");
        });
    }

    [RelayCommand]
    private Task OpenMatchAsync(Guid matchId) => Shell.Current.GoToAsync($"matchdetails?matchId={matchId}");
}
