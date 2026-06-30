using Kawwer.Application.Common.Interfaces;

namespace Kawwer.Infrastructure.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
