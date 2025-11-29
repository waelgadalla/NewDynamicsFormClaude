using DynamicForms.Core.V2.Runtime;
using DynamicForms.Core.V2.Schemas;
using DynamicForms.Renderer.Models;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Renderer.Services;

/// <summary>
/// Service for evaluating conditional logic rules on form fields.
/// Determines field visibility and enabled states based on other field values.
/// </summary>
public class ConditionalLogicEngine
{
    private readonly ILogger<ConditionalLogicEngine> _logger;

    // Supported conditional operators
    private const string OperatorEquals = "equals";
    private const string OperatorNotEquals = "notEquals";
    private const string OperatorContains = "contains";
    private const string OperatorGreaterThan = "greaterThan";
    private const string OperatorLessThan = "lessThan";
    private const string OperatorIsEmpty = "isEmpty";
    private const string OperatorIsNotEmpty = "isNotEmpty";

    // Supported actions
    private const string ActionShow = "show";
    private const string ActionHide = "hide";
    private const string ActionEnable = "enable";
    private const string ActionDisable = "disable";
    private const string ActionRequire = "require";

    /// <summary>
    /// Initializes a new instance of the ConditionalLogicEngine.
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public ConditionalLogicEngine(ILogger<ConditionalLogicEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates a single conditional rule against form data.
    /// </summary>
    /// <param name="rule">The conditional rule to evaluate</param>
    /// <param name="formData">The current form data</param>
    /// <returns>True if the condition is met; otherwise false</returns>
    public bool EvaluateCondition(ConditionalRule rule, FormData formData)
    {
        if (rule == null)
        {
            _logger.LogWarning("Attempted to evaluate null conditional rule");
            return false;
        }

        if (formData == null)
        {
            _logger.LogWarning("Attempted to evaluate conditional rule with null form data");
            return false;
        }

        try
        {
            _logger.LogDebug("Evaluating condition: FieldId={FieldId}, Operator={Operator}, Value={Value}",
                rule.FieldId, rule.Operator, rule.Value);

            // Get the field value from form data
            var fieldValue = formData.GetValue(rule.FieldId);

            // Evaluate based on operator
            var result = rule.Operator.ToLower() switch
            {
                OperatorEquals => EvaluateEquals(fieldValue, rule.Value),
                OperatorNotEquals => EvaluateNotEquals(fieldValue, rule.Value),
                OperatorContains => EvaluateContains(fieldValue, rule.Value),
                OperatorGreaterThan => EvaluateGreaterThan(fieldValue, rule.Value),
                OperatorLessThan => EvaluateLessThan(fieldValue, rule.Value),
                OperatorIsEmpty => EvaluateIsEmpty(fieldValue),
                OperatorIsNotEmpty => EvaluateIsNotEmpty(fieldValue),
                _ => throw new NotSupportedException($"Operator '{rule.Operator}' is not supported")
            };

            _logger.LogDebug("Condition evaluation result: {Result}", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating conditional rule: FieldId={FieldId}, Operator={Operator}",
                rule.FieldId, rule.Operator);
            return false;
        }
    }

    /// <summary>
    /// Determines if a field should be visible based on its conditional rules.
    /// Returns true if the field should be shown.
    /// </summary>
    /// <param name="field">The field schema</param>
    /// <param name="formData">The current form data</param>
    /// <returns>True if the field should be visible; otherwise false</returns>
    public bool ShouldShowField(FormFieldSchema field, FormData formData)
    {
        if (field == null)
            return true; // Default to visible

        if (field.ConditionalRules == null || field.ConditionalRules.Length == 0)
            return true; // No rules means always visible

        try
        {
            // Evaluate all conditional rules
            foreach (var rule in field.ConditionalRules)
            {
                var conditionMet = EvaluateCondition(rule, formData);
                var action = rule.Action.ToLower();

                // Handle show/hide actions
                if (action == ActionShow && conditionMet)
                    return true;
                if (action == ActionHide && conditionMet)
                    return false;
            }

            // If no show/hide rules were triggered, default to visible
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining field visibility: FieldId={FieldId}", field.Id);
            return true; // Default to visible on error
        }
    }

    /// <summary>
    /// Determines if a field should be enabled based on its conditional rules.
    /// Returns true if the field should be enabled.
    /// </summary>
    /// <param name="field">The field schema</param>
    /// <param name="formData">The current form data</param>
    /// <returns>True if the field should be enabled; otherwise false</returns>
    public bool IsFieldEnabled(FormFieldSchema field, FormData formData)
    {
        if (field == null)
            return true; // Default to enabled

        if (field.ConditionalRules == null || field.ConditionalRules.Length == 0)
            return true; // No rules means always enabled

        try
        {
            // Evaluate all conditional rules
            foreach (var rule in field.ConditionalRules)
            {
                var conditionMet = EvaluateCondition(rule, formData);
                var action = rule.Action.ToLower();

                // Handle enable/disable actions
                if (action == ActionEnable && conditionMet)
                    return true;
                if (action == ActionDisable && conditionMet)
                    return false;
            }

            // If no enable/disable rules were triggered, default to enabled
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining field enabled state: FieldId={FieldId}", field.Id);
            return true; // Default to enabled on error
        }
    }

    /// <summary>
    /// Evaluates all conditional rules for an entire form and returns visibility states.
    /// This is the primary method for updating form state after field value changes.
    /// </summary>
    /// <param name="runtime">The form module runtime hierarchy</param>
    /// <param name="formData">The current form data</param>
    /// <returns>Dictionary of field IDs and their visibility states</returns>
    public Dictionary<string, bool> EvaluateAllConditions(FormModuleRuntime runtime, FormData formData)
    {
        var visibilityStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        if (runtime == null || formData == null)
        {
            _logger.LogWarning("Cannot evaluate conditions with null runtime or form data");
            return visibilityStates;
        }

        try
        {
            _logger.LogDebug("Evaluating all conditions for form");

            // Evaluate visibility for all fields
            foreach (var fieldNode in runtime.FieldNodes.Values)
            {
                var isVisible = ShouldShowField(fieldNode.Schema, formData);
                visibilityStates[fieldNode.Schema.Id] = isVisible;
            }

            _logger.LogDebug("Evaluated conditions for {Count} fields", visibilityStates.Count);
            return visibilityStates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating all conditions");
            return visibilityStates;
        }
    }

    /// <summary>
    /// Evaluates all conditional rules and returns both visibility and enabled states.
    /// </summary>
    /// <param name="runtime">The form module runtime hierarchy</param>
    /// <param name="formData">The current form data</param>
    /// <returns>Tuple of visibility states and enabled states</returns>
    public (Dictionary<string, bool> visibilityStates, Dictionary<string, bool> enabledStates)
        EvaluateAllConditionsComplete(FormModuleRuntime runtime, FormData formData)
    {
        var visibilityStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var enabledStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        if (runtime == null || formData == null)
        {
            _logger.LogWarning("Cannot evaluate conditions with null runtime or form data");
            return (visibilityStates, enabledStates);
        }

        try
        {
            _logger.LogDebug("Evaluating all conditions (visibility and enabled) for form");

            // Evaluate visibility and enabled state for all fields
            foreach (var fieldNode in runtime.FieldNodes.Values)
            {
                var isVisible = ShouldShowField(fieldNode.Schema, formData);
                var isEnabled = IsFieldEnabled(fieldNode.Schema, formData);

                visibilityStates[fieldNode.Schema.Id] = isVisible;
                enabledStates[fieldNode.Schema.Id] = isEnabled;
            }

            _logger.LogDebug("Evaluated conditions for {Count} fields", visibilityStates.Count);
            return (visibilityStates, enabledStates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating all conditions");
            return (visibilityStates, enabledStates);
        }
    }

    // ========================================================================
    // OPERATOR IMPLEMENTATIONS
    // ========================================================================

    /// <summary>
    /// Evaluates the Equals operator.
    /// Compares field value with rule value for equality.
    /// </summary>
    private bool EvaluateEquals(object? fieldValue, string? ruleValue)
    {
        if (fieldValue == null && ruleValue == null)
            return true;

        if (fieldValue == null || ruleValue == null)
            return false;

        // Try to compare as strings first (case-insensitive)
        var fieldStr = fieldValue.ToString();
        return string.Equals(fieldStr, ruleValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Evaluates the NotEquals operator.
    /// Compares field value with rule value for inequality.
    /// </summary>
    private bool EvaluateNotEquals(object? fieldValue, string? ruleValue)
    {
        return !EvaluateEquals(fieldValue, ruleValue);
    }

    /// <summary>
    /// Evaluates the Contains operator.
    /// Checks if the field value contains the rule value (case-insensitive).
    /// </summary>
    private bool EvaluateContains(object? fieldValue, string? ruleValue)
    {
        if (fieldValue == null || ruleValue == null)
            return false;

        var fieldStr = fieldValue.ToString();
        return fieldStr?.Contains(ruleValue, StringComparison.OrdinalIgnoreCase) ?? false;
    }

    /// <summary>
    /// Evaluates the GreaterThan operator.
    /// Compares numeric or date values.
    /// </summary>
    private bool EvaluateGreaterThan(object? fieldValue, string? ruleValue)
    {
        if (fieldValue == null || ruleValue == null)
            return false;

        try
        {
            // Try numeric comparison first
            if (TryCompareNumeric(fieldValue, ruleValue, out var numericResult))
                return numericResult > 0;

            // Try date comparison
            if (TryCompareDateTime(fieldValue, ruleValue, out var dateResult))
                return dateResult > 0;

            // Fall back to string comparison
            return string.Compare(fieldValue.ToString(), ruleValue, StringComparison.Ordinal) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error comparing values for GreaterThan: FieldValue={FieldValue}, RuleValue={RuleValue}",
                fieldValue, ruleValue);
            return false;
        }
    }

    /// <summary>
    /// Evaluates the LessThan operator.
    /// Compares numeric or date values.
    /// </summary>
    private bool EvaluateLessThan(object? fieldValue, string? ruleValue)
    {
        if (fieldValue == null || ruleValue == null)
            return false;

        try
        {
            // Try numeric comparison first
            if (TryCompareNumeric(fieldValue, ruleValue, out var numericResult))
                return numericResult < 0;

            // Try date comparison
            if (TryCompareDateTime(fieldValue, ruleValue, out var dateResult))
                return dateResult < 0;

            // Fall back to string comparison
            return string.Compare(fieldValue.ToString(), ruleValue, StringComparison.Ordinal) < 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error comparing values for LessThan: FieldValue={FieldValue}, RuleValue={RuleValue}",
                fieldValue, ruleValue);
            return false;
        }
    }

    /// <summary>
    /// Evaluates the IsEmpty operator.
    /// Checks if the field value is null, empty string, or whitespace.
    /// </summary>
    private bool EvaluateIsEmpty(object? fieldValue)
    {
        if (fieldValue == null)
            return true;

        if (fieldValue is string str)
            return string.IsNullOrWhiteSpace(str);

        return false;
    }

    /// <summary>
    /// Evaluates the IsNotEmpty operator.
    /// Checks if the field value is not null and not empty.
    /// </summary>
    private bool EvaluateIsNotEmpty(object? fieldValue)
    {
        return !EvaluateIsEmpty(fieldValue);
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Attempts to compare two values as numeric types.
    /// </summary>
    private bool TryCompareNumeric(object fieldValue, string ruleValue, out int result)
    {
        result = 0;

        try
        {
            // Try to parse both as decimal (covers int, long, double, float)
            if (decimal.TryParse(fieldValue.ToString(), out var fieldDecimal) &&
                decimal.TryParse(ruleValue, out var ruleDecimal))
            {
                result = fieldDecimal.CompareTo(ruleDecimal);
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to compare two values as DateTime types.
    /// </summary>
    private bool TryCompareDateTime(object fieldValue, string ruleValue, out int result)
    {
        result = 0;

        try
        {
            DateTime fieldDate;
            DateTime ruleDate;

            // Try to parse field value as DateTime
            if (fieldValue is DateTime dt)
            {
                fieldDate = dt;
            }
            else if (!DateTime.TryParse(fieldValue.ToString(), out fieldDate))
            {
                return false;
            }

            // Try to parse rule value as DateTime
            if (!DateTime.TryParse(ruleValue, out ruleDate))
            {
                return false;
            }

            result = fieldDate.CompareTo(ruleDate);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
