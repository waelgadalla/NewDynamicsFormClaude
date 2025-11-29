using DynamicForms.Editor.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository implementation for managing application configuration settings.
/// Provides type-safe access to configuration values stored in the database.
/// </summary>
public class EditorConfigurationRepository : IEditorConfigurationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EditorConfigurationRepository> _logger;

    // Default configuration values
    private static readonly Dictionary<string, (string Value, string Type, string Description)> DefaultConfigs = new()
    {
        { "AutoSave.IntervalSeconds", ("30", "Int", "Auto-save interval in seconds") },
        { "UndoRedo.MaxActions", ("100", "Int", "Maximum undo/redo actions per session") },
        { "History.RetentionDays", ("90", "Int", "Days to keep history before cleanup") }
    };

    /// <summary>
    /// Initializes a new instance of the EditorConfigurationRepository.
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public EditorConfigurationRepository(ApplicationDbContext context, ILogger<EditorConfigurationRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========================================================================
    // BASIC CRUD OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting configuration by ID: {Id}", id);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration by ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem?> GetByKeyAsync(string configKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting configuration by key: {ConfigKey}", configKey);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConfigKey == configKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration by key: {ConfigKey}", configKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all configuration items");
            return await _context.EditorConfiguration
                .AsNoTracking()
                .OrderBy(c => c.ConfigKey)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all configuration items");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> CreateAsync(
        EditorConfigurationItem configItem,
        CancellationToken cancellationToken = default)
    {
        if (configItem == null)
            throw new ArgumentNullException(nameof(configItem));

        try
        {
            _logger.LogInformation("Creating configuration: Key={ConfigKey}, Value={ConfigValue}",
                configItem.ConfigKey, configItem.ConfigValue);

            configItem.ModifiedAt = DateTime.UtcNow;

            _context.EditorConfiguration.Add(configItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created configuration with ID: {Id}", configItem.Id);
            return configItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating configuration: Key={ConfigKey}", configItem.ConfigKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> UpdateAsync(
        EditorConfigurationItem configItem,
        CancellationToken cancellationToken = default)
    {
        if (configItem == null)
            throw new ArgumentNullException(nameof(configItem));

        try
        {
            _logger.LogInformation("Updating configuration: ID={Id}, Key={ConfigKey}",
                configItem.Id, configItem.ConfigKey);

            var existing = await _context.EditorConfiguration
                .FirstOrDefaultAsync(c => c.Id == configItem.Id, cancellationToken);

            if (existing == null)
            {
                _logger.LogWarning("Configuration not found for update: ID={Id}", configItem.Id);
                throw new InvalidOperationException($"Configuration with ID {configItem.Id} not found");
            }

            existing.ConfigKey = configItem.ConfigKey;
            existing.ConfigValue = configItem.ConfigValue;
            existing.ConfigType = configItem.ConfigType;
            existing.Description = configItem.Description;
            existing.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated configuration: ID={Id}", configItem.Id);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration: ID={Id}", configItem.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting configuration: ID={Id}", id);

            var configItem = await _context.EditorConfiguration
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (configItem == null)
            {
                _logger.LogWarning("Configuration not found for deletion: ID={Id}", id);
                return false;
            }

            _context.EditorConfiguration.Remove(configItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted configuration: ID={Id}, Key={ConfigKey}", id, configItem.ConfigKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration: ID={Id}", id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteByKeyAsync(string configKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting configuration by key: {ConfigKey}", configKey);

            var configItem = await _context.EditorConfiguration
                .FirstOrDefaultAsync(c => c.ConfigKey == configKey, cancellationToken);

            if (configItem == null)
            {
                _logger.LogWarning("Configuration not found for deletion: Key={ConfigKey}", configKey);
                return false;
            }

            _context.EditorConfiguration.Remove(configItem);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted configuration: Key={ConfigKey}", configKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting configuration by key: {ConfigKey}", configKey);
            throw;
        }
    }

    // ========================================================================
    // TYPE-SAFE VALUE ACCESSORS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<string> GetStringValueAsync(
        string configKey,
        string defaultValue = "",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configItem = await GetByKeyAsync(configKey, cancellationToken);
            if (configItem == null)
            {
                _logger.LogDebug("Configuration key not found, returning default: Key={ConfigKey}", configKey);
                return defaultValue;
            }

            return configItem.ConfigValue ?? defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting string value: Key={ConfigKey}", configKey);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetIntValueAsync(
        string configKey,
        int defaultValue = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configItem = await GetByKeyAsync(configKey, cancellationToken);
            if (configItem == null)
            {
                _logger.LogDebug("Configuration key not found, returning default: Key={ConfigKey}", configKey);
                return defaultValue;
            }

            if (int.TryParse(configItem.ConfigValue, out var intValue))
            {
                return intValue;
            }

            _logger.LogWarning("Failed to parse int value for key {ConfigKey}, returning default", configKey);
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting int value: Key={ConfigKey}", configKey);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> GetBoolValueAsync(
        string configKey,
        bool defaultValue = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configItem = await GetByKeyAsync(configKey, cancellationToken);
            if (configItem == null)
            {
                _logger.LogDebug("Configuration key not found, returning default: Key={ConfigKey}", configKey);
                return defaultValue;
            }

            if (bool.TryParse(configItem.ConfigValue, out var boolValue))
            {
                return boolValue;
            }

            // Also handle "1"/"0" and "yes"/"no"
            var lowerValue = configItem.ConfigValue?.ToLower();
            if (lowerValue == "1" || lowerValue == "yes")
                return true;
            if (lowerValue == "0" || lowerValue == "no")
                return false;

            _logger.LogWarning("Failed to parse bool value for key {ConfigKey}, returning default", configKey);
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bool value: Key={ConfigKey}", configKey);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task<decimal> GetDecimalValueAsync(
        string configKey,
        decimal defaultValue = 0m,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configItem = await GetByKeyAsync(configKey, cancellationToken);
            if (configItem == null)
            {
                _logger.LogDebug("Configuration key not found, returning default: Key={ConfigKey}", configKey);
                return defaultValue;
            }

            if (decimal.TryParse(configItem.ConfigValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue))
            {
                return decimalValue;
            }

            _logger.LogWarning("Failed to parse decimal value for key {ConfigKey}, returning default", configKey);
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting decimal value: Key={ConfigKey}", configKey);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task<DateTime> GetDateTimeValueAsync(
        string configKey,
        DateTime defaultValue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var configItem = await GetByKeyAsync(configKey, cancellationToken);
            if (configItem == null)
            {
                _logger.LogDebug("Configuration key not found, returning default: Key={ConfigKey}", configKey);
                return defaultValue;
            }

            if (DateTime.TryParse(configItem.ConfigValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeValue))
            {
                return dateTimeValue;
            }

            _logger.LogWarning("Failed to parse DateTime value for key {ConfigKey}, returning default", configKey);
            return defaultValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting DateTime value: Key={ConfigKey}", configKey);
            return defaultValue;
        }
    }

    // ========================================================================
    // TYPE-SAFE VALUE SETTERS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> SetStringValueAsync(
        string configKey,
        string value,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        return await UpsertConfigAsync(configKey, value, "String", description, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> SetIntValueAsync(
        string configKey,
        int value,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        return await UpsertConfigAsync(configKey, value.ToString(), "Int", description, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> SetBoolValueAsync(
        string configKey,
        bool value,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        return await UpsertConfigAsync(configKey, value.ToString(), "Bool", description, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> SetDecimalValueAsync(
        string configKey,
        decimal value,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        return await UpsertConfigAsync(configKey, value.ToString(CultureInfo.InvariantCulture), "Decimal", description, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<EditorConfigurationItem> SetDateTimeValueAsync(
        string configKey,
        DateTime value,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        return await UpsertConfigAsync(configKey, value.ToString("O"), "DateTime", description, cancellationToken);
    }

    /// <summary>
    /// Internal helper method to upsert (insert or update) a configuration value.
    /// </summary>
    private async Task<EditorConfigurationItem> UpsertConfigAsync(
        string configKey,
        string configValue,
        string configType,
        string? description,
        CancellationToken cancellationToken)
    {
        try
        {
            var existing = await _context.EditorConfiguration
                .FirstOrDefaultAsync(c => c.ConfigKey == configKey, cancellationToken);

            if (existing != null)
            {
                // Update existing
                _logger.LogInformation("Updating configuration: Key={ConfigKey}, OldValue={OldValue}, NewValue={NewValue}",
                    configKey, existing.ConfigValue, configValue);

                existing.ConfigValue = configValue;
                existing.ConfigType = configType;
                if (description != null)
                    existing.Description = description;
                existing.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new
                _logger.LogInformation("Creating new configuration: Key={ConfigKey}, Value={ConfigValue}",
                    configKey, configValue);

                existing = new EditorConfigurationItem
                {
                    ConfigKey = configKey,
                    ConfigValue = configValue,
                    ConfigType = configType,
                    Description = description,
                    ModifiedAt = DateTime.UtcNow
                };
                _context.EditorConfiguration.Add(existing);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting configuration: Key={ConfigKey}", configKey);
            throw;
        }
    }

    // ========================================================================
    // QUERY METHODS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> GetByTypeAsync(
        string configType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting configurations by type: {ConfigType}", configType);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .Where(c => c.ConfigType == configType)
                .OrderBy(c => c.ConfigKey)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configurations by type: {ConfigType}", configType);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> GetByKeyPrefixAsync(
        string keyPrefix,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting configurations by key prefix: {KeyPrefix}", keyPrefix);
            var lowerPrefix = keyPrefix.ToLower();
            return await _context.EditorConfiguration
                .AsNoTracking()
                .Where(c => c.ConfigKey.ToLower().StartsWith(lowerPrefix))
                .OrderBy(c => c.ConfigKey)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configurations by key prefix: {KeyPrefix}", keyPrefix);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Searching configurations: {SearchTerm}", searchTerm);
            var lowerSearchTerm = searchTerm.ToLower();
            return await _context.EditorConfiguration
                .AsNoTracking()
                .Where(c => c.ConfigKey.ToLower().Contains(lowerSearchTerm) ||
                           (c.Description != null && c.Description.ToLower().Contains(lowerSearchTerm)))
                .OrderBy(c => c.ConfigKey)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching configurations: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting configurations by date range: {StartDate} to {EndDate}", startDate, endDate);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .Where(c => c.ModifiedAt >= startDate && c.ModifiedAt <= endDate)
                .OrderByDescending(c => c.ModifiedAt)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configurations by date range");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> GetRecentlyModifiedAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {Count} recently modified configurations", count);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .OrderByDescending(c => c.ModifiedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently modified configurations");
            throw;
        }
    }

    // ========================================================================
    // EXISTENCE & COUNT
    // ========================================================================

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string configKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if configuration key exists: {ConfigKey}", configKey);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .AnyAsync(c => c.ConfigKey == configKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if configuration exists: {ConfigKey}", configKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting count of all configurations");
            return await _context.EditorConfiguration
                .AsNoTracking()
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting count of configurations");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetCountByTypeAsync(string configType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting count of configurations by type: {ConfigType}", configType);
            return await _context.EditorConfiguration
                .AsNoTracking()
                .CountAsync(c => c.ConfigType == configType, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting count of configurations by type: {ConfigType}", configType);
            throw;
        }
    }

    // ========================================================================
    // BATCH OPERATIONS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> CreateBatchAsync(
        IEnumerable<EditorConfigurationItem> configItems,
        CancellationToken cancellationToken = default)
    {
        if (configItems == null)
            throw new ArgumentNullException(nameof(configItems));

        var itemList = configItems.ToList();
        if (!itemList.Any())
            return new List<EditorConfigurationItem>();

        try
        {
            _logger.LogInformation("Creating batch of {Count} configuration items", itemList.Count);

            var now = DateTime.UtcNow;
            foreach (var item in itemList)
            {
                item.ModifiedAt = now;
            }

            _context.EditorConfiguration.AddRange(itemList);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created batch of {Count} configuration items", itemList.Count);
            return itemList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch of configuration items");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> UpdateBatchAsync(
        IEnumerable<EditorConfigurationItem> configItems,
        CancellationToken cancellationToken = default)
    {
        if (configItems == null)
            throw new ArgumentNullException(nameof(configItems));

        var itemList = configItems.ToList();
        if (!itemList.Any())
            return new List<EditorConfigurationItem>();

        try
        {
            _logger.LogInformation("Updating batch of {Count} configuration items", itemList.Count);

            var ids = itemList.Select(i => i.Id).ToList();
            var existingItems = await _context.EditorConfiguration
                .Where(c => ids.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, cancellationToken);

            var updatedItems = new List<EditorConfigurationItem>();
            var now = DateTime.UtcNow;

            foreach (var item in itemList)
            {
                if (existingItems.TryGetValue(item.Id, out var existing))
                {
                    existing.ConfigKey = item.ConfigKey;
                    existing.ConfigValue = item.ConfigValue;
                    existing.ConfigType = item.ConfigType;
                    existing.Description = item.Description;
                    existing.ModifiedAt = now;
                    updatedItems.Add(existing);
                }
                else
                {
                    _logger.LogWarning("Configuration item not found for batch update: ID={Id}", item.Id);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated batch of {Count} configuration items", updatedItems.Count);
            return updatedItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating batch of configuration items");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<EditorConfigurationItem>> UpsertBatchAsync(
        IEnumerable<EditorConfigurationItem> configItems,
        CancellationToken cancellationToken = default)
    {
        if (configItems == null)
            throw new ArgumentNullException(nameof(configItems));

        var itemList = configItems.ToList();
        if (!itemList.Any())
            return new List<EditorConfigurationItem>();

        try
        {
            _logger.LogInformation("Upserting batch of {Count} configuration items", itemList.Count);

            var keys = itemList.Select(i => i.ConfigKey).ToList();
            var existingItems = await _context.EditorConfiguration
                .Where(c => keys.Contains(c.ConfigKey))
                .ToDictionaryAsync(c => c.ConfigKey, cancellationToken);

            var upsertedItems = new List<EditorConfigurationItem>();
            var now = DateTime.UtcNow;

            foreach (var item in itemList)
            {
                if (existingItems.TryGetValue(item.ConfigKey, out var existing))
                {
                    // Update existing
                    existing.ConfigValue = item.ConfigValue;
                    existing.ConfigType = item.ConfigType;
                    if (item.Description != null)
                        existing.Description = item.Description;
                    existing.ModifiedAt = now;
                    upsertedItems.Add(existing);
                }
                else
                {
                    // Insert new
                    item.ModifiedAt = now;
                    _context.EditorConfiguration.Add(item);
                    upsertedItems.Add(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Upserted batch of {Count} configuration items", upsertedItems.Count);
            return upsertedItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting batch of configuration items");
            throw;
        }
    }

    // ========================================================================
    // INITIALIZATION & DEFAULTS
    // ========================================================================

    /// <inheritdoc/>
    public async Task<int> InitializeDefaultsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initializing default configuration values");

            var existingKeys = await _context.EditorConfiguration
                .AsNoTracking()
                .Select(c => c.ConfigKey)
                .ToListAsync(cancellationToken);

            var missingDefaults = DefaultConfigs
                .Where(kvp => !existingKeys.Contains(kvp.Key))
                .Select(kvp => new EditorConfigurationItem
                {
                    ConfigKey = kvp.Key,
                    ConfigValue = kvp.Value.Value,
                    ConfigType = kvp.Value.Type,
                    Description = kvp.Value.Description,
                    ModifiedAt = DateTime.UtcNow
                })
                .ToList();

            if (missingDefaults.Any())
            {
                _context.EditorConfiguration.AddRange(missingDefaults);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Initialized {Count} default configuration values", missingDefaults.Count);
            }
            else
            {
                _logger.LogInformation("All default configuration values already exist");
            }

            return missingDefaults.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing default configuration values");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ResetToDefaultAsync(string configKey, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!DefaultConfigs.TryGetValue(configKey, out var defaultConfig))
            {
                _logger.LogWarning("No default value exists for key: {ConfigKey}", configKey);
                return false;
            }

            _logger.LogInformation("Resetting configuration to default: Key={ConfigKey}", configKey);

            var existing = await _context.EditorConfiguration
                .FirstOrDefaultAsync(c => c.ConfigKey == configKey, cancellationToken);

            if (existing != null)
            {
                existing.ConfigValue = defaultConfig.Value;
                existing.ConfigType = defaultConfig.Type;
                existing.ModifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Reset configuration to default: Key={ConfigKey}, Value={Value}",
                    configKey, defaultConfig.Value);
                return true;
            }

            _logger.LogWarning("Configuration key not found for reset: {ConfigKey}", configKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting configuration to default: {ConfigKey}", configKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> GetModifiedKeysAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting modified configuration keys");

            var allConfigs = await _context.EditorConfiguration
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var modifiedKeys = allConfigs
                .Where(c => DefaultConfigs.TryGetValue(c.ConfigKey, out var defaultConfig) &&
                           c.ConfigValue != defaultConfig.Value)
                .Select(c => c.ConfigKey)
                .ToList();

            _logger.LogDebug("Found {Count} modified configuration keys", modifiedKeys.Count);
            return modifiedKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting modified configuration keys");
            throw;
        }
    }
}
