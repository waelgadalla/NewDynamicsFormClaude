using DynamicForms.Core.V2.Schemas;

namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Service for managing the current workflow editing state.
/// Tracks the currently loaded workflow, dirty state, and modification history.
/// Thread-safe for concurrent access.
/// </summary>
public class WorkflowStateService
{
    private readonly object _lock = new object();
    private FormWorkflowSchema? _currentWorkflow;
    private bool _isDirty;
    private DateTime _lastModified;
    private DateTime? _lastSaved;

    // ========================================================================
    // EVENTS
    // ========================================================================

    /// <summary>
    /// Event fired when any state change occurs.
    /// </summary>
    public event EventHandler? StateChanged;

    /// <summary>
    /// Event fired when the current workflow is loaded or updated.
    /// </summary>
    public event EventHandler? WorkflowChanged;

    // ========================================================================
    // PROPERTIES
    // ========================================================================

    /// <summary>
    /// Gets the unique identifier for the current editor session.
    /// A new GUID is generated each time a workflow is loaded.
    /// </summary>
    public Guid EditorSessionId { get; private set; }

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
    /// Gets a value indicating whether the current workflow has unsaved changes.
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
    /// Gets the timestamp of the last modification.
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
    /// Gets the timestamp of the last save, or null if never saved.
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
    // METHODS
    // ========================================================================

    /// <summary>
    /// Loads a workflow into the editor state.
    /// This resets the dirty flag and creates a new editor session.
    /// </summary>
    public void LoadWorkflow(FormWorkflowSchema workflow)
    {
        lock (_lock)
        {
            CurrentWorkflow = workflow;
            IsDirty = false;
            LastModified = DateTime.UtcNow;
            EditorSessionId = Guid.NewGuid();
        }

        OnWorkflowChanged();
        OnStateChanged();
    }

    /// <summary>
    /// Updates the current workflow with a new version.
    /// This marks the state as dirty and updates the last modified timestamp.
    /// </summary>
    public void UpdateWorkflow(FormWorkflowSchema workflow, string? description = null)
    {
        lock (_lock)
        {
            CurrentWorkflow = workflow;
            IsDirty = true;
            LastModified = DateTime.UtcNow;
        }

        OnWorkflowChanged();
        OnStateChanged();
    }

    /// <summary>
    /// Marks the current state as saved.
    /// This clears the dirty flag and updates the last saved timestamp.
    /// </summary>
    public void MarkAsSaved()
    {
        lock (_lock)
        {
            IsDirty = false;
            LastSaved = DateTime.UtcNow;
        }

        OnStateChanged();
    }

    /// <summary>
    /// Clears the current workflow and resets the state.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            CurrentWorkflow = null;
            IsDirty = false;
            LastModified = DateTime.UtcNow;
            LastSaved = null;
            EditorSessionId = Guid.Empty;
        }

        OnStateChanged();
    }

    // ========================================================================
    // EVENT RAISING
    // ========================================================================

    protected virtual void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnWorkflowChanged()
    {
        WorkflowChanged?.Invoke(this, EventArgs.Empty);
    }
}
