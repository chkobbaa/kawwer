namespace Kawwer.Application.Common.Interfaces;

/// <summary>Abstraction over the system clock to keep handlers testable.</summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
