using System.Text.Json;
using DynamicForms.Core.V2.Schemas;
using DynamicForms.Editor.Data;
using DynamicForms.Editor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Editor.Services;

/// <summary>
/// Service for publishing form modules and workflows to production.
/// Handles versioning, validation, and atomic publishing operations.
/// </summary>
public class PublishService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<PublishService> _logger;

    // ========================================================================
    // CONSTRUCTOR
    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the PublishService class.
    /// </summary>
    /// <param name="dbContext">Application database context</param>
    /// <param name="logger">Logger instance</param>
    public PublishService(
        ApplicationDbContext dbContext,
        ILogger<PublishService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // PUBLIC METHODS - MODULE PUBLISHING
    // ========================================================================

    /// <summary>
    /// Publishes a draft module to production.
    /// Creates a new version, deactivates previous versions, and updates draft status.
    /// </summary>
    /// <param name="moduleId">Module ID to publish</param>
    /// <param name="publishedBy">Username of the publisher (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PublishResult indicating success or failure with details</returns>
    public async Task<PublishResult> PublishModuleAsync(
        int moduleId,
        string? publishedBy = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting publish process for module ID: {ModuleId}", moduleId);

        try
        {
            // Begin transaction for atomicity
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Load draft module from EditorFormModules
                var draftModule = await _dbContext.EditorFormModules
                    .Where(m => m.ModuleId == moduleId)
                    .OrderByDescending(m => m.ModifiedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (draftModule == null)
                {
                    _logger.LogWarning("Module ID {ModuleId} not found in draft modules", moduleId);
                    return PublishResult.CreateFailure($"Module ID {moduleId} not found");
                }

                // 2. Deserialize and validate schema
                FormModuleSchema? schema;
                try
                {
                    schema = JsonSerializer.Deserialize<FormModuleSchema>(draftModule.SchemaJson);
                    if (schema == null)
                    {
                        return PublishResult.CreateFailure("Failed to deserialize module schema");
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid JSON in module schema for module ID: {ModuleId}", moduleId);
                    return PublishResult.CreateFailure($"Invalid JSON in module schema: {ex.Message}");
                }

                var validationResult = await ValidateSchemaForPublishAsync(schema, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Schema validation failed for module ID {ModuleId}: {Errors}",
                        moduleId,
                        string.Join(", ", validationResult.Errors));
                    return PublishResult.CreateFailure(validationResult.Errors);
                }

                // Log warnings if any
                if (validationResult.Warnings.Any())
                {
                    _logger.LogWarning(
                        "Schema validation warnings for module ID {ModuleId}: {Warnings}",
                        moduleId,
                        string.Join(", ", validationResult.Warnings));
                }

                // 3. Get next version number
                var latestVersion = await _dbContext.PublishedFormModules
                    .Where(p => p.ModuleId == moduleId)
                    .MaxAsync(p => (int?)p.Version, cancellationToken) ?? 0;

                var newVersion = latestVersion + 1;

                // 4. Deactivate previous versions
                var previousVersions = await _dbContext.PublishedFormModules
                    .Where(p => p.ModuleId == moduleId && p.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var previousVersion in previousVersions)
                {
                    previousVersion.IsActive = false;
                    _logger.LogInformation(
                        "Deactivated module ID {ModuleId} version {Version}",
                        moduleId,
                        previousVersion.Version);
                }

                // 5. Create new PublishedFormModule record
                var publishedModule = new PublishedFormModule
                {
                    ModuleId = moduleId,
                    Title = draftModule.Title,
                    TitleFr = draftModule.TitleFr,
                    SchemaJson = draftModule.SchemaJson,
                    Version = newVersion,
                    PublishedAt = DateTime.UtcNow,
                    PublishedBy = publishedBy ?? "System",
                    IsActive = true
                };

                _dbContext.PublishedFormModules.Add(publishedModule);

                // 6. Update draft status to "Published"
                draftModule.Status = "Published";
                draftModule.ModifiedAt = DateTime.UtcNow;
                draftModule.ModifiedBy = publishedBy ?? "System";

                // 7. Save changes
                await _dbContext.SaveChangesAsync(cancellationToken);

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully published module ID {ModuleId} as version {Version}",
                    moduleId,
                    newVersion);

                return PublishResult.CreateSuccess(newVersion, publishedModule.Id);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error during publish transaction for module ID: {ModuleId}", moduleId);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish module ID: {ModuleId}", moduleId);
            return PublishResult.CreateFailure($"Publish failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Publishes a draft workflow to production.
    /// Creates a new version, deactivates previous versions, and updates draft status.
    /// </summary>
    /// <param name="workflowId">Workflow ID to publish</param>
    /// <param name="publishedBy">Username of the publisher (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PublishResult indicating success or failure with details</returns>
    public async Task<PublishResult> PublishWorkflowAsync(
        int workflowId,
        string? publishedBy = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting publish process for workflow ID: {WorkflowId}", workflowId);

        // TODO: Implement when EditorWorkflows and PublishedWorkflows entities are created
        await Task.CompletedTask;
        return PublishResult.CreateFailure("Workflow publishing not yet implemented");
    }

    // ========================================================================
    // PUBLIC METHODS - SCHEMA VALIDATION
    // ========================================================================

    /// <summary>
    /// Validates a form module schema for publishing.
    /// Checks for structural validity, uniqueness, and referential integrity.
    /// </summary>
    /// <param name="schema">Form module schema to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SchemaValidationResult indicating validity and any errors/warnings</returns>
    public Task<SchemaValidationResult> ValidateSchemaForPublishAsync(
        FormModuleSchema schema,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Validation 1: Schema must exist
        if (schema == null)
        {
            errors.Add("Schema cannot be null");
            return Task.FromResult(SchemaValidationResult.CreateInvalid(errors, warnings));
        }

        // Validation 2: At least one field must exist
        if (schema.Fields == null || schema.Fields.Length == 0)
        {
            errors.Add("Module must contain at least one field");
        }

        // Validation 3: All fields must have unique IDs
        var fieldIds = schema.Fields?.Select(f => f.Id).ToList() ?? new List<string>();
        var duplicateIds = fieldIds
            .GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateIds.Any())
        {
            errors.Add($"Duplicate field IDs found: {string.Join(", ", duplicateIds)}");
        }

        // Validation 4: No circular parent references
        if (schema.Fields != null)
        {
            foreach (var field in schema.Fields)
            {
                if (HasCircularParentReference(schema.Fields, field.Id))
                {
                    errors.Add($"Field '{field.Id}' has a circular parent reference");
                }
            }
        }

        // Validation 5: All parent IDs must reference existing fields
        if (schema.Fields != null)
        {
            var fieldIdSet = fieldIds.ToHashSet();
            foreach (var field in schema.Fields)
            {
                if (field.ParentId != null && !fieldIdSet.Contains(field.ParentId))
                {
                    errors.Add($"Field '{field.Id}' references non-existent parent '{field.ParentId}'");
                }
            }
        }

        // Validation 6: All conditional target fields must exist
        if (schema.Fields != null)
        {
            var fieldIdSet = fieldIds.ToHashSet();
            foreach (var field in schema.Fields)
            {
                if (field.ConditionalRules != null)
                {
                    foreach (var rule in field.ConditionalRules)
                    {
                        if (!fieldIdSet.Contains(rule.FieldId))
                        {
                            errors.Add(
                                $"Field '{field.Id}' has conditional rule referencing non-existent field '{rule.FieldId}'");
                        }
                    }
                }
            }
        }

        // Validation 7: Required fields must have labels
        if (schema.Fields != null)
        {
            foreach (var field in schema.Fields.Where(f => f.IsRequired))
            {
                if (string.IsNullOrWhiteSpace(field.LabelEn))
                {
                    warnings.Add($"Required field '{field.Id}' is missing an English label");
                }
            }
        }

        // Validation 8: Dropdowns must have options or code set
        if (schema.Fields != null)
        {
            foreach (var field in schema.Fields.Where(f =>
                f.FieldType == "DropDown" || f.FieldType == "RadioButtons" || f.FieldType == "CheckboxList"))
            {
                if ((field.Options == null || field.Options.Length == 0) && field.CodeSetId == null)
                {
                    errors.Add(
                        $"Field '{field.Id}' is a {field.FieldType} but has no options or code set");
                }
            }
        }

        // Validation 9: File upload fields must have allowed extensions
        if (schema.Fields != null)
        {
            foreach (var field in schema.Fields.Where(f => f.FieldType == "FileUpload"))
            {
                if (field.TypeConfig is FileUploadConfig config)
                {
                    if (config.AllowedExtensions == null || config.AllowedExtensions.Length == 0)
                    {
                        errors.Add($"File upload field '{field.Id}' has no allowed extensions");
                    }
                }
                else
                {
                    errors.Add($"File upload field '{field.Id}' is missing FileUploadConfig");
                }
            }
        }

        // Validation 10: Modal table fields must have modal fields defined
        if (schema.Fields != null)
        {
            foreach (var field in schema.Fields.Where(f => f.FieldType == "ModalTable"))
            {
                if (field.TypeConfig is ModalTableConfig config)
                {
                    if (config.ModalFields == null || config.ModalFields.Length == 0)
                    {
                        errors.Add($"Modal table field '{field.Id}' has no modal fields defined");
                    }
                }
                else
                {
                    errors.Add($"Modal table field '{field.Id}' is missing ModalTableConfig");
                }
            }
        }

        var result = errors.Any()
            ? SchemaValidationResult.CreateInvalid(errors, warnings)
            : SchemaValidationResult.CreateValid(warnings);

        return Task.FromResult(result);
    }

    // ========================================================================
    // PRIVATE HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Checks if a field has a circular parent reference.
    /// Returns true if the field is its own ancestor.
    /// </summary>
    private bool HasCircularParentReference(FormFieldSchema[] fields, string fieldId)
    {
        var visited = new HashSet<string>();
        var currentId = fieldId;

        while (currentId != null)
        {
            if (visited.Contains(currentId))
            {
                return true; // Circular reference detected
            }

            visited.Add(currentId);

            var field = fields.FirstOrDefault(f => f.Id == currentId);
            if (field == null)
            {
                break; // Field not found, stop traversal
            }

            currentId = field.ParentId;
        }

        return false;
    }
}
