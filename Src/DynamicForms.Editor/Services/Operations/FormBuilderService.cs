using DynamicForms.Core.V2.Schemas;
using DynamicForms.Editor.Services.State;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Editor.Services.Operations;

/// <summary>
/// Service for form building operations (CRUD on fields).
/// Provides methods to add, update, delete, clone, and reorder fields in a form module.
/// Integrates with EditorStateService for state management and UndoRedoService for undo/redo.
/// </summary>
public class FormBuilderService
{
    private readonly EditorStateService _editorState;
    private readonly UndoRedoService _undoRedo;
    private readonly ILogger<FormBuilderService> _logger;

    // ========================================================================
    // CONSTRUCTOR
    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the FormBuilderService class.
    /// </summary>
    /// <param name="editorState">Editor state service</param>
    /// <param name="undoRedo">Undo/redo service</param>
    /// <param name="logger">Logger instance</param>
    public FormBuilderService(
        EditorStateService editorState,
        UndoRedoService undoRedo,
        ILogger<FormBuilderService> logger)
    {
        _editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));
        _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // FIELD CRUD OPERATIONS
    // ========================================================================

    /// <summary>
    /// Adds a new field to the module.
    /// </summary>
    /// <param name="newField">Field to add</param>
    /// <param name="parentId">Optional parent field ID (null for root-level)</param>
    /// <exception cref="InvalidOperationException">If no module is loaded, field ID is duplicate, or parent doesn't exist</exception>
    public async Task AddFieldAsync(FormFieldSchema newField, string? parentId)
    {
        ArgumentNullException.ThrowIfNull(newField);

        _logger.LogInformation("Adding field: {FieldId}, Parent: {ParentId}", newField.Id, parentId);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check for duplicate field ID
        if (currentModule.Fields.Any(f => f.Id == newField.Id))
        {
            throw new InvalidOperationException($"Field with ID '{newField.Id}' already exists");
        }

        // Validate: Check parent exists if specified
        if (parentId != null && !currentModule.Fields.Any(f => f.Id == parentId))
        {
            throw new InvalidOperationException($"Parent field '{parentId}' does not exist");
        }

        // Set parent on new field
        var fieldToAdd = parentId != null
            ? newField with { ParentId = parentId }
            : newField;

        // Determine order: Add at end of siblings
        var siblings = GetSiblings(currentModule.Fields, parentId);
        var maxOrder = siblings.Any() ? siblings.Max(f => f.Order) : 0;
        fieldToAdd = fieldToAdd with { Order = maxOrder + 1 };

        // Create new module with field added
        var newFields = currentModule.Fields.Append(fieldToAdd).ToArray();
        var updatedModule = currentModule with { Fields = newFields };

        // Update state (this triggers undo/redo snapshot via EditorStateService events)
        var actionDescription = $"Added {newField.FieldType}: {newField.LabelEn ?? newField.Id}";
        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation("Field added successfully: {FieldId}", newField.Id);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Updates an existing field in the module.
    /// </summary>
    /// <param name="updatedField">Updated field with same ID as existing field</param>
    /// <exception cref="InvalidOperationException">If no module is loaded or field doesn't exist</exception>
    public async Task UpdateFieldAsync(FormFieldSchema updatedField)
    {
        ArgumentNullException.ThrowIfNull(updatedField);

        _logger.LogInformation("Updating field: {FieldId}", updatedField.Id);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check field exists
        var existingField = currentModule.Fields.FirstOrDefault(f => f.Id == updatedField.Id)
            ?? throw new InvalidOperationException($"Field '{updatedField.Id}' does not exist");

        // Create new module with field updated
        var newFields = currentModule.Fields
            .Select(f => f.Id == updatedField.Id ? updatedField : f)
            .ToArray();
        var updatedModule = currentModule with { Fields = newFields };

        // Update state
        var actionDescription = $"Updated {updatedField.FieldType}: {updatedField.LabelEn ?? updatedField.Id}";
        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation("Field updated successfully: {FieldId}", updatedField.Id);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes a field from the module.
    /// Also deletes all child fields recursively.
    /// </summary>
    /// <param name="fieldId">ID of field to delete</param>
    /// <exception cref="InvalidOperationException">If no module is loaded or field doesn't exist</exception>
    public async Task DeleteFieldAsync(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        _logger.LogInformation("Deleting field: {FieldId}", fieldId);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check field exists
        var fieldToDelete = currentModule.Fields.FirstOrDefault(f => f.Id == fieldId)
            ?? throw new InvalidOperationException($"Field '{fieldId}' does not exist");

        // Get all descendants (recursively)
        var idsToDelete = GetDescendantIds(currentModule.Fields, fieldId).ToHashSet();
        idsToDelete.Add(fieldId);

        // Create new module with field and descendants removed
        var newFields = currentModule.Fields
            .Where(f => !idsToDelete.Contains(f.Id))
            .ToArray();
        var updatedModule = currentModule with { Fields = newFields };

        // Update state
        var actionDescription = $"Deleted {fieldToDelete.FieldType}: {fieldToDelete.LabelEn ?? fieldToDelete.Id}";
        if (idsToDelete.Count > 1)
        {
            actionDescription += $" and {idsToDelete.Count - 1} child field(s)";
        }

        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation(
            "Field deleted successfully: {FieldId}, Total deleted: {Count}",
            fieldId,
            idsToDelete.Count);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Clones a field with a new unique ID.
    /// Child fields are not cloned (only the specified field).
    /// </summary>
    /// <param name="fieldId">ID of field to clone</param>
    /// <exception cref="InvalidOperationException">If no module is loaded or field doesn't exist</exception>
    public async Task CloneFieldAsync(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        _logger.LogInformation("Cloning field: {FieldId}", fieldId);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check field exists
        var fieldToClone = currentModule.Fields.FirstOrDefault(f => f.Id == fieldId)
            ?? throw new InvalidOperationException($"Field '{fieldId}' does not exist");

        // Generate unique ID for clone
        var cloneId = GenerateUniqueFieldId(currentModule.Fields, fieldId);

        // Clone field with new ID
        var clonedField = fieldToClone with
        {
            Id = cloneId,
            Order = fieldToClone.Order + 1  // Place right after original
        };

        // Create new module with cloned field added
        var newFields = currentModule.Fields.Append(clonedField).ToArray();
        var updatedModule = currentModule with { Fields = newFields };

        // Update state
        var actionDescription = $"Cloned {fieldToClone.FieldType}: {fieldToClone.LabelEn ?? fieldToClone.Id} → {cloneId}";
        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation("Field cloned successfully: {FieldId} → {CloneId}", fieldId, cloneId);

        await Task.CompletedTask;
    }

    // ========================================================================
    // FIELD ORDERING OPERATIONS
    // ========================================================================

    /// <summary>
    /// Moves a field up in the display order (swaps with previous sibling).
    /// </summary>
    /// <param name="fieldId">ID of field to move up</param>
    /// <exception cref="InvalidOperationException">If no module is loaded, field doesn't exist, or already at top</exception>
    public async Task MoveFieldUpAsync(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        _logger.LogInformation("Moving field up: {FieldId}", fieldId);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check field exists
        var field = currentModule.Fields.FirstOrDefault(f => f.Id == fieldId)
            ?? throw new InvalidOperationException($"Field '{fieldId}' does not exist");

        // Get siblings ordered by Order property
        var siblings = GetSiblings(currentModule.Fields, field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();

        // Find current position
        var currentIndex = siblings.FindIndex(f => f.Id == fieldId);
        if (currentIndex <= 0)
        {
            throw new InvalidOperationException($"Field '{fieldId}' is already at the top");
        }

        // Swap order with previous sibling
        var previousField = siblings[currentIndex - 1];
        var updatedFields = currentModule.Fields.Select(f =>
        {
            if (f.Id == fieldId)
                return f with { Order = previousField.Order };
            if (f.Id == previousField.Id)
                return f with { Order = field.Order };
            return f;
        }).ToArray();

        var updatedModule = currentModule with { Fields = updatedFields };

        // Update state
        var actionDescription = $"Moved {field.FieldType} up: {field.LabelEn ?? field.Id}";
        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation("Field moved up successfully: {FieldId}", fieldId);

        await Task.CompletedTask;
    }

    /// <summary>
    /// Moves a field down in the display order (swaps with next sibling).
    /// </summary>
    /// <param name="fieldId">ID of field to move down</param>
    /// <exception cref="InvalidOperationException">If no module is loaded, field doesn't exist, or already at bottom</exception>
    public async Task MoveFieldDownAsync(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        _logger.LogInformation("Moving field down: {FieldId}", fieldId);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check field exists
        var field = currentModule.Fields.FirstOrDefault(f => f.Id == fieldId)
            ?? throw new InvalidOperationException($"Field '{fieldId}' does not exist");

        // Get siblings ordered by Order property
        var siblings = GetSiblings(currentModule.Fields, field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();

        // Find current position
        var currentIndex = siblings.FindIndex(f => f.Id == fieldId);
        if (currentIndex >= siblings.Count - 1)
        {
            throw new InvalidOperationException($"Field '{fieldId}' is already at the bottom");
        }

        // Swap order with next sibling
        var nextField = siblings[currentIndex + 1];
        var updatedFields = currentModule.Fields.Select(f =>
        {
            if (f.Id == fieldId)
                return f with { Order = nextField.Order };
            if (f.Id == nextField.Id)
                return f with { Order = field.Order };
            return f;
        }).ToArray();

        var updatedModule = currentModule with { Fields = updatedFields };

        // Update state
        var actionDescription = $"Moved {field.FieldType} down: {field.LabelEn ?? field.Id}";
        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation("Field moved down successfully: {FieldId}", fieldId);

        await Task.CompletedTask;
    }

    // ========================================================================
    // FIELD HIERARCHY OPERATIONS
    // ========================================================================

    /// <summary>
    /// Changes the parent of a field.
    /// </summary>
    /// <param name="fieldId">ID of field to reparent</param>
    /// <param name="newParentId">New parent ID (null for root-level)</param>
    /// <exception cref="InvalidOperationException">If validation fails</exception>
    public async Task SetFieldParentAsync(string fieldId, string? newParentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        _logger.LogInformation(
            "Setting field parent: {FieldId}, New Parent: {NewParentId}",
            fieldId,
            newParentId);

        // Get current module
        var currentModule = _editorState.GetCurrentModule()
            ?? throw new InvalidOperationException("No module is currently loaded");

        // Validate: Check field exists
        var field = currentModule.Fields.FirstOrDefault(f => f.Id == fieldId)
            ?? throw new InvalidOperationException($"Field '{fieldId}' does not exist");

        // Validate: Check new parent exists if specified
        if (newParentId != null && !currentModule.Fields.Any(f => f.Id == newParentId))
        {
            throw new InvalidOperationException($"Parent field '{newParentId}' does not exist");
        }

        // Validate: Prevent setting self as parent
        if (fieldId == newParentId)
        {
            throw new InvalidOperationException("Field cannot be its own parent");
        }

        // Validate: Prevent circular relationships
        if (newParentId != null && WouldCreateCircularRelationship(currentModule.Fields, fieldId, newParentId))
        {
            throw new InvalidOperationException(
                $"Setting '{newParentId}' as parent of '{fieldId}' would create a circular relationship");
        }

        // Update field's parent
        var updatedFields = currentModule.Fields.Select(f =>
            f.Id == fieldId ? f with { ParentId = newParentId } : f
        ).ToArray();

        var updatedModule = currentModule with { Fields = updatedFields };

        // Update state
        var parentLabel = newParentId != null
            ? currentModule.Fields.FirstOrDefault(f => f.Id == newParentId)?.LabelEn ?? newParentId
            : "root";
        var actionDescription = $"Moved {field.LabelEn ?? field.Id} to {parentLabel}";
        _editorState.UpdateModule(updatedModule, actionDescription);

        // Create snapshot for undo/redo
        var snapshot = CreateSnapshot(updatedModule, actionDescription);
        _undoRedo.PushSnapshot(snapshot, actionDescription);

        _logger.LogInformation(
            "Field parent changed successfully: {FieldId} → {NewParentId}",
            fieldId,
            newParentId);

        await Task.CompletedTask;
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Gets all sibling fields (fields with the same parent).
    /// </summary>
    private IEnumerable<FormFieldSchema> GetSiblings(FormFieldSchema[] fields, string? parentId)
    {
        return fields.Where(f => f.ParentId == parentId);
    }

    /// <summary>
    /// Gets all descendant field IDs recursively.
    /// </summary>
    private IEnumerable<string> GetDescendantIds(FormFieldSchema[] fields, string parentId)
    {
        var children = fields.Where(f => f.ParentId == parentId).ToList();

        foreach (var child in children)
        {
            yield return child.Id;

            // Recursively get descendants
            foreach (var descendantId in GetDescendantIds(fields, child.Id))
            {
                yield return descendantId;
            }
        }
    }

    /// <summary>
    /// Generates a unique field ID based on the original ID.
    /// Appends "_copy", "_copy2", "_copy3", etc. until unique.
    /// </summary>
    private string GenerateUniqueFieldId(FormFieldSchema[] fields, string baseId)
    {
        var existingIds = fields.Select(f => f.Id).ToHashSet();

        var candidateId = $"{baseId}_copy";
        var suffix = 2;

        while (existingIds.Contains(candidateId))
        {
            candidateId = $"{baseId}_copy{suffix}";
            suffix++;
        }

        return candidateId;
    }

    /// <summary>
    /// Checks if setting newParentId as parent of fieldId would create a circular relationship.
    /// Returns true if fieldId is an ancestor of newParentId.
    /// </summary>
    private bool WouldCreateCircularRelationship(
        FormFieldSchema[] fields,
        string fieldId,
        string newParentId)
    {
        // Get all ancestors of newParentId
        var ancestorIds = GetAncestorIds(fields, newParentId).ToHashSet();

        // If fieldId is an ancestor of newParentId, this would create a circle
        return ancestorIds.Contains(fieldId);
    }

    /// <summary>
    /// Gets all ancestor field IDs recursively.
    /// </summary>
    private IEnumerable<string> GetAncestorIds(FormFieldSchema[] fields, string fieldId)
    {
        var field = fields.FirstOrDefault(f => f.Id == fieldId);
        if (field?.ParentId == null)
        {
            yield break;
        }

        yield return field.ParentId;

        // Recursively get ancestors
        foreach (var ancestorId in GetAncestorIds(fields, field.ParentId))
        {
            yield return ancestorId;
        }
    }

    /// <summary>
    /// Creates an EditorSnapshot from a module for undo/redo.
    /// </summary>
    private EditorSnapshot CreateSnapshot(FormModuleSchema module, string actionDescription)
    {
        return new EditorSnapshot(
            SessionId: _editorState.EditorSessionId,
            EntityType: EditorEntityType.Module,
            SnapshotJson: System.Text.Json.JsonSerializer.Serialize(module),
            ActionDescription: actionDescription,
            Timestamp: DateTime.UtcNow,
            SequenceNumber: 0  // Will be assigned by UndoRedoService
        );
    }
}
