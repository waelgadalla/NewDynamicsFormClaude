using System.Text.RegularExpressions;
using DynamicForms.Core.V2.Runtime;

namespace DynamicForms.Core.V2.Validation;

/// <summary>
/// Validates that a required field has a value
/// </summary>
public class RequiredFieldRule : IValidationRule
{
    public string RuleId => "required";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (!field.Schema.IsRequired)
            return Task.FromResult(ValidationResult.Success());

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(
                    field.Schema.Id,
                    "REQUIRED",
                    $"{field.Schema.LabelEn ?? field.Schema.Id} is required",
                    $"{field.Schema.LabelFr ?? field.Schema.LabelEn} est requis"
                )
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

/// <summary>
/// Validates field value length against min/max constraints
/// </summary>
public class LengthValidationRule : IValidationRule
{
    public string RuleId => "length";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (value == null)
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value.ToString() ?? string.Empty;
        var errors = new List<ValidationError>();

        if (field.Schema.MinLength.HasValue && stringValue.Length < field.Schema.MinLength.Value)
        {
            errors.Add(new ValidationError(
                field.Schema.Id,
                "MIN_LENGTH",
                $"{field.Schema.LabelEn ?? field.Schema.Id} must be at least {field.Schema.MinLength} characters",
                $"{field.Schema.LabelFr ?? field.Schema.LabelEn} doit contenir au moins {field.Schema.MinLength} caractères"
            ));
        }

        if (field.Schema.MaxLength.HasValue && stringValue.Length > field.Schema.MaxLength.Value)
        {
            errors.Add(new ValidationError(
                field.Schema.Id,
                "MAX_LENGTH",
                $"{field.Schema.LabelEn ?? field.Schema.Id} must not exceed {field.Schema.MaxLength} characters",
                $"{field.Schema.LabelFr ?? field.Schema.LabelEn} ne doit pas dépasser {field.Schema.MaxLength} caractères"
            ));
        }

        return errors.Any()
            ? Task.FromResult(ValidationResult.Failure(errors.ToArray()))
            : Task.FromResult(ValidationResult.Success());
    }
}

/// <summary>
/// Validates field value against a regular expression pattern
/// </summary>
public class PatternValidationRule : IValidationRule
{
    public string RuleId => "pattern";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (value == null || string.IsNullOrWhiteSpace(field.Schema.Pattern))
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value.ToString() ?? string.Empty;

        try
        {
            var regex = new Regex(field.Schema.Pattern);
            if (!regex.IsMatch(stringValue))
            {
                return Task.FromResult(ValidationResult.Failure(
                    new ValidationError(
                        field.Schema.Id,
                        "PATTERN_MISMATCH",
                        $"{field.Schema.LabelEn ?? field.Schema.Id} format is invalid",
                        $"Le format de {field.Schema.LabelFr ?? field.Schema.LabelEn} est invalide"
                    )
                ));
            }
        }
        catch (Exception)
        {
            // Invalid regex pattern - log but don't fail validation
            return Task.FromResult(ValidationResult.Success());
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

/// <summary>
/// Validates that a field contains a valid email address
/// </summary>
public class EmailValidationRule : IValidationRule
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string RuleId => "email";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (value == null)
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(stringValue))
            return Task.FromResult(ValidationResult.Success());

        if (!EmailRegex.IsMatch(stringValue))
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(
                    field.Schema.Id,
                    "INVALID_EMAIL",
                    $"{field.Schema.LabelEn ?? field.Schema.Id} must be a valid email address",
                    $"{field.Schema.LabelFr ?? field.Schema.LabelEn} doit être une adresse courriel valide"
                )
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}
