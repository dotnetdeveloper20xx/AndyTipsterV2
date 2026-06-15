namespace AndyTipster.Domain.Exceptions;

/// <summary>
/// Thrown when validation fails with field-specific error messages.
/// Maps to HTTP 400 with errors dictionary.
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

/// <summary>
/// Thrown when a requested resource is not found.
/// Maps to HTTP 404.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a business rule is violated (e.g., invalid state transition).
/// Maps to HTTP 409 Conflict or 422 Unprocessable Entity.
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}
