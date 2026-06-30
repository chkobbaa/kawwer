using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Kawwer.Infrastructure.Persistence;

/// <summary>
/// Lets the EF Core tools (migrations) create the context without booting the API.
/// Reads the connection string from KAWWER_DB_CONNECTION when present.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KawwerDbContext>
{
    public KawwerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("KAWWER_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=kawwer;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<KawwerDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new KawwerDbContext(options);
    }
}
