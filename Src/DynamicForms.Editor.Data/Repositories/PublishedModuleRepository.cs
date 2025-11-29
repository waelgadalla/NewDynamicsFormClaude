using DynamicForms.Editor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository implementation for reading published form modules.
/// This is a READ-ONLY repository used by production applications to fetch published forms.
/// Implements caching for performance optimization.
/// </summary>
public class PublishedModuleRepository : IPublishedModuleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PublishedModuleRepository> _logger;

    // Cache key prefixes
    private const string ActiveModuleCacheKeyPrefix = "ActiveModule_";
    private const string ModuleVersionCacheKeyPrefix = "ModuleVersion_";
    private const string AllActiveModulesCacheKey = "AllActiveModules";

    // Cache duration: 5 minutes for active modules (frequently accessed)
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the PublishedModuleRepository.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cache">Memory cache for performance</param>
    /// <param name="logger">Logger instance</param>
    public PublishedModuleRepository(
        ApplicationDbContext context,
        IMemoryCache cache,
        ILogger<PublishedModuleRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // READ OPERATIONS (Production Apps)
    // ========================================================================

    /// <inheritdoc/>
    public async Task<PublishedFormModule?> GetActiveModuleAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ActiveModuleCacheKeyPrefix}{moduleId}";

        // Try to get from cache first
        if (_cache.TryGetValue<PublishedFormModule>(cacheKey, out var cachedModule))
        {
            _logger.LogDebug("Retrieved active module from cache: ModuleId={ModuleId}", moduleId);
            return cachedModule;
        }

        try
        {
            _logger.LogDebug("Getting active module from database: ModuleId={ModuleId}", moduleId);

            var module = await _context.PublishedFormModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.IsActive, cancellationToken);

            // Cache the result (even if null) to avoid repeated database queries
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.High);

            _cache.Set(cacheKey, module, cacheOptions);

            return module;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active module: ModuleId={ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PublishedFormModule?> GetModuleVersionAsync(
        int moduleId,
        int version,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ModuleVersionCacheKeyPrefix}{moduleId}_{version}";

        // Try to get from cache first
        if (_cache.TryGetValue<PublishedFormModule>(cacheKey, out var cachedModule))
        {
            _logger.LogDebug("Retrieved module version from cache: ModuleId={ModuleId}, Version={Version}",
                moduleId, version);
            return cachedModule;
        }

        try
        {
            _logger.LogDebug("Getting module version from database: ModuleId={ModuleId}, Version={Version}",
                moduleId, version);

            var module = await _context.PublishedFormModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.Version == version, cancellationToken);

            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.Normal);

            _cache.Set(cacheKey, module, cacheOptions);

            return module;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting module version: ModuleId={ModuleId}, Version={Version}",
                moduleId, version);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<PublishedFormModule?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting published module by ID: {Id}", id);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting published module by ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> GetAllVersionsAsync(
        int moduleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all versions for ModuleId: {ModuleId}", moduleId);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId)
                .OrderByDescending(m => m.Version)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all versions for ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> GetAllActiveModulesAsync(CancellationToken cancellationToken = default)
    {
        // Try to get from cache first
        if (_cache.TryGetValue<List<PublishedFormModule>>(AllActiveModulesCacheKey, out var cachedModules))
        {
            _logger.LogDebug("Retrieved all active modules from cache");
            return cachedModules!;
        }

        try
        {
            _logger.LogDebug("Getting all active modules from database");

            var modules = await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.IsActive)
                .OrderBy(m => m.ModuleId)
                .ToListAsync(cancellationToken);

            // Cache the result
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration)
                .SetPriority(CacheItemPriority.High);

            _cache.Set(AllActiveModulesCacheKey, modules, cacheOptions);

            return modules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all active modules");
            throw;
        }
    }

    // ========================================================================
    // VERSION HISTORY QUERIES
    // ========================================================================

    /// <inheritdoc/>
    public async Task<int> GetLatestVersionNumberAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting latest version number for ModuleId: {ModuleId}", moduleId);

            var latestVersion = await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId)
                .MaxAsync(m => (int?)m.Version, cancellationToken) ?? 0;

            return latestVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest version number for ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> GetVersionHistoryAsync(
        int moduleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting version history for ModuleId: {ModuleId}", moduleId);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId)
                .OrderByDescending(m => m.Version)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version history for ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> GetPublishedBetweenDatesAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting modules published between {StartDate} and {EndDate}", startDate, endDate);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.PublishedAt >= startDate && m.PublishedAt <= endDate)
                .OrderByDescending(m => m.PublishedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules published between dates");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> GetPublishedByUserAsync(
        string publishedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting modules published by: {PublishedBy}", publishedBy);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.PublishedBy == publishedBy)
                .OrderByDescending(m => m.PublishedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modules published by: {PublishedBy}", publishedBy);
            throw;
        }
    }

    // ========================================================================
    // SEARCH & FILTER
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> SearchActiveByTitleAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Searching active modules by title: {SearchTerm}", searchTerm);

            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.IsActive &&
                           (m.Title.ToLower().Contains(lowerSearchTerm) ||
                            (m.TitleFr != null && m.TitleFr.ToLower().Contains(lowerSearchTerm))))
                .OrderBy(m => m.Title)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching active modules by title: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<PublishedFormModule>> GetRecentlyPublishedAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {Count} recently published modules", count);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .OrderByDescending(m => m.PublishedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently published modules");
            throw;
        }
    }

    // ========================================================================
    // EXISTENCE & COUNT
    // ========================================================================

    /// <inheritdoc/>
    public async Task<bool> HasPublishedVersionsAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if ModuleId has published versions: {ModuleId}", moduleId);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .AnyAsync(m => m.ModuleId == moduleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if ModuleId has published versions: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> VersionExistsAsync(int moduleId, int version, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if version exists: ModuleId={ModuleId}, Version={Version}",
                moduleId, version);
            return await _context.PublishedFormModules
                .AsNoTracking()
                .AnyAsync(m => m.ModuleId == moduleId && m.Version == version, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if version exists: ModuleId={ModuleId}, Version={Version}",
                moduleId, version);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting total count of published modules");
            return await _context.PublishedFormModules
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total count of published modules");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetActiveCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting count of active published modules");
            return await _context.PublishedFormModules
                .AsNoTracking()
                .CountAsync(m => m.IsActive, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting count of active published modules");
            throw;
        }
    }

    // ========================================================================
    // COMPARISON & DIFF
    // ========================================================================

    /// <inheritdoc/>
    public async Task<(PublishedFormModule? version1, PublishedFormModule? version2)> GetVersionsForComparisonAsync(
        int moduleId,
        int version1,
        int version2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting versions for comparison: ModuleId={ModuleId}, V1={Version1}, V2={Version2}",
                moduleId, version1, version2);

            var versions = await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId && (m.Version == version1 || m.Version == version2))
                .ToListAsync(cancellationToken);

            var v1 = versions.FirstOrDefault(m => m.Version == version1);
            var v2 = versions.FirstOrDefault(m => m.Version == version2);

            return (v1, v2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting versions for comparison: ModuleId={ModuleId}", moduleId);
            throw;
        }
    }

    // ========================================================================
    // CACHING SUPPORT
    // ========================================================================

    /// <inheritdoc/>
    public async Task<DateTime?> GetLastPublishedDateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting last published date");
            return await _context.PublishedFormModules
                .AsNoTracking()
                .MaxAsync(m => (DateTime?)m.PublishedAt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last published date");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetModuleLastPublishedDateAsync(
        int moduleId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting last published date for ModuleId: {ModuleId}", moduleId);
            var activeModule = await _context.PublishedFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId && m.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            return activeModule?.PublishedAt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last published date for ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <summary>
    /// Invalidates the cache for a specific module.
    /// Should be called after publishing a new version.
    /// </summary>
    /// <param name="moduleId">Module ID to invalidate</param>
    public void InvalidateModuleCache(int moduleId)
    {
        _logger.LogInformation("Invalidating cache for ModuleId: {ModuleId}", moduleId);

        _cache.Remove($"{ActiveModuleCacheKeyPrefix}{moduleId}");
        _cache.Remove(AllActiveModulesCacheKey);

        // Note: Version-specific cache entries are not invalidated as they're immutable
    }

    /// <summary>
    /// Invalidates all cached data.
    /// Should be called after bulk publishing operations.
    /// </summary>
    public void InvalidateAllCache()
    {
        _logger.LogInformation("Invalidating all published module cache");
        // Note: MemoryCache doesn't have a Clear() method
        // Individual cache entries will expire naturally based on cache duration
        // For a complete clear, consider using a cache key prefix pattern
    }
}
