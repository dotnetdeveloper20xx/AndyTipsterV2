using System.Text.RegularExpressions;

namespace AndyTipster.Application.Auth.Validators;

/// <summary>
/// Validates password complexity requirements:
/// - At least 8 characters
/// - At least one uppercase letter
/// - At least one lowercase letter
/// - At least one digit
/// - At least one special character
/// Returns specific error messages for each failed rule.
/// </summary>
public static class PasswordValidator
{
    public static PasswordValidationResult Validate(string password)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password) || password.Length < 8)
        {
            errors.Add("Password must be at least 8 characters long.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }

        if (string.IsNullOrEmpty(password) || !password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }

        if (string.IsNullOrEmpty(password) || !Regex.IsMatch(password ?? string.Empty, @"[^a-zA-Z0-9]"))
        {
            errors.Add("Password must contain at least one special character.");
        }

        return new PasswordValidationResult(errors.Count == 0, errors);
    }
}

public record PasswordValidationResult(bool IsValid, List<string> Errors);
