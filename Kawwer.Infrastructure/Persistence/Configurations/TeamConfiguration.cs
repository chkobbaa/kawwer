using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Description).HasMaxLength(250);
        builder.HasIndex(t => t.OwnerId);

        // Map the private _members backing field exposed via the read-only Members property.
        builder.HasMany(t => t.Members)
            .WithOne()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Team.Members))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
