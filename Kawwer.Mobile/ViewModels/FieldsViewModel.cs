using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class FieldsViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public FieldsViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "Football fields";
    }

    public ObservableCollection<FootballFieldDto> Fields { get; } = new();

    [ObservableProperty] private string _searchTerm = string.Empty;

    [RelayCommand]
    public Task LoadAsync() => RunAsync(async () =>
    {
        var result = await _api.SearchFieldsAsync(string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm.Trim());
        Fields.Clear();
        foreach (var field in result.Items)
        {
            Fields.Add(field);
        }
    });

    [RelayCommand]
    private Task SearchAsync() => LoadAsync();

    [RelayCommand]
    private Task CreateFieldAsync() => Shell.Current.GoToAsync("createfield");

    [RelayCommand]
    private async Task OpenMapsAsync(FootballFieldDto field)
    {
        // Prefer the owner-provided link; fall back to a coordinates search.
        var url = string.IsNullOrWhiteSpace(field.GoogleMapsUrl)
            ? $"https://www.google.com/maps/search/?api=1&query={field.Latitude},{field.Longitude}"
            : field.GoogleMapsUrl;
        try
        {
            await Launcher.Default.OpenAsync(url);
        }
        catch
        {
            ErrorMessage = "Could not open maps on this device.";
        }
    }
}
