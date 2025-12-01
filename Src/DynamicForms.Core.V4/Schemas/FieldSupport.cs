namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Represents an option in a dropdown, radio button group, or checkbox list
/// </summary>
/// <param name="Value">The underlying value of the option (submitted with form data)</param>
/// <param name="LabelEn">English display label for the option</param>
/// <param name="LabelFr">French display label for the option (optional)</param>
/// <param name="IsDefault">Whether this option is selected by default</param>
/// <param name="Order">Display order for the option (lower values appear first)</param>
public record FieldOption(
    string Value,
    string LabelEn,
    string? LabelFr = null,
    bool IsDefault = false,
    int Order = 0
);
