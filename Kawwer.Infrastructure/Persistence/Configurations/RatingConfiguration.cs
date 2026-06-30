using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("ratings");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Comment).HasMaxLength(500);
        builder.HasIndex(r => new { r.MatchId, r.RaterId, r.RateeId, r.Type }).IsUnique();
        builder.HasIndex(r => r.RateeId);
    }
}
