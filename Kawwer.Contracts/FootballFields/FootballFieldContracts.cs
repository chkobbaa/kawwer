using Kawwer.Domain.Enums;

namespace Kawwer.Contracts.FootballFields;

public sealed record CreateFootballFieldRequest(
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    int Capacity,
    int MatchDurationMinutes,
    decimal Price,
    decimal ReservationFee,
    SurfaceType Surface,
    bool Indoor,
    bool Parking,
    bool Shower,
    bool Lights,
    string? PhoneNumber,
    string? GoogleMapsUrl,
    string? Notes);

public sealed record UpdateFootballFieldRequest(
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    int Capacity,
    int MatchDurationMinutes,
    decimal Price,
    decimal ReservationFee,
    SurfaceType Surface,
    bool Indoor,
    bool Parking,
    bool Shower,
    bool Lights,
    string? PhoneNumber,
    string? GoogleMapsUrl,
    string? Notes);

public sealed record FootballFieldDto(
    Guid Id,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    int Capacity,
    int MatchDurationMinutes,
    decimal Price,
    decimal ReservationFee,
    SurfaceType Surface,
    bool Indoor,
    bool Parking,
    bool Shower,
    bool Lights,
    string? PhoneNumber,
    string? GoogleMapsUrl,
    string? Notes,
    Guid CreatedBy,
    DateTime CreatedAt);
