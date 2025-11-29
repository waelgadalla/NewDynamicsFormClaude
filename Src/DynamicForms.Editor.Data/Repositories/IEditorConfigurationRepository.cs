using DynamicForms.Editor.Data.Entities;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository interface for managing application configuration settings.
/// Provides type-safe access to configuration values stored in the database.
/// </summary>
public interface IEditorConfigurationRepository
{
    // ========================================================================
    // BASIC CRUD OPERATIONS
    // ========================================================================

    /// <summary>
    /// Gets a configuration item by its internal database ID.
    /// </summary>
    /// <param name="id">Internal database ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration item if found; otherwise null</returns>
    Task<EditorConfigurationItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration item by its unique key.
    /// This is the primary method for retrieving configuration values.
    /// </summary>
    /// <param name="configKey">Configuration key (e.g., "AutoSave.IntervalSeconds")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration item if found; otherwise null</returns>
    Task<EditorConfigurationItem?> GetByKeyAsync(string configKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all configuration items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all configuration items</returns>
    Task<List<EditorConfigurationItem>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new configuration item.
    /// </summary>
    /// <param name="configItem">Configuration item to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created configuration item with ID assigned</returns>
    Task<EditorConfigurationItem> CreateAsync(
        EditorConfigurationItem configItem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing configuration item.
    /// </summary>
    /// <param name="configItem">Configuration item to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated configuration item</returns>
    Task<EditorConfigurationItem> UpdateAsync(
        EditorConfigurationItem configItem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration item by its internal database ID.
    /// </summary>
    /// <param name="id">Internal database ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted; false if not found</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration item by its unique key.
    /// </summary>
    /// <param name="configKey">Configuration key to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted; false if not found</returns>
    Task<bool> DeleteByKeyAsync(string configKey, CancellationToken cancellationToken = default);

    // ========================================================================
    // TYPE-SAFE VALUE ACCESSORS
    // ========================================================================

    /// <summary>
    /// Gets a configuration value as a string.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration value as string, or default value</returns>
    Task<string> GetStringValueAsync(
        string configKey,
        string defaultValue = "",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration value as an integer.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration value as integer, or default value</returns>
    Task<int> GetIntValueAsync(
        string configKey,
        int defaultValue = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration value as a boolean.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration value as boolean, or default value</returns>
    Task<bool> GetBoolValueAsync(
        string configKey,
        bool defaultValue = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration value as a decimal.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration value as decimal, or default value</returns>
    Task<decimal> GetDecimalValueAsync(
        string configKey,
        decimal defaultValue = 0m,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration value as a DateTime.
    /// Expects ISO 8601 format in storage.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found or conversion fails</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Configuration value as DateTime, or default value</returns>
    Task<DateTime> GetDateTimeValueAsync(
        string configKey,
        DateTime defaultValue,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // TYPE-SAFE VALUE SETTERS
    // ========================================================================

    /// <summary>
    /// Sets a configuration value from a string.
    /// Creates new configuration item if key doesn't exist, otherwise updates existing.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="value">String value to set</param>
    /// <param name="description">Optional description of what this configuration controls</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated or created configuration item</returns>
    Task<EditorConfigurationItem> SetStringValueAsync(
        string configKey,
        string value,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a configuration value from an integer.
    /// Creates new configuration item if key doesn't exist, otherwise updates existing.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="value">Integer value to set</param>
    /// <param name="description">Optional description of what this configuration controls</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated or created configuration item</returns>
    Task<EditorConfigurationItem> SetIntValueAsync(
        string configKey,
        int value,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a configuration value from a boolean.
    /// Creates new configuration item if key doesn't exist, otherwise updates existing.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="value">Boolean value to set</param>
    /// <param name="description">Optional description of what this configuration controls</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated or created configuration item</returns>
    Task<EditorConfigurationItem> SetBoolValueAsync(
        string configKey,
        bool value,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a configuration value from a decimal.
    /// Creates new configuration item if key doesn't exist, otherwise updates existing.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="value">Decimal value to set</param>
    /// <param name="description">Optional description of what this configuration controls</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated or created configuration item</returns>
    Task<EditorConfigurationItem> SetDecimalValueAsync(
        string configKey,
        decimal value,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a configuration value from a DateTime.
    /// Stores in ISO 8601 format.
    /// Creates new configuration item if key doesn't exist, otherwise updates existing.
    /// </summary>
    /// <param name="configKey">Configuration key</param>
    /// <param name="value">DateTime value to set</param>
    /// <param name="description">Optional description of what this configuration controls</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated or created configuration item</returns>
    Task<EditorConfigurationItem> SetDateTimeValueAsync(
        string configKey,
        DateTime value,
        string? description = null,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // QUERY METHODS
    // ========================================================================

    /// <summary>
    /// Gets all configuration items of a specific type.
    /// </summary>
    /// <param name="configType">Configuration type filter (e.g., "Int", "String", "Bool")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of configuration items with the specified type</returns>
    Task<List<EditorConfigurationItem>> GetByTypeAsync(
        string configType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configuration items with keys matching a prefix.
    /// Useful for getting all settings in a category (e.g., "AutoSave.*").
    /// </summary>
    /// <param name="keyPrefix">Key prefix to match (case-insensitive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of configuration items with keys starting with the prefix</returns>
    Task<List<EditorConfigurationItem>> GetByKeyPrefixAsync(
        string keyPrefix,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches configuration items by key or description (partial match, case-insensitive).
    /// </summary>
    /// <param name="searchTerm">Search term to match against ConfigKey or Description</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of configuration items matching the search term</returns>
    Task<List<EditorConfigurationItem>> SearchAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configuration items modified within a specific date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of configuration items modified within the date range</returns>
    Task<List<EditorConfigurationItem>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recently modified configuration items.
    /// </summary>
    /// <param name="count">Maximum number of items to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of most recently modified configuration items</returns>
    Task<List<EditorConfigurationItem>> GetRecentlyModifiedAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // EXISTENCE & COUNT
    // ========================================================================

    /// <summary>
    /// Checks if a configuration key exists.
    /// </summary>
    /// <param name="configKey">Configuration key to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the key exists; otherwise false</returns>
    Task<bool> ExistsAsync(string configKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of configuration items.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of configuration items</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of configuration items by type.
    /// </summary>
    /// <param name="configType">Configuration type filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of configuration items with the specified type</returns>
    Task<int> GetCountByTypeAsync(string configType, CancellationToken cancellationToken = default);

    // ========================================================================
    // BATCH OPERATIONS
    // ========================================================================

    /// <summary>
    /// Creates multiple configuration items in a single transaction.
    /// </summary>
    /// <param name="configItems">Configuration items to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created configuration items with IDs assigned</returns>
    Task<List<EditorConfigurationItem>> CreateBatchAsync(
        IEnumerable<EditorConfigurationItem> configItems,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple configuration items in a single transaction.
    /// </summary>
    /// <param name="configItems">Configuration items to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated configuration items</returns>
    Task<List<EditorConfigurationItem>> UpdateBatchAsync(
        IEnumerable<EditorConfigurationItem> configItems,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts (insert or update) multiple configuration items.
    /// If a key exists, it updates the value; otherwise it creates a new entry.
    /// </summary>
    /// <param name="configItems">Configuration items to upsert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Upserted configuration items</returns>
    Task<List<EditorConfigurationItem>> UpsertBatchAsync(
        IEnumerable<EditorConfigurationItem> configItems,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // INITIALIZATION & DEFAULTS
    // ========================================================================

    /// <summary>
    /// Initializes default configuration values if they don't exist.
    /// Safe to call multiple times - only creates missing keys.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of default configuration items created</returns>
    Task<int> InitializeDefaultsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a configuration value to its default.
    /// </summary>
    /// <param name="configKey">Configuration key to reset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if reset; false if key not found or no default exists</returns>
    Task<bool> ResetToDefaultAsync(string configKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all configuration keys that have been modified from their defaults.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of configuration keys that differ from defaults</returns>
    Task<List<string>> GetModifiedKeysAsync(CancellationToken cancellationToken = default);
}
