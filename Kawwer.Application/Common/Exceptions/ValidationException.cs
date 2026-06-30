using FluentValidation.Results;

namespace Kawwer.Application.Common.Exceptions;

/// <summary>Thrown when one or more validation rules fail.</summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("One or more validation failures occurred.")
    {
        Errors = failures.Select(f => f.ErrorMessage).ToArray();
    }

    public ValidationException(string message) : base("One or more validation failures occurred.")
    {
        Errors = new[] { message };
    }

    public IReadOnlyList<string> Errors { get; }
}
