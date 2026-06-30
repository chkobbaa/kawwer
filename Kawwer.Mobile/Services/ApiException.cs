namespace Kawwer.Mobile.Services;

/// <summary>Raised when an API call returns a non-success envelope.</summary>
public sealed class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
