using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Sport).HasDefaultValue(Kawwer.Domain.Enums.SportType.Football);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(150);
        builder.Property(m => m.Description).HasMaxLength(1000);
        builder.Property(m => m.TotalFieldPrice).HasPrecision(10, 2);
        builder.Property(m => m.ReservationPaid).HasPrecision(10, 2);
        builder.Property(m => m.OpponentName).HasMaxLength(100);

        builder.HasIndex(m => m.OrganizerId);
        builder.HasIndex(m => m.FootballFieldId);
        builder.HasIndex(m => new { m.Visibility, m.Status, m.MatchDate });

        builder.HasMany(m => m.Participants)
            .WithOne()
            .HasForeignKey(p => p.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Match.Participants))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(m => m.Guests)
            .WithOne()
            .HasForeignKey(g => g.MatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Match.Guests))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
