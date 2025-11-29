namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Event arguments for successful auto-save completion.
/// Provides information about what was saved and when.
/// </summary>
public class AutoSaveEventArgs : EventArgs
{
    /// <summary>
    /// Gets the editor session ID when the save occurred.
    /// </summary>
    public Guid SessionId { get; }

    /// <summary>
    /// Gets the type of entity that was saved (Module or Workflow).
    /// </summary>
    public EditorEntityType EntityType { get; }

    /// <summary>
    /// Gets the database ID of the saved entity.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    /// Gets the timestamp when the save completed (UTC).
    /// </summary>
    public DateTime SavedAt { get; }

    /// <summary>
    /// Gets the size of the saved data in bytes.
    /// </summary>
    public long DataSize { get; }

    /// <summary>
    /// Gets whether this was a manual save (true) or automatic timer-based save (false).
    /// </summary>
    public bool IsManualSave { get; }

    /// <summary>
    /// Initializes a new instance of the AutoSaveEventArgs class.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="entityType">Type of entity saved</param>
    /// <param name="entityId">Database ID of saved entity</param>
    /// <param name="savedAt">Timestamp when save completed</param>
    /// <param name="dataSize">Size of saved data in bytes</param>
    /// <param name="isManualSave">Whether this was a manual save</param>
    public AutoSaveEventArgs(
        Guid sessionId,
        EditorEntityType entityType,
        int entityId,
        DateTime savedAt,
        long dataSize,
        bool isManualSave = false)
    {
        SessionId = sessionId;
        EntityType = entityType;
        EntityId = entityId;
        SavedAt = savedAt;
        DataSize = dataSize;
        IsManualSave = isManualSave;
    }
}

/// <summary>
/// Event arguments for auto-save errors.
/// Provides information about what failed and why.
/// </summary>
public class AutoSaveErrorEventArgs : EventArgs
{
    /// <summary>
    /// Gets the editor session ID when the error occurred.
    /// </summary>
    public Guid SessionId { get; }

    /// <summary>
    /// Gets the type of entity that failed to save (Module or Workflow).
    /// </summary>
    public EditorEntityType EntityType { get; }

    /// <summary>
    /// Gets the exception that was thrown during save.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the timestamp when the error occurred (UTC).
    /// </summary>
    public DateTime ErrorAt { get; }

    /// <summary>
    /// Gets whether this was a manual save attempt (true) or automatic (false).
    /// </summary>
    public bool IsManualSave { get; }

    /// <summary>
    /// Gets the number of consecutive save failures (for retry logic).
    /// </summary>
    public int FailureCount { get; }

    /// <summary>
    /// Initializes a new instance of the AutoSaveErrorEventArgs class.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="entityType">Type of entity that failed to save</param>
    /// <param name="exception">Exception that occurred</param>
    /// <param name="errorAt">Timestamp when error occurred</param>
    /// <param name="isManualSave">Whether this was a manual save attempt</param>
    /// <param name="failureCount">Number of consecutive failures</param>
    public AutoSaveErrorEventArgs(
        Guid sessionId,
        EditorEntityType entityType,
        Exception exception,
        DateTime errorAt,
        bool isManualSave = false,
        int failureCount = 1)
    {
        SessionId = sessionId;
        EntityType = entityType;
        Exception = exception;
        ErrorAt = errorAt;
        IsManualSave = isManualSave;
        FailureCount = failureCount;
    }
}
