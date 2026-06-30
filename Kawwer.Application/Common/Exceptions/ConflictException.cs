namespace Kawwer.Application.Common.Exceptions;

/// <summary>Thrown when an action conflicts with current state (e.g. duplicates). Maps to HTTP 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
