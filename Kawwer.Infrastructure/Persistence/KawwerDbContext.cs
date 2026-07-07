using Kawwer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kawwer.Infrastructure.Persistence;

public sealed class KawwerDbContext : DbContext
{
    public KawwerDbContext(DbContextOptions<KawwerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<FootballField> FootballFields => Set<FootballField>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchParticipant> MatchParticipants => Set<MatchParticipant>();
    public DbSet<GuestPlayer> GuestPlayers => Set<GuestPlayer>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KawwerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        // All domain entities generate their own Guid IDs (Guid.NewGuid() in Entity base class),
        // so the store never generates values. Without this, EF treats entities discovered through
        // navigation collections (e.g. new MatchParticipant added to Match._participants) as
        // already-existing rows and emits UPDATE instead of INSERT, causing
        // DbUpdateConcurrencyException when the row doesn't actually exist yet.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var pk = entityType.FindPrimaryKey();
            if (pk is null) continue;

            foreach (var prop in pk.Properties)
            {
                if (prop.ClrType == typeof(Guid))
                {
                    prop.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
                }
            }
        }
    }
}
