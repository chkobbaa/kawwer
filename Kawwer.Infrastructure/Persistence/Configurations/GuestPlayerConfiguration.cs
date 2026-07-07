using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class GuestPlayerConfiguration : IEntityTypeConfiguration<GuestPlayer>
{
    public void Configure(EntityTypeBuilder<GuestPlayer> builder)
    {
        builder.ToTable("match_guest_players");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name).IsRequired().HasMaxLength(60);

        builder.HasIndex(g => g.MatchId);
    }
}
