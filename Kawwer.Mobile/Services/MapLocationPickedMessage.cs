namespace Kawwer.Mobile.Services;

/// <summary>Sent by the map picker when the user confirms a location.</summary>
public sealed record MapLocationPickedMessage(double Latitude, double Longitude, string? Name, string? Address);
