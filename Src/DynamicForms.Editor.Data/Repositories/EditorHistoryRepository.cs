using DynamicForms.Editor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository implementation for managing undo/redo history snapshots.
/// Provides operations for saving and retrieving editor state snapshots for undo/redo functionality.
/// Includes automatic cleanup of old snapshots.
/// </summary>
public class EditorHistoryRepository : IEditorHistoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EditorHistoryRepository> _logger;

    // Default retention period: 90 days
    private static readonly TimeSpan DefaultRetentionPeriod = TimeSpan.FromDays(90);

    /// <summary>
    /// Initializes a new instance of the EditorHistoryRepository.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public EditorHistoryRepository(ApplicationDbContext context, ILogger<EditorHistoryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // SNAPSHOT OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<EditorHistorySnapshot> SaveSnapshotAsync(
        EditorHistorySnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        try
        {
            _logger.LogDebug("Saving snapshot: SessionId={SessionId}, EntityType={EntityType}, EntityId={EntityId}, Sequence={Sequence}",
                snapshot.EditorSessionId, snapshot.EntityType, snapshot.EntityId, snapshot.SequenceNumber);

            snapshot.CreatedAt = DateTime.UtcNow;

            _context.EditorHistory.Add(snapshot);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Saved snapshot with ID: {Id}", snapshot.Id);
            return snapshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving snapshot: SessionId={SessionId}", snapshot.EditorSessionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorHistorySnapshot>> SaveSnapshotBatchAsync(
        IEnumerable<EditorHistorySnapshot> snapshots,
        CancellationToken cancellationToken = default)
    {
        if (snapshots == null)
            throw new ArgumentNullException(nameof(snapshots));

        var snapshotList = snapshots.ToList();
        if (!snapshotList.Any())
            return new List<EditorHistorySnapshot>();

        try
        {
            _logger.LogInformation("Saving batch of {Count} snapshots", snapshotList.Count);

            var now = DateTime.UtcNow;
            foreach (var snapshot in snapshotList)
            {
                snapshot.CreatedAt = now;
            }

            _context.EditorHistory.AddRange(snapshotList);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Saved batch of {Count} snapshots", snapshotList.Count);
            return snapshotList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving snapshot batch");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorHistorySnapshot?> GetSnapshotByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshot by ID: {Id}", id);
            return await _context.EditorHistory
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshot by ID: {Id}", id);
            throw;
        }
    }

    // ========================================================================
    // SESSION-BASED QUERIES (for Undo/Redo)
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<EditorHistorySnapshot>> GetSessionSnapshotsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshots for session: {SessionId}", sessionId);
            return await _context.EditorHistory
                .AsNoTracking()
                .Where(s => s.EditorSessionId == sessionId)
                .OrderByDescending(s => s.SequenceNumber)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshots for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorHistorySnapshot>> GetSessionSnapshotsUpToAsync(
        Guid sessionId,
        int maxSequenceNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshots for session up to sequence: SessionId={SessionId}, MaxSeq={MaxSeq}",
                sessionId, maxSequenceNumber);

            return await _context.EditorHistory
                .AsNoTracking()
                .Where(s => s.EditorSessionId == sessionId && s.SequenceNumber <= maxSequenceNumber)
                .OrderByDescending(s => s.SequenceNumber)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshots up to sequence: SessionId={SessionId}", sessionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorHistorySnapshot?> GetLatestSnapshotAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting latest snapshot for session: {SessionId}", sessionId);
            return await _context.EditorHistory
                .AsNoTracking()
                .Where(s => s.EditorSessionId == sessionId)
                .OrderByDescending(s => s.SequenceNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest snapshot for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorHistorySnapshot?> GetSnapshotBySequenceAsync(
        Guid sessionId,
        int sequenceNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshot by sequence: SessionId={SessionId}, Sequence={Sequence}",
                sessionId, sequenceNumber);

            return await _context.EditorHistory
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.EditorSessionId == sessionId && s.SequenceNumber == sequenceNumber,
                    cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshot by sequence: SessionId={SessionId}, Sequence={Sequence}",
                sessionId, sequenceNumber);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetNextSequenceNumberAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting next sequence number for session: {SessionId}", sessionId);

            var maxSequence = await _context.EditorHistory
                .AsNoTracking()
                .Where(s => s.EditorSessionId == sessionId)
                .MaxAsync(s => (int?)s.SequenceNumber, cancellationToken) ?? 0;

            return maxSequence + 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next sequence number for session: {SessionId}", sessionId);
            throw;
        }
    }

    // ========================================================================
    // ENTITY-BASED QUERIES
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<EditorHistorySnapshot>> GetEntitySnapshotsAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshots for entity: Type={EntityType}, Id={EntityId}", entityType, entityId);

            return await _context.EditorHistory
                .AsNoTracking()
                .Where(s => s.EntityType == entityType && s.EntityId == entityId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshots for entity: Type={EntityType}, Id={EntityId}",
                entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorHistorySnapshot>> GetEntitySnapshotsByDateRangeAsync(
        string entityType,
        int entityId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshots for entity by date range: Type={EntityType}, Id={EntityId}",
                entityType, entityId);

            return await _context.EditorHistory
                .AsNoTracking()
                .Where(s => s.EntityType == entityType &&
                           s.EntityId == entityId &&
                           s.CreatedAt >= startDate &&
                           s.CreatedAt <= endDate)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshots for entity by date range");
            throw;
        }
    }

    // ========================================================================
    // CLEANUP OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<int> DeleteSessionSnapshotsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting snapshots for session: {SessionId}", sessionId);

            var snapshots = await _context.EditorHistory
                .Where(s => s.EditorSessionId == sessionId)
                .ToListAsync(cancellationToken);

            var count = snapshots.Count;
            _context.EditorHistory.RemoveRange(snapshots);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted {Count} snapshots for session: {SessionId}", count, sessionId);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting snapshots for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> DeleteSnapshotsOlderThanAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting snapshots older than: {OlderThan}", olderThan);

            var snapshots = await _context.EditorHistory
                .Where(s => s.CreatedAt < olderThan)
                .ToListAsync(cancellationToken);

            var count = snapshots.Count;
            if (count > 0)
            {
                _context.EditorHistory.RemoveRange(snapshots);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Deleted {Count} old snapshots", count);
            }
            else
            {
                _logger.LogInformation("No old snapshots found to delete");
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting snapshots older than: {OlderThan}", olderThan);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> DeleteEntitySnapshotsAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Deleting ALL snapshots for entity: Type={EntityType}, Id={EntityId}",
                entityType, entityId);

            var snapshots = await _context.EditorHistory
                .Where(s => s.EntityType == entityType && s.EntityId == entityId)
                .ToListAsync(cancellationToken);

            var count = snapshots.Count;
            _context.EditorHistory.RemoveRange(snapshots);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Deleted {Count} snapshots for entity: Type={EntityType}, Id={EntityId}",
                count, entityType, entityId);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting snapshots for entity: Type={EntityType}, Id={EntityId}",
                entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> TrimSessionHistoryAsync(
        Guid sessionId,
        int keepCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Trimming session history: SessionId={SessionId}, KeepCount={KeepCount}",
                sessionId, keepCount);

            var allSnapshots = await _context.EditorHistory
                .Where(s => s.EditorSessionId == sessionId)
                .OrderByDescending(s => s.SequenceNumber)
                .ToListAsync(cancellationToken);

            if (allSnapshots.Count <= keepCount)
            {
                _logger.LogDebug("Session has {Count} snapshots, no trimming needed", allSnapshots.Count);
                return 0;
            }

            var snapshotsToDelete = allSnapshots.Skip(keepCount).ToList();
            var count = snapshotsToDelete.Count;

            _context.EditorHistory.RemoveRange(snapshotsToDelete);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Trimmed {Count} snapshots from session: {SessionId}", count, sessionId);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error trimming session history: SessionId={SessionId}", sessionId);
            throw;
        }
    }

    // ========================================================================
    // COUNT & STATISTICS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<int> GetSessionSnapshotCountAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshot count for session: {SessionId}", sessionId);
            return await _context.EditorHistory
                .AsNoTracking()
                .CountAsync(s => s.EditorSessionId == sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshot count for session: {SessionId}", sessionId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalSnapshotCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting total snapshot count");
            return await _context.EditorHistory
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total snapshot count");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<long> GetTotalStorageSizeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting total storage size");

            var snapshots = await _context.EditorHistory
                .AsNoTracking()
                .Select(s => s.SnapshotJson.Length)
                .ToListAsync(cancellationToken);

            var totalSize = snapshots.Sum(len => (long)len * 2); // UTF-16 = 2 bytes per char
            _logger.LogDebug("Total storage size: {TotalSize} bytes", totalSize);
            return totalSize;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total storage size");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SnapshotStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting snapshot statistics");

            var totalCount = await _context.EditorHistory.AsNoTracking().CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new SnapshotStatistics
                {
                    TotalCount = 0,
                    UniqueSessionCount = 0,
                    OldestSnapshotDate = null,
                    NewestSnapshotDate = null,
                    TotalStorageBytes = 0,
                    AverageSnapshotsPerSession = 0
                };
            }

            var uniqueSessionCount = await _context.EditorHistory
                .AsNoTracking()
                .Select(s => s.EditorSessionId)
                .Distinct()
                .CountAsync(cancellationToken);

            var oldestDate = await _context.EditorHistory
                .AsNoTracking()
                .MinAsync(s => (DateTime?)s.CreatedAt, cancellationToken);

            var newestDate = await _context.EditorHistory
                .AsNoTracking()
                .MaxAsync(s => (DateTime?)s.CreatedAt, cancellationToken);

            var totalStorageBytes = await GetTotalStorageSizeAsync(cancellationToken);

            var averageSnapshotsPerSession = uniqueSessionCount > 0
                ? (double)totalCount / uniqueSessionCount
                : 0;

            var statistics = new SnapshotStatistics
            {
                TotalCount = totalCount,
                UniqueSessionCount = uniqueSessionCount,
                OldestSnapshotDate = oldestDate,
                NewestSnapshotDate = newestDate,
                TotalStorageBytes = totalStorageBytes,
                AverageSnapshotsPerSession = averageSnapshotsPerSession
            };

            _logger.LogInformation("Snapshot statistics: Total={Total}, Sessions={Sessions}, Storage={Storage}MB",
                statistics.TotalCount, statistics.UniqueSessionCount, statistics.TotalStorageBytes / 1024 / 1024);

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting snapshot statistics");
            throw;
        }
    }

    // ========================================================================
    // BULK OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<int> DeleteMultipleSessionsAsync(
        IEnumerable<Guid> sessionIds,
        CancellationToken cancellationToken = default)
    {
        if (sessionIds == null)
            throw new ArgumentNullException(nameof(sessionIds));

        var sessionIdList = sessionIds.ToList();
        if (!sessionIdList.Any())
            return 0;

        try
        {
            _logger.LogInformation("Deleting snapshots for {Count} sessions", sessionIdList.Count);

            var snapshots = await _context.EditorHistory
                .Where(s => sessionIdList.Contains(s.EditorSessionId))
                .ToListAsync(cancellationToken);

            var count = snapshots.Count;
            _context.EditorHistory.RemoveRange(snapshots);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted {Count} snapshots for {SessionCount} sessions",
                count, sessionIdList.Count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting snapshots for multiple sessions");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetOldestSnapshotDateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting oldest snapshot date");
            return await _context.EditorHistory
                .AsNoTracking()
                .MinAsync(s => (DateTime?)s.CreatedAt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting oldest snapshot date");
            throw;
        }
    }

    /// <summary>
    /// Performs automatic cleanup of snapshots older than the retention period (90 days).
    /// This method should be called periodically (e.g., daily background job).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of snapshots deleted</returns>
    public async Task<int> PerformAutomaticCleanupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow - DefaultRetentionPeriod;
            _logger.LogInformation("Performing automatic cleanup of snapshots older than {CutoffDate}", cutoffDate);

            var deletedCount = await DeleteSnapshotsOlderThanAsync(cutoffDate, cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Automatic cleanup completed: Deleted {Count} old snapshots", deletedCount);
            }
            else
            {
                _logger.LogInformation("Automatic cleanup completed: No old snapshots to delete");
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing automatic cleanup");
            throw;
        }
    }
}
