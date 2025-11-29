namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Service for managing undo/redo functionality in the form editor.
/// Uses two stacks to track state changes and supports configurable history depth.
/// Thread-safe for concurrent access.
/// </summary>
public class UndoRedoService
{
    private readonly object _lock = new object();
    private readonly Stack<EditorSnapshot> _undoStack;
    private readonly Stack<EditorSnapshot> _redoStack;
    private int _nextSequenceNumber;

    // ========================================================================
    // CONFIGURATION
    // ========================================================================

    /// <summary>
    /// Gets or sets the maximum number of actions to retain in the undo stack.
    /// When this limit is exceeded, the oldest action is removed.
    /// Default is 100.
    /// </summary>
    public int MaxActions { get; set; } = 100;

    // ========================================================================
    // EVENTS
    // ========================================================================

    /// <summary>
    /// Event fired when the undo or redo stacks are modified.
    /// </summary>
    public event EventHandler? StackChanged;

    // ========================================================================
    // PROPERTIES
    // ========================================================================

    /// <summary>
    /// Gets a value indicating whether the undo operation is available.
    /// </summary>
    public bool CanUndo
    {
        get
        {
            lock (_lock)
            {
                return _undoStack.Count > 0;
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the redo operation is available.
    /// </summary>
    public bool CanRedo
    {
        get
        {
            lock (_lock)
            {
                return _redoStack.Count > 0;
            }
        }
    }

    /// <summary>
    /// Gets the number of actions available for undo.
    /// </summary>
    public int UndoCount
    {
        get
        {
            lock (_lock)
            {
                return _undoStack.Count;
            }
        }
    }

    /// <summary>
    /// Gets the number of actions available for redo.
    /// </summary>
    public int RedoCount
    {
        get
        {
            lock (_lock)
            {
                return _redoStack.Count;
            }
        }
    }

    // ========================================================================
    // CONSTRUCTOR
    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the UndoRedoService class.
    /// </summary>
    public UndoRedoService()
    {
        _undoStack = new Stack<EditorSnapshot>();
        _redoStack = new Stack<EditorSnapshot>();
        _nextSequenceNumber = 1;
    }

    /// <summary>
    /// Initializes a new instance of the UndoRedoService class with a custom max actions limit.
    /// </summary>
    /// <param name="maxActions">The maximum number of actions to retain</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxActions is less than 1</exception>
    public UndoRedoService(int maxActions) : this()
    {
        if (maxActions < 1)
            throw new ArgumentOutOfRangeException(nameof(maxActions), "MaxActions must be at least 1");

        MaxActions = maxActions;
    }

    // ========================================================================
    // PUBLIC METHODS
    // ========================================================================

    /// <summary>
    /// Pushes a new snapshot onto the undo stack.
    /// Clears the redo stack and enforces the MaxActions limit.
    /// </summary>
    /// <param name="snapshot">The snapshot to push (must have SequenceNumber = 0, will be auto-assigned)</param>
    /// <param name="actionDescription">Description of the action (overrides snapshot.ActionDescription)</param>
    /// <exception cref="ArgumentNullException">Thrown when snapshot is null</exception>
    public void PushSnapshot(EditorSnapshot snapshot, string actionDescription)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        lock (_lock)
        {
            // Assign sequence number
            var numberedSnapshot = snapshot with
            {
                SequenceNumber = _nextSequenceNumber++,
                ActionDescription = actionDescription
            };

            // Push to undo stack
            _undoStack.Push(numberedSnapshot);

            // Clear redo stack (standard undo/redo behavior)
            _redoStack.Clear();

            // Enforce MaxActions limit
            if (_undoStack.Count > MaxActions)
            {
                TrimUndoStack();
            }
        }

        // Fire event outside of lock
        OnStackChanged();
    }

    /// <summary>
    /// Undoes the last action by popping from the undo stack and pushing to the redo stack.
    /// </summary>
    /// <returns>The snapshot to restore, or null if undo stack is empty</returns>
    public EditorSnapshot? Undo()
    {
        EditorSnapshot? snapshot = null;

        lock (_lock)
        {
            if (_undoStack.Count == 0)
                return null;

            // Pop from undo stack
            snapshot = _undoStack.Pop();

            // Push to redo stack
            _redoStack.Push(snapshot);
        }

        // Fire event outside of lock
        OnStackChanged();

        return snapshot;
    }

    /// <summary>
    /// Redoes the last undone action by popping from the redo stack and pushing to the undo stack.
    /// </summary>
    /// <returns>The snapshot to restore, or null if redo stack is empty</returns>
    public EditorSnapshot? Redo()
    {
        EditorSnapshot? snapshot = null;

        lock (_lock)
        {
            if (_redoStack.Count == 0)
                return null;

            // Pop from redo stack
            snapshot = _redoStack.Pop();

            // Push to undo stack
            _undoStack.Push(snapshot);
        }

        // Fire event outside of lock
        OnStackChanged();

        return snapshot;
    }

    /// <summary>
    /// Clears both undo and redo stacks.
    /// Resets the sequence number counter.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _undoStack.Clear();
            _redoStack.Clear();
            _nextSequenceNumber = 1;
        }

        // Fire event outside of lock
        OnStackChanged();
    }

    /// <summary>
    /// Gets the action descriptions from the undo stack, most recent first.
    /// Useful for displaying undo history in the UI.
    /// </summary>
    /// <returns>List of action descriptions</returns>
    public List<string> GetUndoActionHistory()
    {
        lock (_lock)
        {
            return _undoStack
                .Select(s => s.ActionDescription)
                .ToList();
        }
    }

    /// <summary>
    /// Gets the action descriptions from the redo stack, most recent first.
    /// Useful for displaying redo history in the UI.
    /// </summary>
    /// <returns>List of action descriptions</returns>
    public List<string> GetRedoActionHistory()
    {
        lock (_lock)
        {
            return _redoStack
                .Select(s => s.ActionDescription)
                .ToList();
        }
    }

    /// <summary>
    /// Gets detailed information about the undo stack for debugging.
    /// </summary>
    /// <returns>List of snapshot summaries</returns>
    public List<string> GetUndoStackDetails()
    {
        lock (_lock)
        {
            return _undoStack
                .Select(s => $"#{s.SequenceNumber}: {s.ActionDescription} ({s.Timestamp:HH:mm:ss})")
                .ToList();
        }
    }

    /// <summary>
    /// Gets detailed information about the redo stack for debugging.
    /// </summary>
    /// <returns>List of snapshot summaries</returns>
    public List<string> GetRedoStackDetails()
    {
        lock (_lock)
        {
            return _redoStack
                .Select(s => $"#{s.SequenceNumber}: {s.ActionDescription} ({s.Timestamp:HH:mm:ss})")
                .ToList();
        }
    }

    // ========================================================================
    // PRIVATE METHODS
    // ========================================================================

    /// <summary>
    /// Trims the undo stack to MaxActions by removing the oldest entries.
    /// Must be called within a lock.
    /// </summary>
    private void TrimUndoStack()
    {
        // Convert stack to list (reversed so oldest is first)
        var items = _undoStack.Reverse().ToList();

        // Remove oldest items until we're at MaxActions
        while (items.Count > MaxActions)
        {
            items.RemoveAt(0); // Remove oldest (first in list)
        }

        // Clear and rebuild stack
        _undoStack.Clear();
        foreach (var item in items.Reverse<EditorSnapshot>())
        {
            _undoStack.Push(item);
        }
    }

    /// <summary>
    /// Raises the StackChanged event.
    /// </summary>
    protected virtual void OnStackChanged()
    {
        StackChanged?.Invoke(this, EventArgs.Empty);
    }
}
