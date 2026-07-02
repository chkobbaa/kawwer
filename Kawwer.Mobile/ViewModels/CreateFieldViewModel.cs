using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Kawwer.Mobile.Models;
using Kawwer.Mobile.Services;

namespace Kawwer.Mobile.ViewModels;

public sealed partial class CreateFieldViewModel : BaseViewModel
{
    private readonly KawwerApiClient _api;

    public CreateFieldViewModel(KawwerApiClient api)
    {
        _api = api;
        Title = "New field";

        // Result callback from the map picker page.
        WeakReferenceMessenger.Default.Register<MapLocationPickedMessage>(this, static (recipient, message) =>
        {
            var vm = (CreateFieldViewModel)recipient;
            vm.Latitude = message.Latitude.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            vm.Longitude = message.Longitude.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(vm.Address) && !string.IsNullOrWhiteSpace(message.Address))
            {
                vm.Address = message.Address;
            }

            if (string.IsNullOrWhiteSpace(vm.Name) && !string.IsNullOrWhiteSpace(message.Name))
            {
                vm.Name = message.Name;
            }

            vm.LocationCaptured = true;
        });
    }

    // Typical values from docs/FootballField.md.
    public ObservableCollection<int> CapacityOptions { get; } = new() { 10, 12, 14, 16, 22 };
    public ObservableCollection<int> DurationOptions { get; } = new() { 60, 90, 120 };
    public ObservableCollection<string> SurfaceOptions { get; } = new() { "Artificial turf", "Natural grass", "Concrete" };

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _address = string.Empty;
    [ObservableProperty] private string _latitude = string.Empty;
    [ObservableProperty] private string _longitude = string.Empty;
    [ObservableProperty] private int _capacity = 14;
    [ObservableProperty] private int _durationMinutes = 90;
    [ObservableProperty] private string _price = string.Empty;
    [ObservableProperty] private string _reservationFee = string.Empty;
    [ObservableProperty] private string _selectedSurface = "Artificial turf";
    [ObservableProperty] private bool _indoor;
    [ObservableProperty] private bool _parking;
    [ObservableProperty] private bool _shower;
    [ObservableProperty] private bool _lights = true;
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private string _googleMapsUrl = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _locationCaptured;

    [RelayCommand]
    private Task PickOnMapAsync() => Shell.Current.GoToAsync("mappicker");

    [RelayCommand]
    private Task UseMyLocationAsync() => RunAsync(async () =>
    {
        var location = await Geolocation.Default.GetLastKnownLocationAsync()
                       ?? await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
        if (location is null)
        {
            ErrorMessage = "Could not read your location. Enter coordinates manually or paste a Google Maps link.";
            return;
        }

        Latitude = location.Latitude.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
        Longitude = location.Longitude.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
        LocationCaptured = true;
    });

    [RelayCommand]
    private Task SaveAsync() => RunAsync(async () =>
    {
        if (Name.Trim().Length < 2)
        {
            ErrorMessage = "Give the field a name.";
            return;
        }

        if (Address.Trim().Length < 5)
        {
            ErrorMessage = "Enter the full address.";
            return;
        }

        if (!TryParseDecimal(Latitude, out var lat) || !TryParseDecimal(Longitude, out var lng))
        {
            ErrorMessage = "Set the GPS position: tap 'Use my location' while at the field, or type the coordinates.";
            return;
        }

        if (!TryParseDecimal(Price, out var price) || price <= 0)
        {
            ErrorMessage = "Enter the full match price in TND.";
            return;
        }

        // Reservation fee is optional in the form; defaults to 0.
        TryParseDecimal(string.IsNullOrWhiteSpace(ReservationFee) ? "0" : ReservationFee, out var fee);
        if (fee < 0 || fee > price)
        {
            ErrorMessage = "The reservation fee can't exceed the match price.";
            return;
        }

        var surface = SelectedSurface switch
        {
            "Natural grass" => SurfaceType.NaturalGrass,
            "Concrete" => SurfaceType.Concrete,
            _ => SurfaceType.ArtificialTurf
        };

        await _api.CreateFieldAsync(new
        {
            name = Name.Trim(),
            address = Address.Trim(),
            latitude = lat,
            longitude = lng,
            capacity = Capacity,
            matchDurationMinutes = DurationMinutes,
            price,
            reservationFee = fee,
            surface,
            indoor = Indoor,
            parking = Parking,
            shower = Shower,
            lights = Lights,
            phoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            googleMapsUrl = string.IsNullOrWhiteSpace(GoogleMapsUrl) ? null : GoogleMapsUrl.Trim(),
            notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
        });

        await Shell.Current.DisplayAlertAsync("Field created", $"{Name.Trim()} is ready to host matches.", "OK");
        await Shell.Current.GoToAsync("..");
    });

    private static bool TryParseDecimal(string value, out decimal result) =>
        decimal.TryParse(value.Replace(',', '.'), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out result);
}
