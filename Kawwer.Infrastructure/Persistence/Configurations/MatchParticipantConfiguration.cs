using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class MatchParticipantConfiguration : IEntityTypeConfiguration<MatchParticipant>
{
    public void Configure(EntityTypeBuilder<MatchParticipant> builder)
    {
        builder.ToTable("match_participants");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaidAmount).HasPrecision(10, 2);
        builder.Property(p => p.Latitude).HasPrecision(9, 6);
        builder.Property(p => p.Longitude).HasPrecision(9, 6);

        builder.HasIndex(p => new { p.MatchId, p.UserId }).IsUnique();
        builder.HasIndex(p => p.UserId);
    }
}
