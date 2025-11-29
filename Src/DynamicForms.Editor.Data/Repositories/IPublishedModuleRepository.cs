using DynamicForms.Editor.Data.Entities;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository interface for reading published form modules.
/// This is a READ-ONLY repository used by production applications to fetch published forms.
/// Publishing is handled by the PublishService, not this repository.
/// </summary>
public interface IPublishedModuleRepository
{
    // ========================================================================
    // READ OPERATIONS (Production Apps)
    // ========================================================================

    /// <summary>
    /// Gets the active (current production) version of a published module.
    /// Returns the most recent published version where IsActive = true.
    /// This is the primary method used by production applications.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active published module if found; otherwise null</returns>
    Task<PublishedFormModule?> GetActiveModuleAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of a published module.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="version">Version number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Published module for the specified version if found; otherwise null</returns>
    Task<PublishedFormModule?> GetModuleVersionAsync(
        int moduleId,
        int version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a published module by its internal database ID.
    /// </summary>
    /// <param name="id">Internal database ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Published module if found; otherwise null</returns>
    Task<PublishedFormModule?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all published versions of a module, ordered by version descending (newest first).
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all published versions for the module</returns>
    Task<List<PublishedFormModule>> GetAllVersionsAsync(
        int moduleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active (currently published) modules.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all active published modules</returns>
    Task<List<PublishedFormModule>> GetAllActiveModulesAsync(CancellationToken cancellationToken = default);

    // ========================================================================
    // VERSION HISTORY QUERIES
    // ========================================================================

    /// <summary>
    /// Gets the latest version number for a module.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest version number, or 0 if no published versions exist</returns>
    Task<int> GetLatestVersionNumberAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the version history for a module with publish metadata.
    /// Returns version number, published date, and who published it.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of version history entries ordered by version descending</returns>
    Task<List<PublishedFormModule>> GetVersionHistoryAsync(
        int moduleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets modules published within a specific date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules published within the date range</returns>
    Task<List<PublishedFormModule>> GetPublishedBetweenDatesAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets modules published by a specific user.
    /// </summary>
    /// <param name="publishedBy">Username who published the modules</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules published by the specified user</returns>
    Task<List<PublishedFormModule>> GetPublishedByUserAsync(
        string publishedBy,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // SEARCH & FILTER
    // ========================================================================

    /// <summary>
    /// Searches active published modules by title (partial match, case-insensitive).
    /// </summary>
    /// <param name="searchTerm">Search term to match against Title or TitleFr</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of active modules matching the search term</returns>
    Task<List<PublishedFormModule>> SearchActiveByTitleAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recently published modules (all versions).
    /// </summary>
    /// <param name="count">Maximum number of modules to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of most recently published modules</returns>
    Task<List<PublishedFormModule>> GetRecentlyPublishedAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // EXISTENCE & COUNT
    // ========================================================================

    /// <summary>
    /// Checks if a module has any published versions.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if at least one published version exists; otherwise false</returns>
    Task<bool> HasPublishedVersionsAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a specific version of a module is published.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="version">Version number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the version exists; otherwise false</returns>
    Task<bool> VersionExistsAsync(int moduleId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of published modules (all versions).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of published modules</returns>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of active (currently published) modules.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of active modules</returns>
    Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default);

    // ========================================================================
    // COMPARISON & DIFF
    // ========================================================================

    /// <summary>
    /// Gets two versions of a module for comparison.
    /// Useful for displaying differences between versions.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="version1">First version number</param>
    /// <param name="version2">Second version number</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (version1, version2) modules, or nulls if not found</returns>
    Task<(PublishedFormModule? version1, PublishedFormModule? version2)> GetVersionsForComparisonAsync(
        int moduleId,
        int version1,
        int version2,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // CACHING SUPPORT
    // ========================================================================

    /// <summary>
    /// Gets the last modified date for any published module.
    /// Useful for cache invalidation strategies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent PublishedAt timestamp, or null if no published modules</returns>
    Task<DateTime?> GetLastPublishedDateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the PublishedAt timestamp for a specific module's active version.
    /// Useful for cache invalidation.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>PublishedAt timestamp of active version, or null if not found</returns>
    Task<DateTime?> GetModuleLastPublishedDateAsync(
        int moduleId,
        CancellationToken cancellationToken = default);
}
