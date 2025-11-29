using DynamicForms.Core.V2.Schemas;

namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Service for managing the current editing state in the form editor.
/// Tracks the currently loaded module or workflow, dirty state, and modification history.
/// Thread-safe for concurrent access.
/// </summary>
public class EditorStateService
{
    private readonly object _lock = new object();
    private FormModuleSchema? _currentModule;
    private FormWorkflowSchema? _currentWorkflow;
    private bool _isDirty;
    private DateTime _lastModified;
    private DateTime? _lastSaved;

    // ========================================================================
    // EVENTS
    // ========================================================================

    /// <summary>
    /// Event fired when any state change occurs.
    /// This is a general-purpose event that fires for all state changes.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Event fired when the current module is loaded or updated.
    /// </summary>
    public event EventHandler? ModuleChanged;

    /// <summary>
    /// Event fired when the current workflow is loaded or updated.
    /// </summary>
    public event EventHandler? WorkflowChanged;

    // ========================================================================
    // PROPERTIES
    // ========================================================================

    /// <summary>
    /// Gets the unique identifier for the current editor session.
    /// A new GUID is generated each time a module or workflow is loaded.
    /// </summary>
    public Guid EditorSessionId { get; private set; }

    /// <summary>
    /// Gets the type of entity currently being edited (Module or Workflow).
    /// </summary>
    public EditorEntityType EntityType { get; private set; }

    /// <summary>
    /// Gets the currently loaded form module, or null if none is loaded.
    /// </summary>
    public FormModuleSchema? CurrentModule
    {
        get
        {
            lock (_lock)
            {
                return _currentModule;
            }
        }
        private set
        {
            lock (_lock)
            {
                _currentModule = value;
            }
        }
    }

    /// <summary>
    /// Gets the currently loaded workflow, or null if none is loaded.
    /// </summary>
    public FormWorkflowSchema? CurrentWorkflow
    {
        get
        {
            lock (_lock)
            {
                return _currentWorkflow;
            }
        }
        private set
        {
            lock (_lock)
            {
                _currentWorkflow = value;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current entity has unsaved changes.
    /// </summary>
    public bool IsDirty
    {
        get
        {
            lock (_lock)
            {
                return _isDirty;
            }
        }
        private set
        {
            lock (_lock)
            {
                _isDirty = value;
            }
        }
    }

    /// <summary>
    /// Gets the date and time when the entity was last modified.
    /// </summary>
    public DateTime LastModified
    {
        get
        {
            lock (_lock)
            {
                return _lastModified;
            }
        }
        private set
        {
            lock (_lock)
            {
                _lastModified = value;
            }
        }
    }

    /// <summary>
    /// Gets the date and time when the entity was last saved, or null if never saved.
    /// </summary>
    public DateTime? LastSaved
    {
        get
        {
            lock (_lock)
            {
                return _lastSaved;
            }
        }
        private set
        {
            lock (_lock)
            {
                _lastSaved = value;
            }
        }
    }

    // ========================================================================
    // PUBLIC METHODS
    // ========================================================================

    /// <summary>
    /// Loads a form module into the editor.
    /// Generates a new session ID and resets the dirty state.
    /// </summary>
    /// <param name="module">The form module to load</param>
    /// <exception cref="ArgumentNullException">Thrown when module is null</exception>
    public void LoadModule(FormModuleSchema module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        lock (_lock)
        {
            // Clear workflow if any
            _currentWorkflow = null;

            // Set module
            _currentModule = module;
            EntityType = EditorEntityType.Module;

            // Reset session state
            EditorSessionId = Guid.NewGuid();
            _isDirty = false;
            _lastModified = DateTime.UtcNow;
            _lastSaved = null;
        }

        // Fire events outside of lock
        OnModuleChanged();
        OnStateChanged();
    }

    /// <summary>
    /// Loads a workflow into the editor.
    /// Generates a new session ID and resets the dirty state.
    /// </summary>
    /// <param name="workflow">The workflow to load</param>
    /// <exception cref="ArgumentNullException">Thrown when workflow is null</exception>
    public void LoadWorkflow(FormWorkflowSchema workflow)
    {
        if (workflow == null)
            throw new ArgumentNullException(nameof(workflow));

        lock (_lock)
        {
            // Clear module if any
            _currentModule = null;

            // Set workflow
            _currentWorkflow = workflow;
            EntityType = EditorEntityType.Workflow;

            // Reset session state
            EditorSessionId = Guid.NewGuid();
            _isDirty = false;
            _lastModified = DateTime.UtcNow;
            _lastSaved = null;
        }

        // Fire events outside of lock
        OnWorkflowChanged();
        OnStateChanged();
    }

    /// <summary>
    /// Updates the current module with a new version.
    /// Sets IsDirty to true and updates LastModified timestamp.
    /// </summary>
    /// <param name="module">The updated module</param>
    /// <param name="actionDescription">Description of the action that triggered this update (for logging/history)</param>
    /// <exception cref="ArgumentNullException">Thrown when module is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when no module is currently loaded</exception>
    public void UpdateModule(FormModuleSchema module, string actionDescription)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));

        lock (_lock)
        {
            if (_currentModule == null)
                throw new InvalidOperationException("Cannot update module: no module is currently loaded.");

            if (EntityType != EditorEntityType.Module)
                throw new InvalidOperationException("Cannot update module: current entity type is not Module.");

            // Update module
            _currentModule = module;
            _isDirty = true;
            _lastModified = DateTime.UtcNow;
        }

        // Fire events outside of lock
        OnModuleChanged();
        OnStateChanged();
    }

    /// <summary>
    /// Updates the current workflow with a new version.
    /// Sets IsDirty to true and updates LastModified timestamp.
    /// </summary>
    /// <param name="workflow">The updated workflow</param>
    /// <param name="actionDescription">Description of the action that triggered this update (for logging/history)</param>
    /// <exception cref="ArgumentNullException">Thrown when workflow is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when no workflow is currently loaded</exception>
    public void UpdateWorkflow(FormWorkflowSchema workflow, string actionDescription)
    {
        if (workflow == null)
            throw new ArgumentNullException(nameof(workflow));

        lock (_lock)
        {
            if (_currentWorkflow == null)
                throw new InvalidOperationException("Cannot update workflow: no workflow is currently loaded.");

            if (EntityType != EditorEntityType.Workflow)
                throw new InvalidOperationException("Cannot update workflow: current entity type is not Workflow.");

            // Update workflow
            _currentWorkflow = workflow;
            _isDirty = true;
            _lastModified = DateTime.UtcNow;
        }

        // Fire events outside of lock
        OnWorkflowChanged();
        OnStateChanged();
    }

    /// <summary>
    /// Gets the currently loaded module.
    /// Returns null if no module is loaded or if a workflow is loaded instead.
    /// </summary>
    /// <returns>The current module, or null</returns>
    public FormModuleSchema? GetCurrentModule()
    {
        lock (_lock)
        {
            return _currentModule;
        }
    }

    /// <summary>
    /// Gets the currently loaded workflow.
    /// Returns null if no workflow is loaded or if a module is loaded instead.
    /// </summary>
    /// <returns>The current workflow, or null</returns>
    public FormWorkflowSchema? GetCurrentWorkflow()
    {
        lock (_lock)
        {
            return _currentWorkflow;
        }
    }

    /// <summary>
    /// Marks the current entity as saved.
    /// Sets IsDirty to false and updates LastSaved timestamp.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no entity is currently loaded</exception>
    public void MarkAsSaved()
    {
        lock (_lock)
        {
            if (_currentModule == null && _currentWorkflow == null)
                throw new InvalidOperationException("Cannot mark as saved: no entity is currently loaded.");

            _isDirty = false;
            _lastSaved = DateTime.UtcNow;
        }

        // Fire state changed event
        OnStateChanged();
    }

    /// <summary>
    /// Resets the editor session.
    /// Clears the current module/workflow and resets all state.
    /// </summary>
    public void ResetSession()
    {
        lock (_lock)
        {
            _currentModule = null;
            _currentWorkflow = null;
            EditorSessionId = Guid.Empty;
            _isDirty = false;
            _lastModified = DateTime.UtcNow;
            _lastSaved = null;
        }

        // Fire state changed event
        OnStateChanged();
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================

    /// <summary>
    /// Raises the StateChanged event.
    /// </summary>
    protected virtual void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the ModuleChanged event.
    /// </summary>
    protected virtual void OnModuleChanged()
    {
        ModuleChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the WorkflowChanged event.
    /// </summary>
    protected virtual void OnWorkflowChanged()
    {
        WorkflowChanged?.Invoke(this, EventArgs.Empty);
    }
}
