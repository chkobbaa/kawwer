using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kawwer.Infrastructure.Persistence.Configurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("friendships");
        builder.HasKey(f => f.Id);

        builder.HasIndex(f => new { f.UserId, f.FriendId }).IsUnique();
        builder.HasIndex(f => f.FriendId);
    }
}
