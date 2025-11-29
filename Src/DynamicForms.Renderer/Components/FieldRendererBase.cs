using DynamicForms.Core.V2.Runtime;
using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace DynamicForms.Renderer.Components;

/// <summary>
/// Abstract base class for all field renderer components.
/// Provides common functionality for rendering form fields with localization,
/// validation, and styling support.
/// </summary>
public abstract class FieldRendererBase : ComponentBase
{
    // ========================================================================
    // PARAMETERS
    // ========================================================================

    /// <summary>
    /// The form field node containing the schema and hierarchy information.
    /// Required parameter.
    /// </summary>
    [Parameter]
    public FormFieldNode Node { get; set; } = null!;

    /// <summary>
    /// The current value of the field.
    /// Can be null for fields without a value.
    /// </summary>
    [Parameter]
    public object? Value { get; set; }

    /// <summary>
    /// List of validation errors for this field.
    /// Empty list if no errors.
    /// </summary>
    [Parameter]
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Event callback invoked when the field value changes.
    /// The new value is passed as the parameter.
    /// </summary>
    [Parameter]
    public EventCallback<object?> OnValueChanged { get; set; }

    /// <summary>
    /// Whether the field should be disabled (read-only).
    /// Disabled fields are rendered but cannot be edited.
    /// </summary>
    [Parameter]
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Optional additional CSS classes to apply to the field container.
    /// </summary>
    [Parameter]
    public string? AdditionalCssClass { get; set; }

    /// <summary>
    /// Current culture for localization (defaults to current UI culture).
    /// Can be overridden for testing or specific rendering requirements.
    /// </summary>
    [Parameter]
    public CultureInfo? Culture { get; set; }

    // ========================================================================
    // PROPERTIES
    // ========================================================================

    /// <summary>
    /// Gets the current culture to use for localization.
    /// Falls back to CurrentUICulture if not explicitly set.
    /// </summary>
    protected CultureInfo CurrentCulture =>
        Culture ?? CultureInfo.CurrentUICulture;

    /// <summary>
    /// Gets whether the current culture is French.
    /// </summary>
    protected bool IsFrench =>
        CurrentCulture.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the field schema from the node.
    /// Convenience property for cleaner code in derived classes.
    /// </summary>
    protected DynamicForms.Core.V2.Schemas.FormFieldSchema Schema => Node.Schema;

    /// <summary>
    /// Gets the unique field identifier.
    /// </summary>
    protected string FieldId => Schema.Id;

    /// <summary>
    /// Gets whether this field has any validation errors.
    /// </summary>
    protected bool HasErrors => Errors?.Count > 0;

    // ========================================================================
    // LOCALIZATION METHODS
    // ========================================================================

    /// <summary>
    /// Gets the localized label for the field.
    /// Returns French label if current culture is French and French label exists,
    /// otherwise returns English label.
    /// Falls back to field ID if no label is defined.
    /// </summary>
    /// <returns>The localized label text</returns>
    protected string GetLabel()
    {
        if (IsFrench && !string.IsNullOrWhiteSpace(Schema.LabelFr))
            return Schema.LabelFr;

        if (!string.IsNullOrWhiteSpace(Schema.LabelEn))
            return Schema.LabelEn;

        // Fallback to field ID if no label defined
        return Schema.Id;
    }

    /// <summary>
    /// Gets the localized help text for the field.
    /// Returns French help text if current culture is French and French help exists,
    /// otherwise returns English help text.
    /// Returns null if no help text is defined.
    /// </summary>
    /// <returns>The localized help text, or null if none exists</returns>
    protected string? GetHelpText()
    {
        if (IsFrench && !string.IsNullOrWhiteSpace(Schema.HelpFr))
            return Schema.HelpFr;

        return Schema.HelpEn;
    }

    /// <summary>
    /// Gets the localized description for the field.
    /// Returns French description if current culture is French and French description exists,
    /// otherwise returns English description.
    /// Returns null if no description is defined.
    /// </summary>
    /// <returns>The localized description, or null if none exists</returns>
    protected string? GetDescription()
    {
        if (IsFrench && !string.IsNullOrWhiteSpace(Schema.DescriptionFr))
            return Schema.DescriptionFr;

        return Schema.DescriptionEn;
    }

    /// <summary>
    /// Gets the localized placeholder text for input fields.
    /// Returns French placeholder if current culture is French and French placeholder exists,
    /// otherwise returns English placeholder.
    /// Returns null if no placeholder is defined.
    /// </summary>
    /// <returns>The localized placeholder text, or null if none exists</returns>
    protected string? GetPlaceholder()
    {
        if (IsFrench && !string.IsNullOrWhiteSpace(Schema.PlaceholderFr))
            return Schema.PlaceholderFr;

        return Schema.PlaceholderEn;
    }

    // ========================================================================
    // VALIDATION METHODS
    // ========================================================================

    /// <summary>
    /// Determines if this field is required.
    /// Checks the IsRequired property on the schema.
    /// </summary>
    /// <returns>True if the field requires a value; otherwise false</returns>
    protected bool IsRequired()
    {
        return Schema.IsRequired;
    }

    /// <summary>
    /// Gets the minimum length validation rule if defined.
    /// </summary>
    /// <returns>Minimum length, or null if not defined</returns>
    protected int? GetMinLength()
    {
        return Schema.MinLength;
    }

    /// <summary>
    /// Gets the maximum length validation rule if defined.
    /// </summary>
    /// <returns>Maximum length, or null if not defined</returns>
    protected int? GetMaxLength()
    {
        return Schema.MaxLength;
    }

    /// <summary>
    /// Gets the validation pattern (regex) if defined.
    /// </summary>
    /// <returns>Validation pattern, or null if not defined</returns>
    protected string? GetPattern()
    {
        return Schema.Pattern;
    }

    /// <summary>
    /// Gets whether this field has custom validation rules defined.
    /// </summary>
    /// <returns>True if validation rules exist; otherwise false</returns>
    protected bool HasValidationRules()
    {
        return Schema.ValidationRules?.Length > 0;
    }

    // ========================================================================
    // CSS CLASS METHODS
    // ========================================================================

    /// <summary>
    /// Builds the CSS class string for the field container.
    /// Includes base classes, validation state classes, and additional classes.
    /// </summary>
    /// <param name="baseClass">The base CSS class (default: "dynamic-field-group")</param>
    /// <returns>A space-separated string of CSS classes</returns>
    protected string GetCssClasses(string baseClass = "dynamic-field-group")
    {
        var classes = new List<string> { baseClass };

        // Add error state class
        if (HasErrors)
        {
            classes.Add("has-error");
        }

        // Add disabled state class
        if (IsDisabled)
        {
            classes.Add("field-disabled");
        }

        // Add required state class
        if (IsRequired())
        {
            classes.Add("field-required");
        }

        // Add field type class
        classes.Add($"field-type-{Schema.FieldType.ToLower()}");

        // Add additional custom classes
        if (!string.IsNullOrWhiteSpace(AdditionalCssClass))
        {
            classes.Add(AdditionalCssClass);
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Builds the CSS class string for the input element.
    /// Includes base input class and validation state.
    /// </summary>
    /// <param name="baseClass">The base CSS class (default: "form-control")</param>
    /// <returns>A space-separated string of CSS classes</returns>
    protected string GetInputCssClasses(string baseClass = "form-control")
    {
        var classes = new List<string> { baseClass, "dynamic-field-input" };

        // Add validation state class
        if (HasErrors)
        {
            classes.Add("is-invalid");
        }

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Builds the CSS class string for the label element.
    /// </summary>
    /// <returns>A space-separated string of CSS classes</returns>
    protected string GetLabelCssClasses()
    {
        var classes = new List<string> { "form-label", "dynamic-field-label" };

        if (IsRequired())
        {
            classes.Add("required");
        }

        return string.Join(" ", classes);
    }

    // ========================================================================
    // VALUE HANDLING METHODS
    // ========================================================================

    /// <summary>
    /// Handles value changes from input elements.
    /// Invokes the OnValueChanged callback if provided.
    /// </summary>
    /// <param name="newValue">The new value from the input</param>
    protected async Task HandleValueChanged(object? newValue)
    {
        if (OnValueChanged.HasDelegate)
        {
            await OnValueChanged.InvokeAsync(newValue);
        }
    }

    /// <summary>
    /// Gets the current value as a string.
    /// Returns empty string if value is null.
    /// </summary>
    /// <returns>String representation of the current value</returns>
    protected string GetValueAsString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the current value as a specific type.
    /// Returns default(T) if value is null or conversion fails.
    /// </summary>
    /// <typeparam name="T">The target type</typeparam>
    /// <returns>The value as type T, or default(T)</returns>
    protected T? GetValueAs<T>()
    {
        if (Value == null)
            return default;

        try
        {
            if (Value is T typedValue)
                return typedValue;

            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    // ========================================================================
    // RENDERING HELPERS
    // ========================================================================

    /// <summary>
    /// Gets whether this field should render a label.
    /// Container fields (Section, Tab, Panel) typically don't need labels.
    /// </summary>
    /// <returns>True if a label should be rendered; otherwise false</returns>
    protected virtual bool ShouldRenderLabel()
    {
        var containerTypes = new[] { "section", "tab", "panel", "group" };
        return !containerTypes.Contains(Schema.FieldType.ToLower());
    }

    /// <summary>
    /// Gets whether this field should render help text.
    /// </summary>
    /// <returns>True if help text exists and should be rendered; otherwise false</returns>
    protected bool ShouldRenderHelpText()
    {
        return !string.IsNullOrWhiteSpace(GetHelpText());
    }

    /// <summary>
    /// Gets whether this field should render validation errors.
    /// </summary>
    /// <returns>True if errors exist; otherwise false</returns>
    protected bool ShouldRenderErrors()
    {
        return HasErrors;
    }

    /// <summary>
    /// Generates a unique HTML ID for the field input element.
    /// Useful for label "for" attributes and accessibility.
    /// </summary>
    /// <returns>A unique HTML ID based on the field ID</returns>
    protected string GetHtmlId()
    {
        return $"field-{FieldId}";
    }

    /// <summary>
    /// Generates an ID for the help text element.
    /// Used for aria-describedby attributes.
    /// </summary>
    /// <returns>A unique HTML ID for the help text</returns>
    protected string GetHelpTextId()
    {
        return $"help-{FieldId}";
    }

    /// <summary>
    /// Generates an ID for the error message element.
    /// Used for aria-describedby attributes.
    /// </summary>
    /// <returns>A unique HTML ID for the error message</returns>
    protected string GetErrorId()
    {
        return $"error-{FieldId}";
    }

    // ========================================================================
    // LIFECYCLE METHODS
    // ========================================================================

    /// <summary>
    /// Called when parameters are set.
    /// Validates that required parameters are provided.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (Node == null)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} requires a Node parameter to be set.");
        }

        base.OnParametersSet();
    }
}
