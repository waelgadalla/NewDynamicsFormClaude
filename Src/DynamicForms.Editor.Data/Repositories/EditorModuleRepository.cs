using DynamicForms.Editor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository implementation for managing draft form modules in the editor.
/// Provides CRUD operations and queries for modules being edited.
/// </summary>
public class EditorModuleRepository : IEditorModuleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EditorModuleRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the EditorModuleRepository.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public EditorModuleRepository(ApplicationDbContext context, ILogger<EditorModuleRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // BASIC CRUD OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<EditorFormModule?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting editor module by ID: {Id}", id);
            return await _context.EditorFormModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting editor module by ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorFormModule?> GetByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting editor module by ModuleId: {ModuleId}", moduleId);
            return await _context.EditorFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId)
                .OrderByDescending(m => m.ModifiedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting editor module by ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all editor modules");
            return await _context.EditorFormModules
                .AsNoTracking()
                .OrderByDescending(m => m.ModifiedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all editor modules");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorFormModule> CreateAsync(EditorFormModule module, CancellationToken cancellationToken = default)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        try
        {
            _logger.LogInformation("Creating new editor module: ModuleId={ModuleId}, Title={Title}",
                module.ModuleId, module.Title);

            // Set timestamps
            module.CreatedAt = DateTime.UtcNow;
            module.ModifiedAt = DateTime.UtcNow;

            _context.EditorFormModules.Add(module);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created editor module with ID: {Id}", module.Id);
            return module;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating editor module: ModuleId={ModuleId}", module.ModuleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorFormModule> UpdateAsync(EditorFormModule module, CancellationToken cancellationToken = default)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        try
        {
            _logger.LogInformation("Updating editor module: ID={Id}, ModuleId={ModuleId}",
                module.Id, module.ModuleId);

            var existing = await _context.EditorFormModules
                .FirstOrDefaultAsync(m => m.Id == module.Id, cancellationToken);

            if (existing == null)
            {
                _logger.LogWarning("Editor module not found for update: ID={Id}", module.Id);
                throw new InvalidOperationException($"Editor module with ID {module.Id} not found");
            }

            // Update properties
            existing.ModuleId = module.ModuleId;
            existing.Title = module.Title;
            existing.TitleFr = module.TitleFr;
            existing.Description = module.Description;
            existing.DescriptionFr = module.DescriptionFr;
            existing.SchemaJson = module.SchemaJson;
            existing.Version = module.Version;
            existing.Status = module.Status;
            existing.ModifiedBy = module.ModifiedBy;
            // ModifiedAt will be set automatically by SaveChangesAsync

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated editor module: ID={Id}", module.Id);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating editor module: ID={Id}", module.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting editor module: ID={Id}", id);

            var module = await _context.EditorFormModules
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (module == null)
            {
                _logger.LogWarning("Editor module not found for deletion: ID={Id}", id);
                return false;
            }

            _context.EditorFormModules.Remove(module);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted editor module: ID={Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting editor module: ID={Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> DeleteByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting all editor modules by ModuleId: {ModuleId}", moduleId);

            var modules = await _context.EditorFormModules
                .Where(m => m.ModuleId == moduleId)
                .ToListAsync(cancellationToken);

            var count = modules.Count;
            _context.EditorFormModules.RemoveRange(modules);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted {Count} editor modules for ModuleId: {ModuleId}", count, moduleId);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting editor modules by ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    // ========================================================================
    // QUERY METHODS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting editor modules by status: {Status}", status);
            return await _context.EditorFormModules
                .AsNoTracking()
                .Where(m => m.Status == status)
                .OrderByDescending(m => m.ModifiedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting editor modules by status: {Status}", status);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting editor modules by date range: {StartDate} to {EndDate}", startDate, endDate);
            return await _context.EditorFormModules
                .AsNoTracking()
                .Where(m => m.ModifiedAt >= startDate && m.ModifiedAt <= endDate)
                .OrderByDescending(m => m.ModifiedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting editor modules by date range: {StartDate} to {EndDate}",
                startDate, endDate);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> GetByModifiedByAsync(
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting editor modules modified by: {ModifiedBy}", modifiedBy);
            return await _context.EditorFormModules
                .AsNoTracking()
                .Where(m => m.ModifiedBy == modifiedBy)
                .OrderByDescending(m => m.ModifiedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting editor modules modified by: {ModifiedBy}", modifiedBy);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> SearchByTitleAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Searching editor modules by title: {SearchTerm}", searchTerm);

            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.EditorFormModules
                .AsNoTracking()
                .Where(m => m.Title.ToLower().Contains(lowerSearchTerm) ||
                           (m.TitleFr != null && m.TitleFr.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(m => m.ModifiedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching editor modules by title: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> GetRecentlyModifiedAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {Count} recently modified editor modules", count);
            return await _context.EditorFormModules
                .AsNoTracking()
                .OrderByDescending(m => m.ModifiedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently modified editor modules");
            throw;
        }
    }

    // ========================================================================
    // VERSIONING & STATUS METHODS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<int> GetNextModuleIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting next available module ID");

            var maxModuleId = await _context.EditorFormModules
                .AsNoTracking()
                .MaxAsync(m => (int?)m.ModuleId, cancellationToken) ?? 0;

            var nextId = maxModuleId + 1;
            _logger.LogDebug("Next available module ID: {NextId}", nextId);
            return nextId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next module ID");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCurrentVersionAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting current version for ModuleId: {ModuleId}", moduleId);

            var currentVersion = await _context.EditorFormModules
                .AsNoTracking()
                .Where(m => m.ModuleId == moduleId)
                .MaxAsync(m => (int?)m.Version, cancellationToken) ?? 0;

            _logger.LogDebug("Current version for ModuleId {ModuleId}: {Version}", moduleId, currentVersion);
            return currentVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current version for ModuleId: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateStatusAsync(int id, string newStatus, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating status for editor module ID={Id} to {NewStatus}", id, newStatus);

            var module = await _context.EditorFormModules
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

            if (module == null)
            {
                _logger.LogWarning("Editor module not found for status update: ID={Id}", id);
                return false;
            }

            module.Status = newStatus;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated status for editor module ID={Id} to {NewStatus}", id, newStatus);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for editor module ID={Id}", id);
            throw;
        }
    }

    // ========================================================================
    // EXISTENCE & COUNT METHODS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(int moduleId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if ModuleId exists: {ModuleId}", moduleId);
            return await _context.EditorFormModules
                .AsNoTracking()
                .AnyAsync(m => m.ModuleId == moduleId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if ModuleId exists: {ModuleId}", moduleId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting count of all editor modules");
            return await _context.EditorFormModules
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting count of editor modules");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting count of editor modules by status: {Status}", status);
            return await _context.EditorFormModules
                .AsNoTracking()
                .CountAsync(m => m.Status == status, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting count of editor modules by status: {Status}", status);
            throw;
        }
    }

    // ========================================================================
    // BATCH OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> CreateBatchAsync(
        IEnumerable<EditorFormModule> modules,
        CancellationToken cancellationToken = default)
    {
        if (modules == null)
            throw new ArgumentNullException(nameof(modules));

        var moduleList = modules.ToList();
        if (!moduleList.Any())
            return new List<EditorFormModule>();

        try
        {
            _logger.LogInformation("Creating batch of {Count} editor modules", moduleList.Count);

            var now = DateTime.UtcNow;
            foreach (var module in moduleList)
            {
                module.CreatedAt = now;
                module.ModifiedAt = now;
            }

            _context.EditorFormModules.AddRange(moduleList);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created batch of {Count} editor modules", moduleList.Count);
            return moduleList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch of editor modules");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorFormModule>> UpdateBatchAsync(
        IEnumerable<EditorFormModule> modules,
        CancellationToken cancellationToken = default)
    {
        if (modules == null)
            throw new ArgumentNullException(nameof(modules));

        var moduleList = modules.ToList();
        if (!moduleList.Any())
            return new List<EditorFormModule>();

        try
        {
            _logger.LogInformation("Updating batch of {Count} editor modules", moduleList.Count);

            var ids = moduleList.Select(m => m.Id).ToList();
            var existingModules = await _context.EditorFormModules
                .Where(m => ids.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, cancellationToken);

            var updatedModules = new List<EditorFormModule>();
            foreach (var module in moduleList)
            {
                if (existingModules.TryGetValue(module.Id, out var existing))
                {
                    existing.ModuleId = module.ModuleId;
                    existing.Title = module.Title;
                    existing.TitleFr = module.TitleFr;
                    existing.Description = module.Description;
                    existing.DescriptionFr = module.DescriptionFr;
                    existing.SchemaJson = module.SchemaJson;
                    existing.Version = module.Version;
                    existing.Status = module.Status;
                    existing.ModifiedBy = module.ModifiedBy;
                    updatedModules.Add(existing);
                }
                else
                {
                    _logger.LogWarning("Editor module not found for batch update: ID={Id}", module.Id);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated batch of {Count} editor modules", updatedModules.Count);
            return updatedModules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch of editor modules");
            throw;
        }
    }
}
