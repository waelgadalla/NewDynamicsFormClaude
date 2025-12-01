using System.Text.Json;
using DynamicForms.Core.V2.Enums;

namespace DynamicForms.Core.V2.Schemas;

/// <summary>
/// Immutable schema definition for a form field.
/// This is the core building block of the dynamic forms system.
/// Serializable to/from JSON for storage and transmission.
/// </summary>
public record FormFieldSchema
{
    #region Core Identity

    /// <summary>
    /// Unique identifier for the field (must be unique within a module)
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Type of the field (TextBox, DropDown, Section, DatePicker, FileUpload, etc.)
    /// </summary>
    public required string FieldType { get; init; }

    /// <summary>
    /// Display order relative to siblings (lower values appear first)
    /// </summary>
    public int Order { get; init; } = 1;

    /// <summary>
    /// Schema version for this field (supports evolution)
    /// </summary>
    public float Version { get; init; } = 1.0f;

    #endregion

    #region Hierarchy

    /// <summary>
    /// ID of the parent field (null for root-level fields)
    /// </summary>
    public string? ParentId { get; init; }

    /// <summary>
    /// Type of relationship to parent field
    /// </summary>
    public RelationshipType Relationship { get; init; } = RelationshipType.Container;

    #endregion

    #region Multilingual Text

    /// <summary>
    /// English label for the field
    /// </summary>
    public string? LabelEn { get; init; }

    /// <summary>
    /// French label for the field
    /// </summary>
    public string? LabelFr { get; init; }

    /// <summary>
    /// English description/help text
    /// </summary>
    public string? DescriptionEn { get; init; }

    /// <summary>
    /// French description/help text
    /// </summary>
    public string? DescriptionFr { get; init; }

    /// <summary>
    /// English tooltip/help text
    /// </summary>
    public string? HelpEn { get; init; }

    /// <summary>
    /// French tooltip/help text
    /// </summary>
    public string? HelpFr { get; init; }

    /// <summary>
    /// English placeholder text for input fields
    /// </summary>
    public string? PlaceholderEn { get; init; }

    /// <summary>
    /// French placeholder text for input fields
    /// </summary>
    public string? PlaceholderFr { get; init; }

    #endregion

    #region Validation

    /// <summary>
    /// Whether this field requires a value
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Minimum length for text input (null = no minimum)
    /// </summary>
    public int? MinLength { get; init; }

    /// <summary>
    /// Maximum length for text input (null = no maximum)
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Regular expression pattern for validation (null = no pattern validation)
    /// </summary>
    public string? Pattern { get; init; }

    /// <summary>
    /// Array of validation rule IDs to apply to this field
    /// </summary>
    public string[]? ValidationRules { get; init; }

    #endregion

    #region Conditional Logic

    /// <summary>
    /// Conditional rules that control this field's visibility or behavior
    /// </summary>
    public ConditionalRule[]? ConditionalRules { get; init; }

    #endregion

    #region Data Source

    /// <summary>
    /// ID of the code set for dropdown/lookup fields (null = use Options array)
    /// </summary>
    public int? CodeSetId { get; init; }

    /// <summary>
    /// Static options for dropdown/radio fields (used if CodeSetId is null)
    /// </summary>
    public FieldOption[]? Options { get; init; }

    #endregion

    #region Layout & Styling

    /// <summary>
    /// CSS grid width class (e.g., 12 for full width, 6 for half width)
    /// </summary>
    public int? WidthClass { get; init; }

    /// <summary>
    /// Custom CSS classes to apply to the field container
    /// </summary>
    public string? CssClasses { get; init; }

    /// <summary>
    /// Whether the field is initially visible (default: true)
    /// </summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>
    /// Whether the field is read-only (default: false)
    /// </summary>
    public bool IsReadOnly { get; init; }

    #endregion

    #region Database Mapping

    /// <summary>
    /// Database column name for this field (null = use Id)
    /// </summary>
    public string? ColumnName { get; init; }

    /// <summary>
    /// Database column type (nvarchar, int, datetime, etc.)
    /// </summary>
    public string? ColumnType { get; init; }

    #endregion

    #region Type-Specific Configuration

    /// <summary>
    /// Type-specific configuration (FileUploadConfig, DateRangeConfig, ModalTableConfig, etc.)
    /// Uses polymorphic JSON serialization
    /// </summary>
    public FieldTypeConfig? TypeConfig { get; init; }

    #endregion

    #region Extensibility

    /// <summary>
    /// Extended properties for custom data not covered by the schema.
    /// Stored as raw JSON for maximum flexibility.
    /// </summary>
    public JsonElement? ExtendedProperties { get; init; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a text field with common settings
    /// </summary>
    /// <param name="id">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required</param>
    /// <param name="order">Display order</param>
    /// <returns>Configured FormFieldSchema for a text field</returns>
    public static FormFieldSchema CreateTextField(
        string id,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = id,
            FieldType = "TextBox",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a section (container) field
    /// </summary>
    /// <param name="id">Unique section identifier</param>
    /// <param name="titleEn">English section title</param>
    /// <param name="titleFr">French section title (optional)</param>
    /// <param name="order">Display order</param>
    /// <returns>Configured FormFieldSchema for a section</returns>
    public static FormFieldSchema CreateSection(
        string id,
        string titleEn,
        string? titleFr = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = id,
            FieldType = "Section",
            LabelEn = titleEn,
            LabelFr = titleFr,
            Order = order
        };
    }

    /// <summary>
    /// Creates a dropdown field with options
    /// </summary>
    /// <param name="id">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="options">Array of dropdown options</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required</param>
    /// <param name="order">Display order</param>
    /// <returns>Configured FormFieldSchema for a dropdown</returns>
    public static FormFieldSchema CreateDropDown(
        string id,
        string labelEn,
        FieldOption[] options,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = id,
            FieldType = "DropDown",
            LabelEn = labelEn,
            LabelFr = labelFr,
            Options = options,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a dropdown field that references a CodeSet
    /// </summary>
    /// <param name="id">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="codeSetId">ID of the CodeSet to use for options</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required</param>
    /// <param name="order">Display order</param>
    /// <returns>Configured FormFieldSchema for a CodeSet-based dropdown</returns>
    public static FormFieldSchema CreateDropDownFromCodeSet(
        string id,
        string labelEn,
        int codeSetId,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = id,
            FieldType = "DropDown",
            LabelEn = labelEn,
            LabelFr = labelFr,
            CodeSetId = codeSetId,
            IsRequired = isRequired,
            Order = order
        };
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Checks if this field requires CodeSet resolution (has CodeSetId but no Options)
    /// </summary>
    /// <returns>True if CodeSet needs to be resolved, false otherwise</returns>
    public bool RequiresCodeSetResolution()
    {
        return CodeSetId.HasValue && (Options == null || Options.Length == 0);
    }

    /// <summary>
    /// Checks if this field is a selection type that can use options
    /// </summary>
    /// <returns>True if field supports options/CodeSets</returns>
    public bool SupportsOptions()
    {
        return FieldType switch
        {
            "DropDown" or "RadioButtonList" or "CheckBoxList" => true,
            _ => false
        };
    }

    #endregion
}
