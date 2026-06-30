using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class FootballFieldConfiguration : IEntityTypeConfiguration<FootballField>
{
    public void Configure(EntityTypeBuilder<FootballField> builder)
    {
        builder.ToTable("football_fields");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name).IsRequired().HasMaxLength(150);
        builder.Property(f => f.Address).IsRequired().HasMaxLength(300);
        builder.Property(f => f.Latitude).HasPrecision(9, 6);
        builder.Property(f => f.Longitude).HasPrecision(9, 6);
        builder.Property(f => f.Price).HasPrecision(10, 2);
        builder.Property(f => f.ReservationFee).HasPrecision(10, 2);
        builder.Property(f => f.PhoneNumber).HasMaxLength(30);
        builder.Property(f => f.GoogleMapsUrl).HasMaxLength(500);
        builder.Property(f => f.Notes).HasMaxLength(1000);

        builder.HasIndex(f => f.CreatedBy);
    }
}
