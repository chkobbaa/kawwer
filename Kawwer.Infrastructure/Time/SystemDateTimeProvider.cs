using Kawwer.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Kawwer.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    // Kawwer is a Tunisia-first product; default to Tunis (UTC+1, no DST) when no override is set.
    private const string DefaultTimeZoneId = "Africa/Tunis";
    private static readonly TimeSpan DefaultOffset = TimeSpan.FromHours(1);

    public SystemDateTimeProvider(IConfiguration configuration)
    {
        AppTimeZone = ResolveTimeZone(configuration["App:TimeZone"] ?? DefaultTimeZoneId);
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public TimeZoneInfo AppTimeZone { get; }

    private static TimeZoneInfo ResolveTimeZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (TimeZoneNotFoundException)
        {
            // The IANA/Windows id isn't present on this host (e.g. a slim container). Fall back to
            // a fixed +1 zone so scheduling stays correct rather than silently reverting to UTC.
            return TimeZoneInfo.CreateCustomTimeZone("Kawwer/Default", DefaultOffset, "Kawwer Local", "Kawwer Local");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("Kawwer/Default", DefaultOffset, "Kawwer Local", "Kawwer Local");
        }
    }
}
