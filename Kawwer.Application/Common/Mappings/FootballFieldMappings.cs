using Kawwer.Contracts.FootballFields;
using Kawwer.Domain.Entities;

namespace Kawwer.Application.Common.Mappings;

public static class FootballFieldMappings
{
    public static FootballFieldDto ToDto(this FootballField field) => new(
        field.Id,
        field.Name,
        field.Address,
        field.Latitude,
        field.Longitude,
        field.Capacity,
        field.MatchDurationMinutes,
        field.Price,
        field.ReservationFee,
        field.Surface,
        field.Indoor,
        field.Parking,
        field.Shower,
        field.Lights,
        field.PhoneNumber,
        field.GoogleMapsUrl,
        field.Notes,
        field.CreatedBy,
        field.CreatedAt);
}
