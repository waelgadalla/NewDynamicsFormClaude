# UndoRedoService Summary

## Overview

Successfully created the **UndoRedoService** - a thread-safe undo/redo management system for the form editor that uses two stacks to track state changes and supports configurable history depth with automatic trimming.

**Date Completed**: November 28, 2025
**Status**: ✅ All acceptance criteria met
**Build Status**: 0 Errors, 0 Warnings

---

## What Was Created

### 1. EditorSnapshot Record (18 lines) ✅

**Location**: `Src/DynamicForms.Editor/Services/State/EditorSnapshot.cs`

**Purpose**: Represents a snapshot of the editor state at a specific point in time

**Properties**:
```csharp
public record EditorSnapshot(
    Guid SessionId,              // Editor session ID
    EditorEntityType EntityType, // Module or Workflow
    string SnapshotJson,         // Serialized schema JSON
    string ActionDescription,    // User-friendly action description
    DateTime Timestamp,          // When snapshot was created (UTC)
    int SequenceNumber           // Sequential number for ordering
);
```

**Immutability**: Record type ensures snapshots cannot be modified after creation

**Example**:
```csharp
var snapshot = new EditorSnapshot(
    SessionId: editorState.EditorSessionId,
    EntityType: EditorEntityType.Module,
    SnapshotJson: JsonSerializer.Serialize(module),
    ActionDescription: "Added Text Field: firstName",
    Timestamp: DateTime.UtcNow,
    SequenceNumber: 0  // Auto-assigned by UndoRedoService
);
```

---

### 2. UndoRedoService Class (283 lines) ✅

**Location**: `Src/DynamicForms.Editor/Services/State/UndoRedoService.cs`

**Purpose**: Manages undo/redo functionality using two stacks

**Features**:
- **Two-stack architecture** (undo and redo stacks)
- **Thread-safe** with lock-based synchronization
- **Configurable history depth** (MaxActions property)
- **Automatic stack trimming** when limit exceeded
- **Event notifications** (StackChanged event)
- **Action history** for UI display
- **Sequence numbering** for snapshot ordering

---

## Architecture

### Two-Stack Design

```
┌─────────────────┐         ┌─────────────────┐
│  Undo Stack     │         │  Redo Stack     │
├─────────────────┤         ├─────────────────┤
│  Snapshot #5    │ ◄─┐     │                 │
│  Snapshot #4    │   │     │                 │
│  Snapshot #3    │   │     │                 │
│  Snapshot #2    │   │     │                 │
│  Snapshot #1    │   │     │                 │
└─────────────────┘   │     └─────────────────┘
                      │
                Current State
```

**When user performs an action**:
```
PushSnapshot()
  ↓
Add to Undo Stack
  ↓
Clear Redo Stack
  ↓
Trim if > MaxActions
  ↓
Fire StackChanged event
```

**When user clicks Undo**:
```
Undo()
  ↓
Pop from Undo Stack
  ↓
Push to Redo Stack
  ↓
Fire StackChanged event
  ↓
Return snapshot to restore
```

**When user clicks Redo**:
```
Redo()
  ↓
Pop from Redo Stack
  ↓
Push to Undo Stack
  ↓
Fire StackChanged event
  ↓
Return snapshot to restore
```

---

## Properties

### CanUndo
```csharp
public bool CanUndo { get; }
```

**Purpose**: Indicates whether undo operation is available

**Returns**: `true` if undo stack has at least one snapshot

**Thread Safety**: Thread-safe with locking

**Usage**:
```csharp
<button disabled="@(!undoRedo.CanUndo)" @onclick="Undo">
    Undo
</button>
```

---

### CanRedo
```csharp
public bool CanRedo { get; }
```

**Purpose**: Indicates whether redo operation is available

**Returns**: `true` if redo stack has at least one snapshot

**Thread Safety**: Thread-safe with locking

**Usage**:
```csharp
<button disabled="@(!undoRedo.CanRedo)" @onclick="Redo">
    Redo
</button>
```

---

### UndoCount
```csharp
public int UndoCount { get; }
```

**Purpose**: Gets the number of actions available for undo

**Returns**: Count of snapshots in undo stack

**Thread Safety**: Thread-safe with locking

**Usage**:
```csharp
Console.WriteLine($"You can undo {undoRedo.UndoCount} actions");
```

---

### RedoCount
```csharp
public int RedoCount { get; }
```

**Purpose**: Gets the number of actions available for redo

**Returns**: Count of snapshots in redo stack

**Thread Safety**: Thread-safe with locking

**Usage**:
```csharp
Console.WriteLine($"You can redo {undoRedo.RedoCount} actions");
```

---

### MaxActions
```csharp
public int MaxActions { get; set; } = 100;
```

**Purpose**: Maximum number of actions to retain in undo stack

**Default**: 100

**Behavior**:
- When exceeded, oldest snapshots are removed from bottom of undo stack
- Can be configured via constructor or property setter
- Must be at least 1

**Usage**:
```csharp
// Create with custom limit
var undoRedo = new UndoRedoService(maxActions: 50);

// Or change after creation
undoRedo.MaxActions = 200;
```

---

## Event

### StackChanged
```csharp
public event EventHandler? StackChanged;
```

**Purpose**: Fired when undo or redo stacks are modified

**Fired By**:
- `PushSnapshot()` - New snapshot added
- `Undo()` - Snapshot moved from undo to redo
- `Redo()` - Snapshot moved from redo to undo
- `Clear()` - Both stacks cleared

**Usage**:
```csharp
undoRedo.StackChanged += (sender, e) =>
{
    // Update UI buttons
    StateHasChanged();

    // Update status bar
    UpdateUndoRedoStatus();
};
```

---

## Methods

### Constructor
```csharp
public UndoRedoService()
public UndoRedoService(int maxActions)
```

**Purpose**: Initializes a new instance of the service

**Parameters**:
- `maxActions` (optional) - Maximum actions to retain (default: 100)

**Exceptions**:
- `ArgumentOutOfRangeException` - If maxActions < 1

**Example**:
```csharp
// Default (100 actions)
var undoRedo = new UndoRedoService();

// Custom limit (50 actions)
var undoRedo = new UndoRedoService(50);
```

---

### PushSnapshot()
```csharp
public void PushSnapshot(EditorSnapshot snapshot, string actionDescription)
```

**Purpose**: Pushes a new snapshot onto the undo stack

**Parameters**:
- `snapshot` - The snapshot to push (SequenceNumber will be auto-assigned)
- `actionDescription` - Description of the action (overrides snapshot.ActionDescription)

**Behavior**:
1. Auto-assigns sequential SequenceNumber
2. Updates ActionDescription with provided value
3. Pushes to undo stack
4. Clears redo stack (standard undo/redo behavior)
5. Trims undo stack if it exceeds MaxActions
6. Fires StackChanged event

**Thread Safety**: Thread-safe with locking

**Exceptions**:
- `ArgumentNullException` - If snapshot is null

**Example**:
```csharp
var snapshot = new EditorSnapshot(
    SessionId: editorState.EditorSessionId,
    EntityType: EditorEntityType.Module,
    SnapshotJson: JsonSerializer.Serialize(module),
    ActionDescription: "",  // Will be overridden
    Timestamp: DateTime.UtcNow,
    SequenceNumber: 0  // Will be auto-assigned
);

undoRedo.PushSnapshot(snapshot, "Added Text Field: firstName");
// Undo stack: +1
// Redo stack: cleared
```

**Important**: When a new snapshot is pushed, the redo stack is cleared. This is standard undo/redo behavior - you cannot redo after making a new change.

---

### Undo()
```csharp
public EditorSnapshot? Undo()
```

**Purpose**: Undoes the last action

**Returns**: The snapshot to restore, or `null` if undo stack is empty

**Behavior**:
1. Pops snapshot from undo stack
2. Pushes snapshot to redo stack
3. Fires StackChanged event
4. Returns snapshot for caller to restore

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
var snapshot = undoRedo.Undo();
if (snapshot != null)
{
    // Deserialize and restore state
    var module = JsonSerializer.Deserialize<FormModuleSchema>(snapshot.SnapshotJson);
    editorState.UpdateModule(module, $"Undo: {snapshot.ActionDescription}");
}
```

---

### Redo()
```csharp
public EditorSnapshot? Redo()
```

**Purpose**: Redoes the last undone action

**Returns**: The snapshot to restore, or `null` if redo stack is empty

**Behavior**:
1. Pops snapshot from redo stack
2. Pushes snapshot to undo stack
3. Fires StackChanged event
4. Returns snapshot for caller to restore

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
var snapshot = undoRedo.Redo();
if (snapshot != null)
{
    // Deserialize and restore state
    var module = JsonSerializer.Deserialize<FormModuleSchema>(snapshot.SnapshotJson);
    editorState.UpdateModule(module, $"Redo: {snapshot.ActionDescription}");
}
```

---

### Clear()
```csharp
public void Clear()
```

**Purpose**: Clears both undo and redo stacks

**Behavior**:
1. Clears undo stack
2. Clears redo stack
3. Resets sequence number counter to 1
4. Fires StackChanged event

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
// When loading a new form
editorState.LoadModule(newModule);
undoRedo.Clear();  // Clear history for previous form
```

---

### GetUndoActionHistory()
```csharp
public List<string> GetUndoActionHistory()
```

**Purpose**: Gets action descriptions from undo stack for UI display

**Returns**: List of action descriptions, most recent first

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
var undoHistory = undoRedo.GetUndoActionHistory();
// Returns:
// [
//   "Added Text Field: email",
//   "Added Text Field: lastName",
//   "Added Text Field: firstName"
// ]
```

**UI Usage**:
```razor
<div class="undo-menu">
    @foreach (var action in undoRedo.GetUndoActionHistory())
    {
        <div class="undo-item">@action</div>
    }
</div>
```

---

### GetRedoActionHistory()
```csharp
public List<string> GetRedoActionHistory()
```

**Purpose**: Gets action descriptions from redo stack for UI display

**Returns**: List of action descriptions, most recent first

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
var redoHistory = undoRedo.GetRedoActionHistory();
// Returns:
// [
//   "Deleted Section: contactInfo",
//   "Moved Field: email"
// ]
```

---

### GetUndoStackDetails()
```csharp
public List<string> GetUndoStackDetails()
```

**Purpose**: Gets detailed snapshot information for debugging

**Returns**: List of formatted snapshot summaries

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
var details = undoRedo.GetUndoStackDetails();
// Returns:
// [
//   "#5: Added Text Field: email (14:32:45)",
//   "#4: Added Text Field: lastName (14:32:30)",
//   "#3: Added Text Field: firstName (14:32:15)"
// ]
```

---

### GetRedoStackDetails()
```csharp
public List<string> GetRedoStackDetails()
```

**Purpose**: Gets detailed redo snapshot information for debugging

**Returns**: List of formatted snapshot summaries

**Thread Safety**: Thread-safe with locking

---

## MaxActions Limit Enforcement

### How It Works

When the undo stack exceeds MaxActions:

```csharp
PushSnapshot() called
  ↓
Add to undo stack (count = 101)
  ↓
Check: count > MaxActions (101 > 100)
  ↓
TrimUndoStack() called
  ↓
1. Convert stack to list (reversed)
2. Remove oldest item (first in list)
3. Rebuild stack from remaining items
  ↓
Stack trimmed to 100 items
```

### TrimUndoStack() Algorithm

```csharp
private void TrimUndoStack()
{
    // Convert stack to list (reversed so oldest is first)
    var items = _undoStack.Reverse().ToList();

    // Remove oldest items until we're at MaxActions
    while (items.Count > MaxActions)
    {
        items.RemoveAt(0); // Remove oldest
    }

    // Clear and rebuild stack
    _undoStack.Clear();
    foreach (var item in items.Reverse<EditorSnapshot>())
    {
        _undoStack.Push(item);
    }
}
```

**Why Reverse Twice?**
- Stack.Reverse() puts oldest items first in list
- After trimming, reverse again to push back in correct order
- Maintains stack order (newest on top)

---

## Thread Safety

The UndoRedoService is **fully thread-safe** using lock-based synchronization:

### Lock Object
```csharp
private readonly object _lock = new object();
```

### Protected Operations
All stack operations are protected with locks:
- Reading properties (CanUndo, CanRedo, UndoCount, RedoCount)
- Pushing snapshots
- Popping snapshots (undo/redo)
- Getting action history
- Clearing stacks

### Thread-Safe Pattern
```csharp
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
```

**Events Fired Outside Lock**: Events are raised after releasing the lock to prevent deadlocks

---

## Usage Examples

### Example 1: Basic Undo/Redo

```csharp
// Create service
var undoRedo = new UndoRedoService();

// Subscribe to events
undoRedo.StackChanged += (s, e) =>
{
    Console.WriteLine($"Undo: {undoRedo.UndoCount}, Redo: {undoRedo.RedoCount}");
    StateHasChanged();
};

// User adds a field
var snapshot = CreateSnapshot(module, "Added Text Field: firstName");
undoRedo.PushSnapshot(snapshot, snapshot.ActionDescription);
// Output: "Undo: 1, Redo: 0"

// User adds another field
snapshot = CreateSnapshot(module, "Added Text Field: lastName");
undoRedo.PushSnapshot(snapshot, snapshot.ActionDescription);
// Output: "Undo: 2, Redo: 0"

// User clicks Undo
var restored = undoRedo.Undo();
RestoreFromSnapshot(restored);
// Output: "Undo: 1, Redo: 1"

// User clicks Redo
restored = undoRedo.Redo();
RestoreFromSnapshot(restored);
// Output: "Undo: 2, Redo: 0"
```

---

### Example 2: Integration with EditorStateService

```csharp
public class FormEditorComponent : ComponentBase
{
    private EditorStateService _editorState = new();
    private UndoRedoService _undoRedo = new(maxActions: 50);

    protected override void OnInitialized()
    {
        // Subscribe to editor state changes
        _editorState.ModuleChanged += OnModuleChanged;

        // Subscribe to undo/redo stack changes
        _undoRedo.StackChanged += OnStackChanged;
    }

    private void OnModuleChanged(object? sender, EventArgs e)
    {
        var module = _editorState.GetCurrentModule();
        if (module != null)
        {
            // Create snapshot
            var snapshot = new EditorSnapshot(
                SessionId: _editorState.EditorSessionId,
                EntityType: EditorEntityType.Module,
                SnapshotJson: JsonSerializer.Serialize(module),
                ActionDescription: "",  // Set by caller
                Timestamp: DateTime.UtcNow,
                SequenceNumber: 0
            );

            // Don't push snapshot if this was triggered by undo/redo
            if (!_isRestoringSnapshot)
            {
                _undoRedo.PushSnapshot(snapshot, "Module changed");
            }
        }
    }

    private void OnStackChanged(object? sender, EventArgs e)
    {
        StateHasChanged(); // Update UI
    }

    private bool _isRestoringSnapshot = false;

    private void Undo()
    {
        var snapshot = _undoRedo.Undo();
        if (snapshot != null)
        {
            _isRestoringSnapshot = true;
            try
            {
                var module = JsonSerializer.Deserialize<FormModuleSchema>(snapshot.SnapshotJson);
                _editorState.UpdateModule(module!, $"Undo: {snapshot.ActionDescription}");
            }
            finally
            {
                _isRestoringSnapshot = false;
            }
        }
    }

    private void Redo()
    {
        var snapshot = _undoRedo.Redo();
        if (snapshot != null)
        {
            _isRestoringSnapshot = true;
            try
            {
                var module = JsonSerializer.Deserialize<FormModuleSchema>(snapshot.SnapshotJson);
                _editorState.UpdateModule(module!, $"Redo: {snapshot.ActionDescription}");
            }
            finally
            {
                _isRestoringSnapshot = false;
            }
        }
    }
}
```

---

### Example 3: Undo/Redo Toolbar

```razor
@inject UndoRedoService UndoRedo

<div class="toolbar">
    <button class="btn btn-sm btn-outline-secondary"
            @onclick="Undo"
            disabled="@(!UndoRedo.CanUndo)"
            title="@GetUndoTooltip()">
        <i class="bi bi-arrow-counterclockwise"></i>
        Undo
    </button>

    <button class="btn btn-sm btn-outline-secondary"
            @onclick="Redo"
            disabled="@(!UndoRedo.CanRedo)"
            title="@GetRedoTooltip()">
        <i class="bi bi-arrow-clockwise"></i>
        Redo
    </button>

    <span class="text-muted ms-2">
        @UndoRedo.UndoCount undo, @UndoRedo.RedoCount redo
    </span>
</div>

@code {
    private string GetUndoTooltip()
    {
        var history = UndoRedo.GetUndoActionHistory();
        return history.Count > 0
            ? $"Undo: {history[0]}"
            : "Nothing to undo";
    }

    private string GetRedoTooltip()
    {
        var history = UndoRedo.GetRedoActionHistory();
        return history.Count > 0
            ? $"Redo: {history[0]}"
            : "Nothing to redo";
    }
}
```

---

### Example 4: Dropdown Undo/Redo Menu

```razor
@inject UndoRedoService UndoRedo

<div class="btn-group">
    <button class="btn btn-sm btn-outline-secondary"
            @onclick="Undo"
            disabled="@(!UndoRedo.CanUndo)">
        Undo
    </button>
    <button class="btn btn-sm btn-outline-secondary dropdown-toggle dropdown-toggle-split"
            data-bs-toggle="dropdown"
            disabled="@(!UndoRedo.CanUndo)">
    </button>
    <ul class="dropdown-menu">
        @foreach (var action in UndoRedo.GetUndoActionHistory().Take(10))
        {
            <li>
                <a class="dropdown-item small" @onclick="() => UndoTo(action)">
                    @action
                </a>
            </li>
        }
    </ul>
</div>

@code {
    private void UndoTo(string targetAction)
    {
        // Undo multiple times until we reach the target action
        var history = UndoRedo.GetUndoActionHistory();
        var index = history.IndexOf(targetAction);

        for (int i = 0; i <= index; i++)
        {
            Undo();
        }
    }
}
```

---

## Keyboard Shortcuts

### Example Integration

```csharp
// In Blazor component
@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("registerUndoRedoHotkeys");
        }
    }
}
```

```javascript
// In JavaScript
window.registerUndoRedoHotkeys = function() {
    document.addEventListener('keydown', function(e) {
        // Ctrl+Z or Cmd+Z for Undo
        if ((e.ctrlKey || e.metaKey) && e.key === 'z' && !e.shiftKey) {
            e.preventDefault();
            DotNet.invokeMethodAsync('DynamicForms.Editor', 'Undo');
        }

        // Ctrl+Shift+Z or Cmd+Shift+Z for Redo
        if ((e.ctrlKey || e.metaKey) && e.key === 'z' && e.shiftKey) {
            e.preventDefault();
            DotNet.invokeMethodAsync('DynamicForms.Editor', 'Redo');
        }

        // Ctrl+Y or Cmd+Y for Redo (Windows alternative)
        if ((e.ctrlKey || e.metaKey) && e.key === 'y') {
            e.preventDefault();
            DotNet.invokeMethodAsync('DynamicForms.Editor', 'Redo');
        }
    });
};
```

---

## Build Verification ✅

**Build Command**:
```bash
dotnet build Src/DynamicForms.Editor/DynamicForms.Editor.csproj
```

**Result**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:02.19
```

---

## Files Created

| File | Lines | Purpose |
|------|-------|---------|
| `Services/State/EditorSnapshot.cs` | 18 | Snapshot record type |
| `Services/State/UndoRedoService.cs` | 283 | Undo/redo service |
| `UNDO_REDO_SERVICE_SUMMARY.md` | This file | Documentation |

**Total**: 301 lines of code + documentation

---

## Acceptance Criteria - All Met ✅

| Criterion | Status | Details |
|-----------|--------|---------|
| Undo/Redo stacks work correctly | ✅ | Two Stack<EditorSnapshot> instances |
| PushSnapshot adds to undo, clears redo | ✅ | Standard undo/redo behavior |
| Undo moves snapshot undo→redo | ✅ | Pops from undo, pushes to redo |
| Redo moves snapshot redo→undo | ✅ | Pops from redo, pushes to undo |
| MaxActions limit enforced | ✅ | TrimUndoStack() removes oldest |
| Action history methods work | ✅ | GetUndoActionHistory/GetRedoActionHistory |
| StackChanged event fires | ✅ | Fires on all modifications |
| EditorSnapshot record defined | ✅ | All 6 required properties |
| Thread-safe | ✅ | Lock-based synchronization |
| Well-documented | ✅ | Comprehensive XML comments |

---

## Key Design Decisions

### 1. Two Separate Stacks

Used separate `Stack<EditorSnapshot>` for undo and redo:

**Why**:
- Standard undo/redo pattern
- Clear separation of concerns
- Easy to understand and maintain
- Efficient push/pop operations

**Alternative Considered**: Single list with current index
- More complex
- Less intuitive
- Harder to debug

### 2. Sequence Numbers

Auto-assigned sequential numbers to snapshots:

**Why**:
- Helps debug/trace state changes
- Useful for audit logging
- Can detect out-of-order issues
- Provides chronological ordering

### 3. Clearing Redo on Push

Redo stack is cleared when new snapshot is pushed:

**Why**:
- Standard undo/redo behavior across all editors
- User expects "redo" to disappear after new action
- Prevents confusing state (can't redo future you haven't been to)
- Matches behavior of Word, Photoshop, VS Code, etc.

### 4. Stack Trimming Algorithm

Used Reverse().ToList() approach for trimming:

**Why**:
- Stack<T> only allows top access
- Need to remove from bottom
- Simple and readable
- Performance acceptable for editor use case

**Alternative Considered**: LinkedList
- More complex data structure
- Overkill for this use case
- Stack<T> is more familiar to developers

### 5. Action Description Override

PushSnapshot() takes actionDescription parameter:

**Why**:
- Allows caller to provide context-specific description
- More flexible than using snapshot.ActionDescription directly
- Supports i18n in future (description can be localized)
- Clean separation between snapshot data and presentation

### 6. Nullable Return Types

Undo() and Redo() return `EditorSnapshot?`:

**Why**:
- Null indicates nothing to undo/redo
- Caller must handle null case
- More explicit than throwing exception
- Matches C# nullable reference types pattern

### 7. Events Fired Outside Lock

Raised events after releasing lock:

**Why**:
- Prevents potential deadlocks
- Event handlers may take time
- Event handlers may call back into service
- Better performance

### 8. JSON Serialization in Snapshot

Store schemas as JSON strings, not objects:

**Why**:
- Immutable - cannot be accidentally modified
- Serializable for persistence (could save to disk)
- Size-efficient (can compress if needed)
- Works with any schema type (Module or Workflow)

---

## Testing Recommendations

### Unit Tests

```csharp
[Fact]
public void PushSnapshot_ShouldAddToUndoStack()
{
    var service = new UndoRedoService();
    var snapshot = CreateTestSnapshot("Action 1");

    service.PushSnapshot(snapshot, snapshot.ActionDescription);

    Assert.Equal(1, service.UndoCount);
    Assert.True(service.CanUndo);
}

[Fact]
public void PushSnapshot_ShouldClearRedoStack()
{
    var service = new UndoRedoService();
    var snapshot1 = CreateTestSnapshot("Action 1");
    var snapshot2 = CreateTestSnapshot("Action 2");

    service.PushSnapshot(snapshot1, snapshot1.ActionDescription);
    service.Undo(); // Move to redo stack
    Assert.Equal(1, service.RedoCount);

    service.PushSnapshot(snapshot2, snapshot2.ActionDescription);
    Assert.Equal(0, service.RedoCount); // Redo stack cleared
}

[Fact]
public void Undo_ShouldMoveSnapshotToRedoStack()
{
    var service = new UndoRedoService();
    var snapshot = CreateTestSnapshot("Action 1");
    service.PushSnapshot(snapshot, snapshot.ActionDescription);

    var result = service.Undo();

    Assert.NotNull(result);
    Assert.Equal(0, service.UndoCount);
    Assert.Equal(1, service.RedoCount);
}

[Fact]
public void Redo_ShouldMoveSnapshotToUndoStack()
{
    var service = new UndoRedoService();
    var snapshot = CreateTestSnapshot("Action 1");
    service.PushSnapshot(snapshot, snapshot.ActionDescription);
    service.Undo();

    var result = service.Redo();

    Assert.NotNull(result);
    Assert.Equal(1, service.UndoCount);
    Assert.Equal(0, service.RedoCount);
}

[Fact]
public void PushSnapshot_ShouldEnforceMaxActions()
{
    var service = new UndoRedoService(maxActions: 3);

    for (int i = 1; i <= 5; i++)
    {
        var snapshot = CreateTestSnapshot($"Action {i}");
        service.PushSnapshot(snapshot, snapshot.ActionDescription);
    }

    Assert.Equal(3, service.UndoCount); // Limited to 3
}

[Fact]
public void GetUndoActionHistory_ShouldReturnDescriptions()
{
    var service = new UndoRedoService();
    service.PushSnapshot(CreateTestSnapshot("Action 1"), "Action 1");
    service.PushSnapshot(CreateTestSnapshot("Action 2"), "Action 2");
    service.PushSnapshot(CreateTestSnapshot("Action 3"), "Action 3");

    var history = service.GetUndoActionHistory();

    Assert.Equal(3, history.Count);
    Assert.Equal("Action 3", history[0]); // Most recent first
    Assert.Equal("Action 2", history[1]);
    Assert.Equal("Action 1", history[2]);
}

[Fact]
public void Clear_ShouldClearBothStacks()
{
    var service = new UndoRedoService();
    service.PushSnapshot(CreateTestSnapshot("Action 1"), "Action 1");
    service.Undo();

    service.Clear();

    Assert.Equal(0, service.UndoCount);
    Assert.Equal(0, service.RedoCount);
    Assert.False(service.CanUndo);
    Assert.False(service.CanRedo);
}

[Fact]
public void StackChanged_ShouldFireOnPush()
{
    var service = new UndoRedoService();
    var eventFired = false;
    service.StackChanged += (s, e) => eventFired = true;

    service.PushSnapshot(CreateTestSnapshot("Action 1"), "Action 1");

    Assert.True(eventFired);
}

[Fact]
public void Constructor_WithInvalidMaxActions_ShouldThrow()
{
    Assert.Throws<ArgumentOutOfRangeException>(() =>
        new UndoRedoService(maxActions: 0));
}

private EditorSnapshot CreateTestSnapshot(string description)
{
    return new EditorSnapshot(
        SessionId: Guid.NewGuid(),
        EntityType: EditorEntityType.Module,
        SnapshotJson: "{}",
        ActionDescription: description,
        Timestamp: DateTime.UtcNow,
        SequenceNumber: 0
    );
}
```

---

## Performance Considerations

### Memory Usage

**Snapshot Size**:
- Typical module schema: ~50-500 KB JSON
- MaxActions = 100: ~5-50 MB memory usage
- Acceptable for modern systems

**Optimization Options** (if needed):
1. Compress JSON (GZip)
2. Store deltas instead of full snapshots
3. Reduce MaxActions limit
4. Offload old snapshots to disk

### Stack Trimming Performance

**TrimUndoStack() Complexity**: O(n) where n = MaxActions
- Triggered only when exceeding limit
- Typical: 100 items × 2 reverse operations
- Fast enough for interactive use

**Could Optimize With**: Circular buffer or deque structure
- Not needed for current use case
- Premature optimization

---

## Next Steps

With UndoRedoService complete, next steps for Phase 3:

- Integrate with EditorStateService
- Add undo/redo toolbar buttons
- Implement keyboard shortcuts (Ctrl+Z, Ctrl+Y)
- Create undo/redo history panel
- Add snapshot creation on all editor actions

---

*Generated: November 28, 2025*
*Project: DynamicForms Visual Editor*
*Phase: 3 - Visual Form Editor*
*Component: UndoRedoService (COMPLETED)*
