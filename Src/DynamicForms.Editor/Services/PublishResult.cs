namespace DynamicForms.Editor.Services;

/// <summary>
/// Result of a publish operation.
/// Indicates success or failure with detailed error information.
/// </summary>
/// <param name="Success">Whether the publish operation succeeded</param>
/// <param name="Version">Version number of published module (null if failed)</param>
/// <param name="Errors">List of errors that occurred (empty if successful)</param>
/// <param name="PublishedModuleId">Database ID of the published module record (null if failed)</param>
public record PublishResult(
    bool Success,
    int? Version,
    List<string> Errors,
    int? PublishedModuleId = null
)
{
    /// <summary>
    /// Creates a successful publish result.
    /// </summary>
    /// <param name="version">Version number of published module</param>
    /// <param name="publishedModuleId">Database ID of the published module record</param>
    /// <returns>PublishResult indicating success</returns>
    public static PublishResult CreateSuccess(int version, int publishedModuleId)
    {
        return new PublishResult(
            Success: true,
            Version: version,
            Errors: new List<string>(),
            PublishedModuleId: publishedModuleId);
    }

    /// <summary>
    /// Creates a failed publish result.
    /// </summary>
    /// <param name="errors">List of errors that occurred</param>
    /// <returns>PublishResult indicating failure</returns>
    public static PublishResult CreateFailure(List<string> errors)
    {
        return new PublishResult(
            Success: false,
            Version: null,
            Errors: errors,
            PublishedModuleId: null);
    }

    /// <summary>
    /// Creates a failed publish result with a single error.
    /// </summary>
    /// <param name="error">Error message</param>
    /// <returns>PublishResult indicating failure</returns>
    public static PublishResult CreateFailure(string error)
    {
        return new PublishResult(
            Success: false,
            Version: null,
            Errors: new List<string> { error },
            PublishedModuleId: null);
    }
}

/// <summary>
/// Result of schema validation for publishing.
/// Indicates whether a schema is valid and ready to publish.
/// </summary>
/// <param name="IsValid">Whether the schema is valid</param>
/// <param name="Errors">List of validation errors (empty if valid)</param>
/// <param name="Warnings">List of non-blocking warnings</param>
public record SchemaValidationResult(
    bool IsValid,
    List<string> Errors,
    List<string> Warnings
)
{
    /// <summary>
    /// Creates a valid schema validation result.
    /// </summary>
    /// <param name="warnings">Optional warnings (non-blocking)</param>
    /// <returns>SchemaValidationResult indicating schema is valid</returns>
    public static SchemaValidationResult CreateValid(List<string>? warnings = null)
    {
        return new SchemaValidationResult(
            IsValid: true,
            Errors: new List<string>(),
            Warnings: warnings ?? new List<string>());
    }

    /// <summary>
    /// Creates an invalid schema validation result.
    /// </summary>
    /// <param name="errors">List of validation errors</param>
    /// <param name="warnings">Optional warnings</param>
    /// <returns>SchemaValidationResult indicating schema is invalid</returns>
    public static SchemaValidationResult CreateInvalid(
        List<string> errors,
        List<string>? warnings = null)
    {
        return new SchemaValidationResult(
            IsValid: false,
            Errors: errors,
            Warnings: warnings ?? new List<string>());
    }

    /// <summary>
    /// Creates an invalid schema validation result with a single error.
    /// </summary>
    /// <param name="error">Error message</param>
    /// <returns>SchemaValidationResult indicating schema is invalid</returns>
    public static SchemaValidationResult CreateInvalid(string error)
    {
        return new SchemaValidationResult(
            IsValid: false,
            Errors: new List<string> { error },
            Warnings: new List<string>());
    }
}
