using System.ComponentModel;
using System.Globalization;
using Kawwer.Mobile.ViewModels;

namespace Kawwer.Mobile.Views;

public partial class MatchDetailsPage : ContentPage
{
    private readonly MatchDetailsViewModel _viewModel;
    private string? _renderedMapKey;

    public MatchDetailsPage(MatchDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MatchDetailsViewModel.Match))
        {
            RenderFieldMap();
        }
    }

    /// <summary>
    /// Renders a small read-only OpenStreetMap (Leaflet) with a pin on the field.
    /// Rebuilt only when the coordinates actually change to avoid WebView flicker on refresh.
    /// </summary>
    private void RenderFieldMap()
    {
        if (_viewModel.Match?.Field is not { } field)
        {
            return;
        }

        var lat = field.Latitude.ToString(CultureInfo.InvariantCulture);
        var lng = field.Longitude.ToString(CultureInfo.InvariantCulture);
        var key = $"{lat},{lng}";
        if (key == _renderedMapKey)
        {
            return;
        }

        _renderedMapKey = key;
        var html = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
                <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                <style>html, body, #map { margin:0; padding:0; height:100%; }</style>
            </head>
            <body>
                <div id="map"></div>
                <script>
                    var map = L.map('map', {
                        zoomControl: false,
                        dragging: false,
                        scrollWheelZoom: false,
                        doubleClickZoom: false,
                        touchZoom: false,
                        boxZoom: false,
                        keyboard: false,
                        attributionControl: true
                    }).setView([{{lat}}, {{lng}}], 15);
                    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                        attribution: '&copy; OpenStreetMap contributors'
                    }).addTo(map);
                    L.marker([{{lat}}, {{lng}}]).addTo(map);
                </script>
            </body>
            </html>
            """;

        FieldMapView.Source = new HtmlWebViewSource { Html = html };
    }
}
