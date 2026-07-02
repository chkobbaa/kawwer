using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.Views;

/// <summary>
/// Full-screen OpenStreetMap picker (Leaflet in a WebView, no API key needed).
/// Search a place, tap an indexed spot, or hold ~1 second for an exact location,
/// then confirm. The result is published as a <see cref="MapLocationPickedMessage"/>.
/// </summary>
public partial class MapPickerPage : ContentPage
{
    private bool _loaded;
    private bool _confirmed;

    public MapPickerPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_loaded)
        {
            return;
        }

        _loaded = true;

        using var stream = await FileSystem.OpenAppPackageFileAsync("map_picker.html");
        using var reader = new StreamReader(stream);
        MapView.Source = new HtmlWebViewSource { Html = await reader.ReadToEndAsync() };
        Loading.IsVisible = false;
        Loading.IsRunning = false;

        // Best effort: center the map on the device position.
        try
        {
            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            if (location is not null)
            {
                var lat = location.Latitude.ToString(CultureInfo.InvariantCulture);
                var lng = location.Longitude.ToString(CultureInfo.InvariantCulture);
                await MapView.EvaluateJavaScriptAsync($"centerOn({lat},{lng})");
            }
        }
        catch
        {
            // The default map view is fine without it.
        }
    }

    private async void OnNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (!e.Url.StartsWith("kawwer://", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        e.Cancel = true;
        if (_confirmed)
        {
            return;
        }

        var query = ParseQuery(e.Url);
        if (!query.TryGetValue("lat", out var latRaw) || !query.TryGetValue("lng", out var lngRaw)
            || !double.TryParse(latRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
            || !double.TryParse(lngRaw, NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
        {
            return;
        }

        _confirmed = true;
        query.TryGetValue("name", out var name);
        query.TryGetValue("address", out var address);

        WeakReferenceMessenger.Default.Send(new MapLocationPickedMessage(lat, lng, name, address));
        await Shell.Current.GoToAsync("..");
    }

    private static Dictionary<string, string> ParseQuery(string url)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var queryStart = url.IndexOf('?');
        if (queryStart < 0)
        {
            return result;
        }

        foreach (var pair in url[(queryStart + 1)..].Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator > 0)
            {
                result[pair[..separator]] = Uri.UnescapeDataString(pair[(separator + 1)..]);
            }
        }

        return result;
    }
}
