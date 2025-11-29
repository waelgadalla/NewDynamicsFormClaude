using System.Text.Json.Serialization;

namespace DynamicForms.Core.V2.Schemas;

/// <summary>
/// Abstract base class for type-specific field configurations.
/// Uses polymorphic JSON serialization for strongly-typed field type settings.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(TextAreaConfig), "textArea")]
[JsonDerivedType(typeof(FileUploadConfig), "fileUpload")]
[JsonDerivedType(typeof(DateRangeConfig), "dateRange")]
[JsonDerivedType(typeof(ModalTableConfig), "modalTable")]
public abstract record FieldTypeConfig;

/// <summary>
/// Configuration for text area fields
/// </summary>
/// <param name="Rows">Number of visible text rows (default: 4)</param>
public record TextAreaConfig(
    int Rows = 4
) : FieldTypeConfig;

/// <summary>
/// Configuration for file upload fields
/// </summary>
/// <param name="AllowedExtensions">Array of allowed file extensions (e.g., [".pdf", ".docx"])</param>
/// <param name="MaxFileSizeBytes">Maximum file size in bytes (default: 10 MB)</param>
/// <param name="MaxFiles">Maximum number of files that can be uploaded (default: 1)</param>
/// <param name="RequireVirusScan">Whether uploaded files must be virus scanned (default: true)</param>
public record FileUploadConfig(
    string[] AllowedExtensions,
    long MaxFileSizeBytes = 10_485_760,
    int MaxFiles = 1,
    bool RequireVirusScan = true
) : FieldTypeConfig;

/// <summary>
/// Configuration for date range fields with validation constraints
/// </summary>
/// <param name="MinDate">Minimum allowed date (null = no minimum)</param>
/// <param name="MaxDate">Maximum allowed date (null = no maximum)</param>
/// <param name="AllowFutureDates">Whether dates in the future are allowed (default: true)</param>
/// <param name="DateFormat">Display format for dates (default: "yyyy-MM-dd")</param>
public record DateRangeConfig(
    DateTime? MinDate = null,
    DateTime? MaxDate = null,
    bool AllowFutureDates = true,
    string DateFormat = "yyyy-MM-dd"
) : FieldTypeConfig;

/// <summary>
/// Configuration for modal table fields (multi-row data entry)
/// </summary>
/// <param name="ModalFields">Array of field schemas that appear in the modal dialog</param>
/// <param name="MaxRecords">Maximum number of records that can be added (null = unlimited)</param>
/// <param name="AllowDuplicates">Whether duplicate records are allowed (default: false)</param>
public record ModalTableConfig(
    FormFieldSchema[] ModalFields,
    int? MaxRecords = null,
    bool AllowDuplicates = false
) : FieldTypeConfig;
