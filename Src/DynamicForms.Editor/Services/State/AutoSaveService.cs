using System.Text.Json;
using DynamicForms.Editor.Data.Entities;
using DynamicForms.Editor.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Service for automatic background saving of editor state.
/// Uses a timer to periodically save changes to the database.
/// Thread-safe and disposable.
/// </summary>
public class AutoSaveService : IDisposable
{
    private readonly EditorStateService _editorState;
    private readonly IEditorModuleRepository _moduleRepository;
    private readonly IEditorConfigurationRepository _configRepository;
    private readonly ILogger<AutoSaveService> _logger;
    private readonly object _lock = new object();
    private Timer? _timer;
    private bool _disposed;
    private int _consecutiveFailures;

    // ========================================================================
    // CONFIGURATION
    // ========================================================================

    private const string AutoSaveIntervalConfigKey = "AutoSave.IntervalSeconds";
    private const int DefaultAutoSaveIntervalSeconds = 30;

    // ========================================================================
    // PROPERTIES
    // ========================================================================

    /// <summary>
    /// Gets or sets whether auto-save is enabled.
    /// When disabled, the timer stops but can be re-enabled later.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            lock (_lock)
            {
                return _timer != null;
            }
        }
    }

    /// <summary>
    /// Gets the timestamp of the last successful auto-save (UTC).
    /// Returns null if no auto-save has occurred yet.
    /// </summary>
    public DateTime? LastAutoSave { get; private set; }

    /// <summary>
    /// Gets whether a save operation is currently in progress.
    /// </summary>
    public bool IsSaving { get; private set; }

    /// <summary>
    /// Gets the configured auto-save interval in seconds.
    /// </summary>
    public int IntervalSeconds { get; private set; } = DefaultAutoSaveIntervalSeconds;

    // ========================================================================
    // EVENTS
    // ========================================================================

    /// <summary>
    /// Event fired when auto-save completes successfully.
    /// </summary>
    public event EventHandler<AutoSaveEventArgs>? AutoSaveCompleted;

    /// <summary>
    /// Event fired when auto-save encounters an error.
    /// </summary>
    public event EventHandler<AutoSaveErrorEventArgs>? AutoSaveError;

    // ========================================================================
    // CONSTRUCTOR
    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the AutoSaveService class.
    /// </summary>
    /// <param name="editorState">Editor state service</param>
    /// <param name="moduleRepository">Module repository for saving data</param>
    /// <param name="configRepository">Configuration repository for settings</param>
    /// <param name="logger">Logger instance</param>
    public AutoSaveService(
        EditorStateService editorState,
        IEditorModuleRepository moduleRepository,
        IEditorConfigurationRepository configRepository,
        ILogger<AutoSaveService> logger)
    {
        _editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));
        _moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to state changes to reset failure counter on manual saves
        _editorState.StateChanged += OnEditorStateChanged;
    }

    // ========================================================================
    // PUBLIC METHODS
    // ========================================================================

    /// <summary>
    /// Starts the auto-save timer.
    /// Loads the interval from configuration and begins periodic saves.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AutoSaveService));

            if (_timer != null)
            {
                _logger.LogWarning("Auto-save is already started");
                return;
            }
        }

        // Load interval from configuration
        try
        {
            IntervalSeconds = await _configRepository.GetIntValueAsync(
                AutoSaveIntervalConfigKey,
                DefaultAutoSaveIntervalSeconds,
                cancellationToken);

            if (IntervalSeconds < 5)
            {
                _logger.LogWarning(
                    "Auto-save interval {Interval}s is too short, using minimum of 5s",
                    IntervalSeconds);
                IntervalSeconds = 5;
            }

            _logger.LogInformation(
                "Auto-save started with interval of {Interval} seconds",
                IntervalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load auto-save interval from configuration, using default");
            IntervalSeconds = DefaultAutoSaveIntervalSeconds;
        }

        // Create and start timer
        lock (_lock)
        {
            var intervalMs = IntervalSeconds * 1000;
            _timer = new Timer(
                callback: _ => _ = PerformAutoSaveAsync(isManual: false),
                state: null,
                dueTime: intervalMs,
                period: intervalMs);
        }
    }

    /// <summary>
    /// Stops the auto-save timer.
    /// Any in-progress save will complete, but no new saves will start.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (_timer == null)
            {
                _logger.LogWarning("Auto-save is already stopped");
                return;
            }

            _timer.Dispose();
            _timer = null;

            _logger.LogInformation("Auto-save stopped");
        }
    }

    /// <summary>
    /// Triggers an immediate save, regardless of IsDirty state or timer schedule.
    /// This is a manual save operation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if save was successful; false otherwise</returns>
    public async Task<bool> SaveNowAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AutoSaveService));

        _logger.LogInformation("Manual save triggered");
        return await PerformAutoSaveAsync(isManual: true, cancellationToken);
    }

    // ========================================================================
    // PRIVATE METHODS
    // ========================================================================

    /// <summary>
    /// Performs the actual auto-save operation.
    /// Checks if save is needed, serializes state, and saves to database.
    /// </summary>
    /// <param name="isManual">Whether this is a manual save (bypasses IsDirty check)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if save was successful; false otherwise</returns>
    private async Task<bool> PerformAutoSaveAsync(
        bool isManual = false,
        CancellationToken cancellationToken = default)
    {
        // Check if already saving
        lock (_lock)
        {
            if (IsSaving)
            {
                _logger.LogDebug("Skipping auto-save - save already in progress");
                return false;
            }
            IsSaving = true;
        }

        try
        {
            // Get current editor state
            var sessionId = _editorState.EditorSessionId;
            var entityType = _editorState.EntityType;
            var isDirty = _editorState.IsDirty;

            // Check if save is needed (unless manual)
            if (!isManual && !isDirty)
            {
                _logger.LogDebug("Skipping auto-save - no unsaved changes");
                return false;
            }

            // Check if anything is loaded
            var currentModule = _editorState.GetCurrentModule();
            var currentWorkflow = _editorState.GetCurrentWorkflow();

            if (currentModule == null && currentWorkflow == null)
            {
                _logger.LogDebug("Skipping auto-save - no entity loaded");
                return false;
            }

            // Save based on entity type
            int entityId;
            long dataSize;

            if (entityType == EditorEntityType.Module && currentModule != null)
            {
                (entityId, dataSize) = await SaveModuleAsync(currentModule, cancellationToken);
            }
            else if (entityType == EditorEntityType.Workflow && currentWorkflow != null)
            {
                // TODO: Implement workflow saving when IEditorWorkflowRepository is available
                _logger.LogWarning("Workflow auto-save not yet implemented");
                return false;
            }
            else
            {
                _logger.LogWarning("Unknown entity type or null entity: {EntityType}", entityType);
                return false;
            }

            // Mark as saved
            _editorState.MarkAsSaved();

            // Update state
            var savedAt = DateTime.UtcNow;
            LastAutoSave = savedAt;
            _consecutiveFailures = 0;

            // Log success
            _logger.LogInformation(
                "Auto-save completed successfully: {EntityType} ID={EntityId}, Size={Size} bytes",
                entityType,
                entityId,
                dataSize);

            // Fire event
            OnAutoSaveCompleted(new AutoSaveEventArgs(
                sessionId: sessionId,
                entityType: entityType,
                entityId: entityId,
                savedAt: savedAt,
                dataSize: dataSize,
                isManualSave: isManual));

            return true;
        }
        catch (Exception ex)
        {
            _consecutiveFailures++;

            var errorAt = DateTime.UtcNow;
            var sessionId = _editorState.EditorSessionId;
            var entityType = _editorState.EntityType;

            _logger.LogError(
                ex,
                "Auto-save failed (attempt {FailureCount}): {EntityType}",
                _consecutiveFailures,
                entityType);

            // Fire error event
            OnAutoSaveError(new AutoSaveErrorEventArgs(
                sessionId: sessionId,
                entityType: entityType,
                exception: ex,
                errorAt: errorAt,
                isManualSave: isManual,
                failureCount: _consecutiveFailures));

            return false;
        }
        finally
        {
            lock (_lock)
            {
                IsSaving = false;
            }
        }
    }

    /// <summary>
    /// Saves a module to the database.
    /// </summary>
    /// <param name="module">Module to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (entity ID, data size in bytes)</returns>
    private async Task<(int EntityId, long DataSize)> SaveModuleAsync(
        DynamicForms.Core.V2.Schemas.FormModuleSchema module,
        CancellationToken cancellationToken)
    {
        // Serialize module to JSON
        var schemaJson = JsonSerializer.Serialize(module, new JsonSerializerOptions
        {
            WriteIndented = false
        });

        var dataSize = System.Text.Encoding.UTF8.GetByteCount(schemaJson);

        // Check if module exists
        var existingModule = await _moduleRepository.GetByModuleIdAsync(
            module.Id,
            cancellationToken);

        if (existingModule != null)
        {
            // Update existing module
            existingModule.Title = module.TitleEn;
            existingModule.TitleFr = module.TitleFr;
            existingModule.Description = module.DescriptionEn;
            existingModule.DescriptionFr = module.DescriptionFr;
            existingModule.SchemaJson = schemaJson;
            existingModule.ModifiedAt = DateTime.UtcNow;
            existingModule.ModifiedBy = "AutoSave"; // TODO: Get from auth context

            var updated = await _moduleRepository.UpdateAsync(existingModule, cancellationToken);
            return (updated.Id, dataSize);
        }
        else
        {
            // Create new module
            var newModule = new EditorFormModule
            {
                ModuleId = module.Id,
                Title = module.TitleEn,
                TitleFr = module.TitleFr,
                Description = module.DescriptionEn,
                DescriptionFr = module.DescriptionFr,
                SchemaJson = schemaJson,
                Version = 1,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = "AutoSave" // TODO: Get from auth context
            };

            var created = await _moduleRepository.CreateAsync(newModule, cancellationToken);
            return (created.Id, dataSize);
        }
    }

    /// <summary>
    /// Handles editor state changes.
    /// Resets failure counter when state is manually saved.
    /// </summary>
    private void OnEditorStateChanged(object? sender, EventArgs e)
    {
        // Reset failure counter when state changes (manual save occurred)
        if (!_editorState.IsDirty && _consecutiveFailures > 0)
        {
            _logger.LogDebug("Resetting auto-save failure counter after manual save");
            _consecutiveFailures = 0;
        }
    }

    /// <summary>
    /// Raises the AutoSaveCompleted event.
    /// </summary>
    protected virtual void OnAutoSaveCompleted(AutoSaveEventArgs e)
    {
        AutoSaveCompleted?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the AutoSaveError event.
    /// </summary>
    protected virtual void OnAutoSaveError(AutoSaveErrorEventArgs e)
    {
        AutoSaveError?.Invoke(this, e);
    }

    // ========================================================================
    // IDISPOSABLE IMPLEMENTATION
    // ========================================================================

    /// <summary>
    /// Disposes the auto-save service and stops the timer.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the auto-save service.
    /// </summary>
    /// <param name="disposing">Whether disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unsubscribe from events
            _editorState.StateChanged -= OnEditorStateChanged;

            // Dispose timer
            lock (_lock)
            {
                _timer?.Dispose();
                _timer = null;
            }

            _logger.LogInformation("AutoSaveService disposed");
        }

        _disposed = true;
    }
}
