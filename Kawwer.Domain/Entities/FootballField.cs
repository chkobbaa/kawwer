using Kawwer.Domain.Common;
using Kawwer.Domain.Enums;

namespace Kawwer.Domain.Entities;

/// <summary>
/// A reusable location where matches are played. Price/fee changes affect only future matches,
/// because each match snapshots the price at creation time.
/// </summary>
public class FootballField : AggregateRoot
{
    private FootballField()
    {
        Name = string.Empty;
        Address = string.Empty;
    }

    public FootballField(
        string name,
        string address,
        decimal latitude,
        decimal longitude,
        int capacity,
        int matchDurationMinutes,
        decimal price,
        decimal reservationFee,
        SurfaceType surface,
        bool indoor,
        bool parking,
        bool shower,
        bool lights,
        Guid createdBy,
        string? phoneNumber = null,
        string? googleMapsUrl = null,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        Capacity = capacity;
        MatchDurationMinutes = matchDurationMinutes;
        Price = price;
        ReservationFee = reservationFee;
        Surface = surface;
        Indoor = indoor;
        Parking = parking;
        Shower = shower;
        Lights = lights;
        CreatedBy = createdBy;
        PhoneNumber = phoneNumber;
        GoogleMapsUrl = googleMapsUrl;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string Name { get; private set; }
    public string Address { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public int Capacity { get; private set; }
    public int MatchDurationMinutes { get; private set; }
    public decimal Price { get; private set; }
    public decimal ReservationFee { get; private set; }
    public SurfaceType Surface { get; private set; }
    public bool Indoor { get; private set; }
    public bool Parking { get; private set; }
    public bool Shower { get; private set; }
    public bool Lights { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? GoogleMapsUrl { get; private set; }
    public string? Notes { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Update(
        string name,
        string address,
        decimal latitude,
        decimal longitude,
        int capacity,
        int matchDurationMinutes,
        decimal price,
        decimal reservationFee,
        SurfaceType surface,
        bool indoor,
        bool parking,
        bool shower,
        bool lights,
        string? phoneNumber,
        string? googleMapsUrl,
        string? notes)
    {
        Name = name;
        Address = address;
        Latitude = latitude;
        Longitude = longitude;
        Capacity = capacity;
        MatchDurationMinutes = matchDurationMinutes;
        Price = price;
        ReservationFee = reservationFee;
        Surface = surface;
        Indoor = indoor;
        Parking = parking;
        Shower = shower;
        Lights = lights;
        PhoneNumber = phoneNumber;
        GoogleMapsUrl = googleMapsUrl;
        Notes = notes;
        UpdatedAt = DateTime.UtcNow;
    }
}
