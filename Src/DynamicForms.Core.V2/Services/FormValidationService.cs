using DynamicForms.Core.V2.Runtime;
using DynamicForms.Core.V2.Validation;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Core.V2.Services;

/// <summary>
/// Implementation of validation service for form modules and fields
/// </summary>
public class FormValidationService : IFormValidationService
{
    private readonly ILogger<FormValidationService> _logger;
    private readonly Dictionary<string, IValidationRule> _rules = new();

    public FormValidationService(ILogger<FormValidationService> logger)
    {
        _logger = logger;

        // Register built-in validation rules
        RegisterRule("required", new RequiredFieldRule());
        RegisterRule("length", new LengthValidationRule());
        RegisterRule("pattern", new PatternValidationRule());
        RegisterRule("email", new EmailValidationRule());

        _logger.LogDebug("FormValidationService initialized with {RuleCount} built-in rules", _rules.Count);
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateModuleAsync(
        FormModuleRuntime module,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating module '{ModuleTitle}' with {FieldCount} fields",
            module.Schema.TitleEn, module.Metrics.TotalFields);

        var allErrors = new List<ValidationError>();

        // Validate each field in the module
        foreach (var field in module.GetFieldsInOrder())
        {
            formData.TryGetValue(field.Schema.Id, out var value);

            var result = await ValidateFieldAsync(field, value, formData, cancellationToken);

            if (!result.IsValid)
            {
                allErrors.AddRange(result.Errors);
            }
        }

        if (allErrors.Any())
        {
            _logger.LogWarning("Module validation failed with {ErrorCount} errors", allErrors.Count);
            return new ValidationResult(false, allErrors);
        }

        _logger.LogDebug("Module validation succeeded");
        return ValidationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateFieldAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        // 1. Check IsRequired - run required rule
        if (field.Schema.IsRequired && _rules.TryGetValue("required", out var requiredRule))
        {
            var result = await requiredRule.ValidateAsync(field, value, formData, cancellationToken);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
                // If required validation fails, skip other validations
                return new ValidationResult(false, errors);
            }
        }

        // Skip further validation if value is empty and not required
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success();
        }

        // 2. Check MinLength/MaxLength - run length rule
        if ((field.Schema.MinLength.HasValue || field.Schema.MaxLength.HasValue) &&
            _rules.TryGetValue("length", out var lengthRule))
        {
            var result = await lengthRule.ValidateAsync(field, value, formData, cancellationToken);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }
        }

        // 3. Check Pattern - run pattern rule
        if (!string.IsNullOrWhiteSpace(field.Schema.Pattern) &&
            _rules.TryGetValue("pattern", out var patternRule))
        {
            var result = await patternRule.ValidateAsync(field, value, formData, cancellationToken);
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors);
            }
        }

        // 4. Check ValidationRules array - run each rule by ID
        if (field.Schema.ValidationRules != null)
        {
            foreach (var ruleId in field.Schema.ValidationRules)
            {
                if (_rules.TryGetValue(ruleId, out var rule))
                {
                    var result = await rule.ValidateAsync(field, value, formData, cancellationToken);
                    if (!result.IsValid)
                    {
                        errors.AddRange(result.Errors);
                    }
                }
                else
                {
                    _logger.LogWarning("Validation rule '{RuleId}' not found for field '{FieldId}'",
                        ruleId, field.Schema.Id);
                }
            }
        }

        return errors.Any()
            ? new ValidationResult(false, errors)
            : ValidationResult.Success();
    }

    /// <inheritdoc/>
    public void RegisterRule(string ruleId, IValidationRule rule)
    {
        _rules[ruleId] = rule;
        _logger.LogInformation("Registered validation rule: {RuleId}", ruleId);
    }
}
