# AutoSaveService Summary

## Overview

Successfully created the **AutoSaveService** - a thread-safe automatic background saving service for the form editor that uses a timer for periodic execution and supports both automatic and manual save triggers with comprehensive error handling and event notifications.

**Date Completed**: November 28, 2025
**Status**: ✅ All acceptance criteria met
**Build Status**: 0 Errors, 0 Warnings

---

## What Was Created

### 1. AutoSaveEventArgs Classes (105 lines) ✅

**Location**: `Src/DynamicForms.Editor/Services/State/AutoSaveEventArgs.cs`

**Purpose**: Event arguments for auto-save completion and error events

**Classes**:

#### AutoSaveEventArgs
```csharp
public class AutoSaveEventArgs : EventArgs
{
    public Guid SessionId { get; }
    public EditorEntityType EntityType { get; }
    public int EntityId { get; }
    public DateTime SavedAt { get; }
    public long DataSize { get; }
    public bool IsManualSave { get; }
}
```

**Properties**:
- `SessionId` - Editor session ID when save occurred
- `EntityType` - Type of entity saved (Module or Workflow)
- `EntityId` - Database ID of saved entity
- `SavedAt` - Timestamp when save completed (UTC)
- `DataSize` - Size of saved data in bytes
- `IsManualSave` - Whether this was a manual save vs automatic

**Usage**:
```csharp
autoSave.AutoSaveCompleted += (sender, args) =>
{
    Console.WriteLine($"Saved {args.EntityType} ID={args.EntityId}, Size={args.DataSize} bytes");
    UpdateStatusBar($"Auto-saved at {args.SavedAt:HH:mm:ss}");
};
```

#### AutoSaveErrorEventArgs
```csharp
public class AutoSaveErrorEventArgs : EventArgs
{
    public Guid SessionId { get; }
    public EditorEntityType EntityType { get; }
    public Exception Exception { get; }
    public DateTime ErrorAt { get; }
    public bool IsManualSave { get; }
    public int FailureCount { get; }
}
```

**Properties**:
- `SessionId` - Editor session ID when error occurred
- `EntityType` - Type of entity that failed to save
- `Exception` - Exception that was thrown
- `ErrorAt` - Timestamp when error occurred (UTC)
- `IsManualSave` - Whether this was a manual save attempt
- `FailureCount` - Number of consecutive save failures (for retry logic)

**Usage**:
```csharp
autoSave.AutoSaveError += (sender, args) =>
{
    _logger.LogError(args.Exception, "Auto-save failed (attempt {Count})", args.FailureCount);
    ShowErrorNotification($"Auto-save failed: {args.Exception.Message}");
};
```

---

### 2. AutoSaveService Class (439 lines) ✅

**Location**: `Src/DynamicForms.Editor/Services/State/AutoSaveService.cs`

**Purpose**: Automatic background saving of editor state to database

**Features**:
- **Timer-based auto-save** with configurable interval
- **Manual save trigger** via SaveNowAsync()
- **IsDirty check** - only saves when changes exist
- **Database persistence** to EditorFormModules table
- **Event notifications** for success and errors
- **Thread-safe** with lock-based synchronization
- **IDisposable** for proper cleanup
- **Consecutive failure tracking** for retry logic
- **Configuration from database** for interval setting

---

## Architecture

### Service Dependencies

```
AutoSaveService
├── EditorStateService (required)
│   └── Provides current module/workflow and dirty state
├── IEditorModuleRepository (required)
│   └── Saves modules to database
├── IEditorConfigurationRepository (required)
│   └── Loads auto-save interval setting
└── ILogger<AutoSaveService> (required)
    └── Logs auto-save operations and errors
```

### Timer Workflow

```
Start() called
  ↓
Load interval from EditorConfiguration table
  ↓
Create Timer with interval (default: 30s)
  ↓
Timer callback fires periodically
  ↓
PerformAutoSaveAsync() called
  ↓
Check if IsSaving → Skip if already saving
  ↓
Check if IsDirty → Skip if no changes
  ↓
Get current module/workflow from EditorStateService
  ↓
Serialize to JSON
  ↓
Save to database (create or update)
  ↓
Call EditorStateService.MarkAsSaved()
  ↓
Fire AutoSaveCompleted event
  ↓
OR on error → Fire AutoSaveError event
```

### Save Logic Flow

```
PerformAutoSaveAsync(isManual)
  ↓
Lock check: IsSaving?
  │
  ├─ Yes → Return false (skip)
  │
  └─ No → Set IsSaving = true
       ↓
  Get EditorState (SessionId, EntityType, IsDirty)
       ↓
  Check IsDirty (unless manual)
  │
  ├─ Not dirty → Return false (skip)
  │
  └─ Is dirty → Continue
       ↓
  Get current module/workflow
  │
  ├─ Module → SaveModuleAsync()
  │   ↓
  │   Serialize FormModuleSchema to JSON
  │   ↓
  │   Check if module exists in DB
  │   │
  │   ├─ Exists → Update existing EditorFormModule
  │   │
  │   └─ Not exists → Create new EditorFormModule
  │       ↓
  │   Return (EntityId, DataSize)
  │
  └─ Workflow → TODO (not yet implemented)
       ↓
  Call EditorStateService.MarkAsSaved()
       ↓
  Update LastAutoSave timestamp
       ↓
  Reset _consecutiveFailures = 0
       ↓
  Fire AutoSaveCompleted event
       ↓
  Return true
```

### Error Handling

```
Exception thrown during save
  ↓
Increment _consecutiveFailures
  ↓
Log error with failure count
  ↓
Fire AutoSaveError event
  ↓
Return false (save failed)
```

---

## Properties

### IsEnabled
```csharp
public bool IsEnabled { get; }
```

**Purpose**: Indicates whether auto-save timer is running

**Returns**: `true` if timer is active; `false` if stopped

**Thread Safety**: Thread-safe with locking

**Usage**:
```csharp
if (autoSave.IsEnabled)
{
    Console.WriteLine("Auto-save is running");
}
else
{
    Console.WriteLine("Auto-save is stopped");
}
```

---

### LastAutoSave
```csharp
public DateTime? LastAutoSave { get; private set; }
```

**Purpose**: Gets timestamp of last successful auto-save (UTC)

**Returns**: DateTime if save occurred; null if no save yet

**Usage**:
```csharp
if (autoSave.LastAutoSave.HasValue)
{
    var elapsed = DateTime.UtcNow - autoSave.LastAutoSave.Value;
    Console.WriteLine($"Last saved {elapsed.TotalSeconds:F0} seconds ago");
}
```

---

### IsSaving
```csharp
public bool IsSaving { get; private set; }
```

**Purpose**: Indicates whether save operation is in progress

**Returns**: `true` if currently saving; `false` otherwise

**Thread Safety**: Thread-safe with locking

**Usage**:
```csharp
<button disabled="@autoSave.IsSaving">
    @(autoSave.IsSaving ? "Saving..." : "Save")
</button>
```

---

### IntervalSeconds
```csharp
public int IntervalSeconds { get; private set; } = 30;
```

**Purpose**: Gets configured auto-save interval in seconds

**Default**: 30 seconds

**Loaded From**: EditorConfiguration table, key: "AutoSave.IntervalSeconds"

**Minimum**: 5 seconds (enforced in StartAsync)

**Usage**:
```csharp
Console.WriteLine($"Auto-save interval: {autoSave.IntervalSeconds}s");
```

---

## Events

### AutoSaveCompleted
```csharp
public event EventHandler<AutoSaveEventArgs>? AutoSaveCompleted;
```

**Purpose**: Fired when auto-save completes successfully

**Event Args**:
- `SessionId` - Editor session ID
- `EntityType` - Module or Workflow
- `EntityId` - Database ID of saved entity
- `SavedAt` - When save completed (UTC)
- `DataSize` - Size in bytes
- `IsManualSave` - Manual vs automatic save

**Usage**:
```csharp
autoSave.AutoSaveCompleted += (sender, args) =>
{
    var saveType = args.IsManualSave ? "Manual" : "Auto";
    var sizeKb = args.DataSize / 1024.0;

    _logger.LogInformation(
        "{Type} save completed: {EntityType} ID={Id}, Size={Size:F1}KB",
        saveType, args.EntityType, args.EntityId, sizeKb);

    // Update UI
    ShowToast($"Saved successfully at {args.SavedAt:HH:mm:ss}");
    UpdateSaveIndicator(saved: true);
};
```

---

### AutoSaveError
```csharp
public event EventHandler<AutoSaveErrorEventArgs>? AutoSaveError;
```

**Purpose**: Fired when auto-save encounters an error

**Event Args**:
- `SessionId` - Editor session ID
- `EntityType` - Module or Workflow
- `Exception` - The exception thrown
- `ErrorAt` - When error occurred (UTC)
- `IsManualSave` - Manual vs automatic save
- `FailureCount` - Consecutive failures

**Usage**:
```csharp
autoSave.AutoSaveError += (sender, args) =>
{
    var saveType = args.IsManualSave ? "Manual" : "Auto";

    _logger.LogError(
        args.Exception,
        "{Type} save failed (attempt {Count}): {EntityType}",
        saveType, args.FailureCount, args.EntityType);

    // Update UI
    ShowErrorToast($"Save failed: {args.Exception.Message}");
    UpdateSaveIndicator(saved: false);

    // Retry logic
    if (args.FailureCount >= 3)
    {
        ShowWarning("Auto-save has failed 3 times. Please save manually.");
    }
};
```

---

## Methods

### Constructor
```csharp
public AutoSaveService(
    EditorStateService editorState,
    IEditorModuleRepository moduleRepository,
    IEditorConfigurationRepository configRepository,
    ILogger<AutoSaveService> logger)
```

**Purpose**: Initializes auto-save service with dependencies

**Parameters**:
- `editorState` - Editor state service (required)
- `moduleRepository` - Module repository (required)
- `configRepository` - Configuration repository (required)
- `logger` - Logger instance (required)

**Exceptions**:
- `ArgumentNullException` - If any parameter is null

**Example**:
```csharp
var autoSave = new AutoSaveService(
    editorState: _editorState,
    moduleRepository: _moduleRepo,
    configRepository: _configRepo,
    logger: _logger);
```

---

### StartAsync()
```csharp
public async Task StartAsync(CancellationToken cancellationToken = default)
```

**Purpose**: Starts the auto-save timer

**Behavior**:
1. Loads interval from configuration (key: "AutoSave.IntervalSeconds")
2. Uses default 30s if not configured
3. Enforces minimum interval of 5s
4. Creates Timer with loaded interval
5. Logs startup

**Thread Safety**: Thread-safe with locking

**Exceptions**:
- `ObjectDisposedException` - If service already disposed

**Example**:
```csharp
// Start auto-save on component initialization
protected override async Task OnInitializedAsync()
{
    await _autoSave.StartAsync();
    _logger.LogInformation("Auto-save started");
}
```

**Configuration Setup**:
```csharp
// Set auto-save interval in configuration (one-time setup)
await _configRepo.SetIntValueAsync(
    "AutoSave.IntervalSeconds",
    60,  // 60 seconds
    "Auto-save interval for form editor");
```

---

### Stop()
```csharp
public void Stop()
```

**Purpose**: Stops the auto-save timer

**Behavior**:
1. Disposes timer
2. Any in-progress save completes
3. No new saves will start
4. Logs shutdown

**Thread Safety**: Thread-safe with locking

**Example**:
```csharp
// Stop auto-save on component disposal
public void Dispose()
{
    _autoSave.Stop();
    _logger.LogInformation("Auto-save stopped");
}
```

---

### SaveNowAsync()
```csharp
public async Task<bool> SaveNowAsync(CancellationToken cancellationToken = default)
```

**Purpose**: Triggers an immediate manual save

**Behavior**:
1. Bypasses IsDirty check (saves even if not dirty)
2. Runs immediately (doesn't wait for timer)
3. Calls PerformAutoSaveAsync(isManual: true)
4. Fires AutoSaveCompleted event
5. Returns success/failure status

**Returns**: `true` if save successful; `false` otherwise

**Thread Safety**: Thread-safe with locking

**Exceptions**:
- `ObjectDisposedException` - If service already disposed

**Example**:
```csharp
// Manual save button click
private async Task OnSaveClick()
{
    var success = await _autoSave.SaveNowAsync();

    if (success)
    {
        ShowToast("Saved successfully");
    }
    else
    {
        ShowError("Save failed - check logs for details");
    }
}
```

---

### PerformAutoSaveAsync() (Private)
```csharp
private async Task<bool> PerformAutoSaveAsync(
    bool isManual = false,
    CancellationToken cancellationToken = default)
```

**Purpose**: Performs actual auto-save operation

**Behavior**:
1. Checks if already saving (skip if yes)
2. Sets IsSaving = true
3. Gets current editor state
4. Checks IsDirty (unless manual save)
5. Gets current module/workflow
6. Calls SaveModuleAsync() or SaveWorkflowAsync()
7. Marks state as saved
8. Updates LastAutoSave
9. Resets failure counter
10. Fires AutoSaveCompleted event
11. On error: increments failure counter, fires AutoSaveError event

**Parameters**:
- `isManual` - Whether this is a manual save (bypasses IsDirty check)
- `cancellationToken` - Cancellation token

**Returns**: `true` if save successful; `false` otherwise

**Thread Safety**: Thread-safe with locking

---

### SaveModuleAsync() (Private)
```csharp
private async Task<(int EntityId, long DataSize)> SaveModuleAsync(
    FormModuleSchema module,
    CancellationToken cancellationToken)
```

**Purpose**: Saves a module to the database

**Behavior**:
1. Serializes FormModuleSchema to JSON
2. Calculates data size in bytes
3. Checks if module exists (by ModuleId)
4. If exists: Updates existing EditorFormModule
5. If not exists: Creates new EditorFormModule
6. Returns (EntityId, DataSize) tuple

**Parameters**:
- `module` - Module schema to save
- `cancellationToken` - Cancellation token

**Returns**: Tuple of (database ID, size in bytes)

**Database Mapping**:
```csharp
EditorFormModule:
  ModuleId = FormModuleSchema.Id
  Title = FormModuleSchema.TitleEn
  TitleFr = FormModuleSchema.TitleFr
  Description = FormModuleSchema.DescriptionEn
  DescriptionFr = FormModuleSchema.DescriptionFr
  SchemaJson = JSON.Serialize(FormModuleSchema)
  ModifiedAt = DateTime.UtcNow
  ModifiedBy = "AutoSave"
```

---

### Dispose()
```csharp
public void Dispose()
```

**Purpose**: Disposes auto-save service and cleans up resources

**Behavior**:
1. Unsubscribes from EditorStateService.StateChanged event
2. Disposes timer
3. Sets _disposed = true
4. Logs disposal

**Implements**: IDisposable

**Example**:
```csharp
// Component disposal
@implements IDisposable

public void Dispose()
{
    _autoSave?.Dispose();
}
```

---

## Configuration

### Database Configuration Table

Auto-save interval is stored in the `EditorConfiguration` table:

```sql
-- Default auto-save configuration
INSERT INTO EditorConfiguration (ConfigKey, ConfigValue, ConfigType, Description, CreatedAt, ModifiedAt)
VALUES (
    'AutoSave.IntervalSeconds',
    '30',
    'Int',
    'Auto-save interval for form editor in seconds (minimum 5)',
    GETUTCDATE(),
    GETUTCDATE()
);
```

### Configuration Values

| Key | Type | Default | Min | Description |
|-----|------|---------|-----|-------------|
| `AutoSave.IntervalSeconds` | Int | 30 | 5 | Auto-save interval in seconds |

### Changing Configuration

```csharp
// Using repository
await _configRepo.SetIntValueAsync(
    "AutoSave.IntervalSeconds",
    60,  // 1 minute
    "Auto-save interval for form editor");

// Restart auto-save to pick up new interval
_autoSave.Stop();
await _autoSave.StartAsync();
```

---

## Usage Examples

### Example 1: Basic Integration

```csharp
@inject AutoSaveService AutoSave
@inject EditorStateService EditorState
@implements IDisposable

protected override async Task OnInitializedAsync()
{
    // Subscribe to events
    AutoSave.AutoSaveCompleted += OnAutoSaveCompleted;
    AutoSave.AutoSaveError += OnAutoSaveError;

    // Start auto-save
    await AutoSave.StartAsync();
}

private void OnAutoSaveCompleted(object? sender, AutoSaveEventArgs args)
{
    StateHasChanged(); // Update UI
}

private void OnAutoSaveError(object? sender, AutoSaveErrorEventArgs args)
{
    StateHasChanged(); // Update UI
}

public void Dispose()
{
    AutoSave.AutoSaveCompleted -= OnAutoSaveCompleted;
    AutoSave.AutoSaveError -= OnAutoSaveError;
    AutoSave.Stop();
}
```

---

### Example 2: Save Status Indicator

```razor
<div class="save-status">
    @if (AutoSave.IsSaving)
    {
        <span class="text-warning">
            <i class="bi bi-hourglass-split"></i>
            Saving...
        </span>
    }
    else if (AutoSave.LastAutoSave.HasValue)
    {
        var elapsed = DateTime.UtcNow - AutoSave.LastAutoSave.Value;
        <span class="text-success">
            <i class="bi bi-check-circle"></i>
            Saved @FormatElapsed(elapsed) ago
        </span>
    }
    else
    {
        <span class="text-muted">
            <i class="bi bi-circle"></i>
            Not saved
        </span>
    }
</div>

@code {
    private string FormatElapsed(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 60)
            return $"{elapsed.TotalSeconds:F0}s";
        if (elapsed.TotalMinutes < 60)
            return $"{elapsed.TotalMinutes:F0}m";
        return $"{elapsed.TotalHours:F0}h";
    }
}
```

---

### Example 3: Manual Save Button

```razor
<button class="btn btn-primary"
        @onclick="SaveManually"
        disabled="@AutoSave.IsSaving">
    <i class="bi bi-floppy"></i>
    @(AutoSave.IsSaving ? "Saving..." : "Save Now")
</button>

@code {
    private async Task SaveManually()
    {
        var success = await AutoSave.SaveNowAsync();

        if (success)
        {
            await JS.InvokeVoidAsync("showToast", "success", "Saved successfully");
        }
        else
        {
            await JS.InvokeVoidAsync("showToast", "error", "Save failed");
        }
    }
}
```

---

### Example 4: Auto-Save Settings Panel

```razor
<div class="settings-panel">
    <h3>Auto-Save Settings</h3>

    <div class="form-group">
        <label>Auto-Save Interval (seconds)</label>
        <input type="number"
               class="form-control"
               min="5"
               max="300"
               @bind="intervalSeconds" />
        <small class="form-text text-muted">
            Minimum: 5 seconds, Maximum: 5 minutes
        </small>
    </div>

    <div class="form-check">
        <input type="checkbox"
               class="form-check-input"
               id="enableAutoSave"
               checked="@AutoSave.IsEnabled"
               @onchange="ToggleAutoSave" />
        <label class="form-check-label" for="enableAutoSave">
            Enable auto-save
        </label>
    </div>

    <button class="btn btn-primary" @onclick="ApplySettings">
        Apply Settings
    </button>
</div>

@code {
    private int intervalSeconds = 30;

    protected override void OnInitialized()
    {
        intervalSeconds = AutoSave.IntervalSeconds;
    }

    private async Task ToggleAutoSave(ChangeEventArgs e)
    {
        var enabled = (bool)e.Value!;

        if (enabled)
        {
            await AutoSave.StartAsync();
        }
        else
        {
            AutoSave.Stop();
        }

        StateHasChanged();
    }

    private async Task ApplySettings()
    {
        // Save to configuration
        await _configRepo.SetIntValueAsync(
            "AutoSave.IntervalSeconds",
            intervalSeconds,
            "Auto-save interval for form editor");

        // Restart with new settings
        if (AutoSave.IsEnabled)
        {
            AutoSave.Stop();
            await AutoSave.StartAsync();
        }

        await JS.InvokeVoidAsync("showToast", "success", "Settings updated");
    }
}
```

---

### Example 5: Error Notification with Retry

```razor
@inject IJSRuntime JS

protected override void OnInitialized()
{
    AutoSave.AutoSaveError += OnAutoSaveError;
}

private async void OnAutoSaveError(object? sender, AutoSaveErrorEventArgs args)
{
    // Log error
    _logger.LogError(
        args.Exception,
        "Auto-save failed (attempt {Count})",
        args.FailureCount);

    // Show notification based on failure count
    if (args.FailureCount == 1)
    {
        // First failure - just log
        await JS.InvokeVoidAsync(
            "showToast",
            "warning",
            "Auto-save failed. Will retry automatically.");
    }
    else if (args.FailureCount == 3)
    {
        // Third failure - warn user
        await JS.InvokeVoidAsync(
            "showNotification",
            "error",
            "Auto-save has failed 3 times",
            "Please save manually or check your connection.");
    }
    else if (args.FailureCount >= 5)
    {
        // Fifth failure - stop auto-save
        AutoSave.Stop();
        await JS.InvokeVoidAsync(
            "showDialog",
            "error",
            "Auto-save disabled",
            "Auto-save has failed 5 times and has been disabled. Please save manually.");
    }

    StateHasChanged();
}
```

---

### Example 6: Service Registration (Dependency Injection)

```csharp
// Program.cs or Startup.cs
builder.Services.AddScoped<EditorStateService>();
builder.Services.AddScoped<AutoSaveService>();
builder.Services.AddScoped<IEditorModuleRepository, EditorModuleRepository>();
builder.Services.AddScoped<IEditorConfigurationRepository, EditorConfigurationRepository>();

// With DbContext
builder.Services.AddDbContext<EditorDbContext>(options =>
    options.UseSqlServer(connectionString));
```

---

## Thread Safety

The AutoSaveService is **fully thread-safe** using lock-based synchronization:

### Lock Object
```csharp
private readonly object _lock = new object();
```

### Protected Operations
All critical operations are protected with locks:
- Checking/setting IsSaving flag
- Checking IsEnabled property
- Creating/disposing timer
- Reading/writing state

### Thread-Safe Pattern
```csharp
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
```

### Event Safety
Events are raised outside of locks to prevent deadlocks:
```csharp
// Inside lock - do work
lock (_lock)
{
    IsSaving = true;
}

// Outside lock - fire events
OnAutoSaveCompleted(eventArgs);
```

---

## Error Handling

### Consecutive Failure Tracking

```csharp
private int _consecutiveFailures;
```

**Purpose**: Tracks number of consecutive save failures

**Behavior**:
- Incremented on each save error
- Reset to 0 on successful save
- Reset to 0 on manual save (via EditorStateService.StateChanged)
- Included in AutoSaveErrorEventArgs for retry logic

**Usage in Event Handler**:
```csharp
AutoSave.AutoSaveError += (sender, args) =>
{
    if (args.FailureCount >= 5)
    {
        // Stop auto-save after 5 failures
        AutoSave.Stop();
        ShowCriticalError("Auto-save disabled after repeated failures");
    }
};
```

### Error Recovery

```csharp
// Manual save resets failure counter
EditorStateService.StateChanged += (sender, e) =>
{
    if (!EditorState.IsDirty && _consecutiveFailures > 0)
    {
        _consecutiveFailures = 0;
        _logger.LogDebug("Failure counter reset after manual save");
    }
};
```

---

## Performance Considerations

### Memory Usage

**Timer**: ~100 bytes (minimal overhead)

**JSON Serialization**:
- Typical module: 50-500 KB JSON
- Serialized on each save
- Not cached (ensures latest state)

**Event Handlers**:
- Weak references not needed (component lifecycle managed)
- Unsubscribe on Dispose()

### Database Impact

**Write Frequency**: Configurable (default 30s)

**Write Size**: 50-500 KB per save (typical)

**Network Impact**: Depends on database connection

**Optimization Options**:
1. Increase interval for less frequent saves
2. Implement delta saves (save only changed fields)
3. Compress JSON before saving
4. Batch saves if editing multiple modules

### Timer Accuracy

**Timer.Period**: Not precise (best-effort scheduling)

**Actual Interval**: May vary ±1 second

**Not Critical**: Auto-save doesn't require exact timing

---

## Acceptance Criteria - All Met ✅

| Criterion | Status | Details |
|-----------|--------|---------|
| Service starts/stops timer | ✅ | StartAsync() and Stop() methods |
| Auto-save runs at configured interval | ✅ | Loads from EditorConfiguration table |
| Only saves if IsDirty is true | ✅ | Checked in PerformAutoSaveAsync() |
| Saves to correct database table | ✅ | EditorFormModules via IEditorModuleRepository |
| Marks state as saved after success | ✅ | Calls EditorStateService.MarkAsSaved() |
| Error handling works and logs errors | ✅ | Try-catch with ILogger |
| Events fire appropriately | ✅ | AutoSaveCompleted and AutoSaveError events |
| Implements IDisposable | ✅ | Proper cleanup of timer and events |

---

## Files Created

| File | Lines | Purpose |
|------|-------|---------|
| `Services/State/AutoSaveEventArgs.cs` | 105 | Event argument classes |
| `Services/State/AutoSaveService.cs` | 439 | Auto-save service implementation |
| `AUTO_SAVE_SERVICE_SUMMARY.md` | This file | Documentation |

**Total**: 544 lines of code + documentation

**Project References Added**:
- `DynamicForms.Editor.Data` to `DynamicForms.Editor`

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
Time Elapsed 00:00:02.46
```

---

## Key Design Decisions

### 1. Timer vs BackgroundService

Used `System.Threading.Timer` instead of BackgroundService:

**Why**:
- Simpler for Blazor WebAssembly components
- Easier to start/stop dynamically
- Lower overhead
- Component-scoped lifecycle

**Alternative Considered**: BackgroundService (for server-side only)

---

### 2. IsDirty Check

Only saves when EditorStateService.IsDirty is true (unless manual save):

**Why**:
- Avoids unnecessary database writes
- Reduces network traffic
- Improves performance
- Standard behavior (Word, Excel, etc.)

---

### 3. Consecutive Failure Tracking

Tracks number of consecutive save failures:

**Why**:
- Enables retry logic in event handlers
- Can stop auto-save after repeated failures
- Helps diagnose persistent issues
- Resets on successful save or manual save

---

### 4. Manual Save Bypass

Manual saves bypass IsDirty check:

**Why**:
- User explicitly requested save
- May want to save even if no changes
- Creates checkpoint in history
- Standard behavior in editors

---

### 5. Configuration from Database

Loads interval from EditorConfiguration table:

**Why**:
- User-configurable at runtime
- No code changes needed
- Per-environment settings
- Can be changed without redeployment

---

### 6. Event-Based Notifications

Uses events instead of callbacks:

**Why**:
- Standard .NET pattern
- Supports multiple subscribers
- Decoupled architecture
- Easy to unsubscribe

---

### 7. Separate Create vs Update

Checks if module exists and creates or updates accordingly:

**Why**:
- Preserves database IDs
- Supports versioning
- Allows for audit trails
- Matches repository pattern

---

### 8. JSON Serialization in Service

Serializes FormModuleSchema to JSON in service (not in EditorStateService):

**Why**:
- EditorStateService is storage-agnostic
- AutoSaveService handles persistence details
- Clear separation of concerns
- Easier to test

---

## Future Enhancements

### 1. Workflow Support

```csharp
// TODO: Implement when IEditorWorkflowRepository is available
if (entityType == EditorEntityType.Workflow && currentWorkflow != null)
{
    (entityId, dataSize) = await SaveWorkflowAsync(currentWorkflow, cancellationToken);
}
```

### 2. Delta Saves

Save only changed fields instead of entire schema:
- Reduces database writes
- Faster saves
- Lower bandwidth

### 3. Compression

Compress JSON before saving:
- Reduce storage size
- Faster network transfer
- More history retained

### 4. Conflict Detection

Detect if another user edited the same module:
- Compare timestamps
- Merge changes
- Alert user to conflicts

### 5. Offline Support

Queue saves when offline:
- Store in IndexedDB
- Sync when online
- Conflict resolution

---

## Testing Recommendations

### Unit Tests

```csharp
[Fact]
public async Task StartAsync_LoadsIntervalFromConfiguration()
{
    // Arrange
    var configRepo = new Mock<IEditorConfigurationRepository>();
    configRepo.Setup(r => r.GetIntValueAsync(
        "AutoSave.IntervalSeconds", 30, default))
        .ReturnsAsync(60);

    var service = new AutoSaveService(
        _editorState, _moduleRepo, configRepo.Object, _logger);

    // Act
    await service.StartAsync();

    // Assert
    Assert.Equal(60, service.IntervalSeconds);
    Assert.True(service.IsEnabled);
}

[Fact]
public async Task SaveNowAsync_SavesEvenWhenNotDirty()
{
    // Arrange
    var module = CreateTestModule();
    _editorState.LoadModule(module);
    _editorState.MarkAsSaved(); // Not dirty

    // Act
    var result = await _autoSave.SaveNowAsync();

    // Assert
    Assert.True(result);
    _moduleRepo.Verify(r => r.UpdateAsync(
        It.IsAny<EditorFormModule>(), default), Times.Once);
}

[Fact]
public async Task PerformAutoSaveAsync_SkipsWhenNotDirty()
{
    // Arrange
    var module = CreateTestModule();
    _editorState.LoadModule(module);
    _editorState.MarkAsSaved(); // Not dirty

    // Act - use reflection to call private method
    var method = typeof(AutoSaveService).GetMethod(
        "PerformAutoSaveAsync",
        BindingFlags.NonPublic | BindingFlags.Instance);
    var task = (Task<bool>)method.Invoke(_autoSave, new object[] { false, default });
    var result = await task;

    // Assert
    Assert.False(result);
    _moduleRepo.Verify(r => r.UpdateAsync(
        It.IsAny<EditorFormModule>(), default), Times.Never);
}

[Fact]
public async Task AutoSaveCompleted_EventFires()
{
    // Arrange
    var module = CreateTestModule();
    _editorState.LoadModule(module);
    _editorState.UpdateModule(module, "Test change"); // Make dirty

    AutoSaveEventArgs? eventArgs = null;
    _autoSave.AutoSaveCompleted += (s, e) => eventArgs = e;

    // Act
    await _autoSave.SaveNowAsync();

    // Assert
    Assert.NotNull(eventArgs);
    Assert.Equal(EditorEntityType.Module, eventArgs.EntityType);
    Assert.True(eventArgs.IsManualSave);
}

[Fact]
public async Task AutoSaveError_EventFires()
{
    // Arrange
    var module = CreateTestModule();
    _editorState.LoadModule(module);
    _editorState.UpdateModule(module, "Test change");

    _moduleRepo.Setup(r => r.UpdateAsync(
        It.IsAny<EditorFormModule>(), default))
        .ThrowsAsync(new Exception("Database error"));

    AutoSaveErrorEventArgs? eventArgs = null;
    _autoSave.AutoSaveError += (s, e) => eventArgs = e;

    // Act
    await _autoSave.SaveNowAsync();

    // Assert
    Assert.NotNull(eventArgs);
    Assert.Equal("Database error", eventArgs.Exception.Message);
    Assert.Equal(1, eventArgs.FailureCount);
}

[Fact]
public void Dispose_StopsTimerAndUnsubscribes()
{
    // Arrange
    var service = new AutoSaveService(
        _editorState, _moduleRepo, _configRepo, _logger);

    // Act
    service.Dispose();

    // Assert
    Assert.False(service.IsEnabled);
    // Verify no memory leaks by checking event subscriptions
}
```

### Integration Tests

```csharp
[Fact]
public async Task IntegrationTest_AutoSaveCreatesThenUpdatesModule()
{
    // Arrange - use real repositories with in-memory database
    var options = new DbContextOptionsBuilder<EditorDbContext>()
        .UseInMemoryDatabase(databaseName: "AutoSaveTest")
        .Options;

    using var context = new EditorDbContext(options);
    var moduleRepo = new EditorModuleRepository(context);
    var configRepo = new EditorConfigurationRepository(context);

    // Set up configuration
    await configRepo.SetIntValueAsync("AutoSave.IntervalSeconds", 30, "Test");

    var editorState = new EditorStateService();
    var autoSave = new AutoSaveService(editorState, moduleRepo, configRepo, _logger);

    // Create test module
    var module = FormModuleSchema.Create(id: 1, titleEn: "Test Module");
    editorState.LoadModule(module);
    editorState.UpdateModule(module, "Initial state");

    // Act - First save (create)
    var result1 = await autoSave.SaveNowAsync();

    // Assert - Module created
    Assert.True(result1);
    var savedModule = await moduleRepo.GetByModuleIdAsync(1);
    Assert.NotNull(savedModule);
    Assert.Equal("Test Module", savedModule.Title);

    // Act - Update module
    var updatedModule = module with { TitleEn = "Updated Module" };
    editorState.UpdateModule(updatedModule, "Update title");
    var result2 = await autoSave.SaveNowAsync();

    // Assert - Module updated
    Assert.True(result2);
    savedModule = await moduleRepo.GetByModuleIdAsync(1);
    Assert.Equal("Updated Module", savedModule.Title);
}
```

---

## Next Steps

With AutoSaveService complete, next steps for Phase 3:

1. **Integrate with UI Components**
   - Add save status indicator to editor toolbar
   - Implement manual save button
   - Show auto-save settings panel

2. **Configure Service Registration**
   - Register in DI container
   - Set up DbContext
   - Configure logging

3. **Add Keyboard Shortcuts**
   - Ctrl+S for manual save
   - Show save status in footer

4. **Implement Conflict Detection**
   - Check for concurrent edits
   - Alert user to conflicts
   - Provide merge UI

5. **Add Workflow Support**
   - Create IEditorWorkflowRepository
   - Implement SaveWorkflowAsync()
   - Test workflow auto-save

---

*Generated: November 28, 2025*
*Project: DynamicForms Visual Editor*
*Phase: 3 - Visual Form Editor*
*Component: AutoSaveService (COMPLETED)*
