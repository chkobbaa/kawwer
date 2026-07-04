using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("push_subscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Endpoint).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.P256dh).IsRequired().HasMaxLength(255);
        builder.Property(s => s.Auth).IsRequired().HasMaxLength(255);

        // A push endpoint identifies exactly one browser subscription, so it must be unique.
        builder.HasIndex(s => s.Endpoint).IsUnique();
        builder.HasIndex(s => s.UserId);
    }
}
