using DynamicForms.Editor.Data.Entities;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository interface for managing undo/redo history snapshots.
/// Provides operations for saving and retrieving editor state snapshots for undo/redo functionality.
/// </summary>
public interface IEditorHistoryRepository
{
    // ========================================================================
    // SNAPSHOT OPERATIONS
    // ========================================================================

    /// <summary>
    /// Saves a snapshot of the current editor state for undo/redo.
    /// </summary>
    /// <param name="snapshot">The snapshot to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The saved snapshot with ID assigned</returns>
    Task<EditorHistorySnapshot> SaveSnapshotAsync(
        EditorHistorySnapshot snapshot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves multiple snapshots in a single transaction.
    /// Useful when initializing a new editing session with multiple initial states.
    /// </summary>
    /// <param name="snapshots">Snapshots to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Saved snapshots with IDs assigned</returns>
    Task<List<EditorHistorySnapshot>> SaveSnapshotBatchAsync(
        IEnumerable<EditorHistorySnapshot> snapshots,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific snapshot by its ID.
    /// </summary>
    /// <param name="id">Snapshot ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Snapshot if found; otherwise null</returns>
    Task<EditorHistorySnapshot?> GetSnapshotByIdAsync(long id, CancellationToken cancellationToken = default);

    // ========================================================================
    // SESSION-BASED QUERIES (for Undo/Redo)
    // ========================================================================

    /// <summary>
    /// Gets all snapshots for an editing session, ordered by sequence number descending.
    /// This is the primary method for undo/redo operations.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of snapshots ordered by sequence number descending (newest first)</returns>
    Task<List<EditorHistorySnapshot>> GetSessionSnapshotsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets snapshots for a session up to a specific sequence number.
    /// Used for undo operations to get all snapshots before a certain point.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="maxSequenceNumber">Maximum sequence number (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of snapshots with sequence number <= maxSequenceNumber</returns>
    Task<List<EditorHistorySnapshot>> GetSessionSnapshotsUpToAsync(
        Guid sessionId,
        int maxSequenceNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent snapshot for a session.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent snapshot if found; otherwise null</returns>
    Task<EditorHistorySnapshot?> GetLatestSnapshotAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific snapshot by session and sequence number.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="sequenceNumber">Sequence number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Snapshot if found; otherwise null</returns>
    Task<EditorHistorySnapshot?> GetSnapshotBySequenceAsync(
        Guid sessionId,
        int sequenceNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next sequence number for a session.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next available sequence number (1 if no snapshots exist)</returns>
    Task<int> GetNextSequenceNumberAsync(Guid sessionId, CancellationToken cancellationToken = default);

    // ========================================================================
    // ENTITY-BASED QUERIES
    // ========================================================================

    /// <summary>
    /// Gets all snapshots for a specific entity (module or workflow).
    /// Useful for viewing complete edit history of an entity across all sessions.
    /// </summary>
    /// <param name="entityType">Entity type ("Module" or "Workflow")</param>
    /// <param name="entityId">Entity ID (ModuleId or WorkflowId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of snapshots ordered by creation date descending</returns>
    Task<List<EditorHistorySnapshot>> GetEntitySnapshotsAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets snapshots for an entity within a date range.
    /// </summary>
    /// <param name="entityType">Entity type ("Module" or "Workflow")</param>
    /// <param name="entityId">Entity ID (ModuleId or WorkflowId)</param>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of snapshots within the date range</returns>
    Task<List<EditorHistorySnapshot>> GetEntitySnapshotsByDateRangeAsync(
        string entityType,
        int entityId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // CLEANUP OPERATIONS
    // ========================================================================

    /// <summary>
    /// Deletes all snapshots for a specific editing session.
    /// Typically called when a session is closed or abandoned.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of snapshots deleted</returns>
    Task<int> DeleteSessionSnapshotsAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes snapshots older than a specified date.
    /// Used for cleanup and retention policy enforcement.
    /// </summary>
    /// <param name="olderThan">Delete snapshots created before this date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of snapshots deleted</returns>
    Task<int> DeleteSnapshotsOlderThanAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all snapshots for a specific entity.
    /// Use with caution - this removes all history for the entity.
    /// </summary>
    /// <param name="entityType">Entity type ("Module" or "Workflow")</param>
    /// <param name="entityId">Entity ID (ModuleId or WorkflowId)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of snapshots deleted</returns>
    Task<int> DeleteEntitySnapshotsAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes snapshots beyond the configured retention limit for a session.
    /// Keeps only the most recent N snapshots, deletes older ones.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="keepCount">Number of recent snapshots to keep</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of snapshots deleted</returns>
    Task<int> TrimSessionHistoryAsync(
        Guid sessionId,
        int keepCount,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // COUNT & STATISTICS
    // ========================================================================

    /// <summary>
    /// Gets the count of snapshots for a specific session.
    /// </summary>
    /// <param name="sessionId">Editor session ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of snapshots in the session</returns>
    Task<int> GetSessionSnapshotCountAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of all history snapshots in the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total snapshot count</returns>
    Task<int> GetTotalSnapshotCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total size (in bytes) of all snapshots in the database.
    /// Useful for monitoring storage usage.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total size of SnapshotJson data in bytes</returns>
    Task<long> GetTotalStorageSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about snapshots (count, oldest, newest).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Snapshot statistics</returns>
    Task<SnapshotStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    // ========================================================================
    // BULK OPERATIONS
    // ========================================================================

    /// <summary>
    /// Deletes multiple sessions' snapshots in a single transaction.
    /// </summary>
    /// <param name="sessionIds">List of session IDs to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total number of snapshots deleted</returns>
    Task<int> DeleteMultipleSessionsAsync(
        IEnumerable<Guid> sessionIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the oldest snapshot date in the database.
    /// Useful for retention policy checks.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Date of oldest snapshot, or null if no snapshots exist</returns>
    Task<DateTime?> GetOldestSnapshotDateAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about history snapshots.
/// </summary>
public record SnapshotStatistics
{
    /// <summary>
    /// Total number of snapshots
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Number of unique editing sessions
    /// </summary>
    public int UniqueSessionCount { get; init; }

    /// <summary>
    /// Date of oldest snapshot
    /// </summary>
    public DateTime? OldestSnapshotDate { get; init; }

    /// <summary>
    /// Date of newest snapshot
    /// </summary>
    public DateTime? NewestSnapshotDate { get; init; }

    /// <summary>
    /// Total storage size in bytes
    /// </summary>
    public long TotalStorageBytes { get; init; }

    /// <summary>
    /// Average snapshots per session
    /// </summary>
    public double AverageSnapshotsPerSession { get; init; }
}
