namespace Kawwer.Application.Common.Exceptions;

/// <summary>Thrown when a requested resource does not exist. Maps to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException For(string resource, object key)
        => new($"{resource} '{key}' was not found.");
}
