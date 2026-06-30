namespace Kawwer.Application.Common.Exceptions;

/// <summary>Thrown when a user attempts an action they are not allowed to perform. Maps to HTTP 403.</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
