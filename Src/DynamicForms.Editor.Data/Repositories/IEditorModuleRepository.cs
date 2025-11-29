using DynamicForms.Editor.Data.Entities;

namespace DynamicForms.Editor.Data.Repositories;

/// <summary>
/// Repository interface for managing draft form modules in the editor.
/// Provides CRUD operations and queries for modules being edited.
/// </summary>
public interface IEditorModuleRepository
{
    // ========================================================================
    // BASIC CRUD OPERATIONS
    // ========================================================================

    /// <summary>
    /// Gets a draft module by its internal database ID.
    /// </summary>
    /// <param name="id">Internal database ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The module if found; otherwise null</returns>
    Task<EditorFormModule?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current draft module by its business module ID.
    /// Returns the most recently modified draft for the given module ID.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The most recent draft module if found; otherwise null</returns>
    Task<EditorFormModule?> GetByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all draft modules, optionally filtered and ordered.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all draft modules</returns>
    Task<List<EditorFormModule>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new draft module.
    /// </summary>
    /// <param name="module">The module to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created module with ID assigned</returns>
    Task<EditorFormModule> CreateAsync(EditorFormModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing draft module.
    /// </summary>
    /// <param name="module">The module to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated module</returns>
    Task<EditorFormModule> UpdateAsync(EditorFormModule module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a draft module by its internal database ID.
    /// </summary>
    /// <param name="id">Internal database ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted; false if not found</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all draft modules for a given module ID.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of modules deleted</returns>
    Task<int> DeleteByModuleIdAsync(int moduleId, CancellationToken cancellationToken = default);

    // ========================================================================
    // QUERY METHODS
    // ========================================================================

    /// <summary>
    /// Gets all draft modules with a specific status.
    /// </summary>
    /// <param name="status">Status filter (e.g., "Draft", "Published", "Archived")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules with the specified status</returns>
    Task<List<EditorFormModule>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets modules modified within a specific date range.
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules modified within the date range</returns>
    Task<List<EditorFormModule>> GetByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets modules modified by a specific user.
    /// </summary>
    /// <param name="modifiedBy">Username who modified the module</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules modified by the specified user</returns>
    Task<List<EditorFormModule>> GetByModifiedByAsync(
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches modules by title (partial match, case-insensitive).
    /// </summary>
    /// <param name="searchTerm">Search term to match against Title or TitleFr</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of modules matching the search term</returns>
    Task<List<EditorFormModule>> SearchByTitleAsync(
        string searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets modules ordered by most recently modified first.
    /// </summary>
    /// <param name="count">Maximum number of modules to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of most recently modified modules</returns>
    Task<List<EditorFormModule>> GetRecentlyModifiedAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    // ========================================================================
    // VERSIONING & STATUS METHODS
    // ========================================================================

    /// <summary>
    /// Gets the next available module ID.
    /// Used when creating a completely new module.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Next available module ID</returns>
    Task<int> GetNextModuleIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current version number for a module ID.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current version number, or 0 if module doesn't exist</returns>
    Task<int> GetCurrentVersionAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a draft module.
    /// </summary>
    /// <param name="id">Internal database ID</param>
    /// <param name="newStatus">New status (e.g., "Draft", "Published", "Archived")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated; false if not found</returns>
    Task<bool> UpdateStatusAsync(int id, string newStatus, CancellationToken cancellationToken = default);

    // ========================================================================
    // EXISTENCE & COUNT METHODS
    // ========================================================================

    /// <summary>
    /// Checks if a module ID already exists in draft or published form.
    /// </summary>
    /// <param name="moduleId">Business module identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if module ID exists; otherwise false</returns>
    Task<bool> ExistsAsync(int moduleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of draft modules.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total count of draft modules</returns>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of draft modules by status.
    /// </summary>
    /// <param name="status">Status filter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of modules with the specified status</returns>
    Task<int> GetCountByStatusAsync(string status, CancellationToken cancellationToken = default);

    // ========================================================================
    // BATCH OPERATIONS
    // ========================================================================

    /// <summary>
    /// Creates multiple draft modules in a single transaction.
    /// </summary>
    /// <param name="modules">Modules to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created modules with IDs assigned</returns>
    Task<List<EditorFormModule>> CreateBatchAsync(
        IEnumerable<EditorFormModule> modules,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple draft modules in a single transaction.
    /// </summary>
    /// <param name="modules">Modules to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated modules</returns>
    Task<List<EditorFormModule>> UpdateBatchAsync(
        IEnumerable<EditorFormModule> modules,
        CancellationToken cancellationToken = default);
}
