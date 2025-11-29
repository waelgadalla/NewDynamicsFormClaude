# Visual Form Editor - Design Proposal

## Executive Summary

This document outlines the comprehensive design for a **Blazor Server-based Visual Form Editor** for DynamicForms.Core.V2. The editor enables non-technical users to create, edit, and manage complex hierarchical forms and multi-module workflows without writing code.

### Key Capabilities
- Create and edit form modules with hierarchical field relationships
- Build multi-module workflows with complex conditional logic
- Preview forms in real-time with validation
- Import/export form schemas as JSON
- Publish forms to production via database deployment
- Undo/redo support (100 actions, configurable)
- Auto-save functionality (30 seconds, configurable)
- Modern, clean, button-based UI (no drag-and-drop)

### Technology Stack
- **Framework**: Blazor Server (.NET 9.0)
- **Database**: SQL Server
- **State Management**: Custom service-based with change tracking
- **UI Library**: Bootstrap 5 + custom CSS
- **Authentication**: None (future consideration)

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Application Structure](#application-structure)
3. [Database Schema](#database-schema)
4. [State Management Strategy](#state-management-strategy)
5. [UI/UX Design Patterns](#uiux-design-patterns)
6. [Form Renderer Architecture](#form-renderer-architecture)
7. [Deployment Model](#deployment-model)
8. [Security Considerations](#security-considerations)
9. [Performance Optimization](#performance-optimization)
10. [Future Enhancements](#future-enhancements)

---

## Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Blazor Server Host                        │
└─────────────────────────────────────────────────────────────┘
                            │
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────▼────────┐                   ┌─────────▼─────────┐
│  Editor Pages  │                   │  Renderer Pages   │
│  (Create/Edit) │                   │  (Preview/Host)   │
└───────┬────────┘                   └─────────┬─────────┘
        │                                       │
┌───────▼────────────────────────────┐  ┌──────▼──────────┐
│   Editor Components & Services     │  │ Renderer Engine │
├────────────────────────────────────┤  ├─────────────────┤
│ - EditorStateService               │  │ - Dynamic Field │
│ - UndoRedoService                  │  │   Components    │
│ - AutoSaveService                  │  │ - Validation    │
│ - ValidationPreviewService         │  │ - Conditional   │
│ - FormBuilderService               │  │   Logic Engine  │
└───────┬────────────────────────────┘  └──────┬──────────┘
        │                                       │
┌───────▼────────────────────────────────────────▼─────────┐
│              DynamicForms.Core.V2 Services               │
│  - FormHierarchyService                                  │
│  - FormValidationService                                 │
│  - IFormModuleRepository                                 │
└───────┬──────────────────────────────────────────────────┘
        │
┌───────▼──────────────────────────────────────────────────┐
│                    SQL Server Database                    │
├──────────────────────────────────────────────────────────┤
│  Tables:                                                  │
│  - EditorFormModules (draft/working versions)            │
│  - PublishedFormModules (production versions)            │
│  - EditorHistory (undo/redo snapshots)                   │
│  - EditorWorkflows (multi-module workflows)              │
└──────────────────────────────────────────────────────────┘
```

### Key Architectural Principles

1. **Separation of Concerns**
   - Editor logic separate from renderer logic
   - Renderer can be used standalone in production apps
   - State management isolated in dedicated services

2. **Immutable Snapshots**
   - All edits create new snapshots for undo/redo
   - Auto-save works on snapshots without disrupting editing
   - Change tracking via snapshot comparison

3. **Service-Based Design**
   - All business logic in services (not in components)
   - Components are thin UI layers
   - Testable, maintainable, debuggable

4. **Reusable Renderer**
   - Renderer is a standalone library
   - Production apps only need renderer + JSON schema
   - Zero dependency on editor components

---

## Application Structure

### Project Organization

```
Solution: DynamicForms.sln
│
├── Src/
│   ├── DynamicForms.Core.V2/              [Existing - Core schemas & services]
│   │
│   ├── DynamicForms.Renderer/             [NEW - Reusable form renderer]
│   │   ├── Components/
│   │   │   ├── DynamicFormRenderer.razor
│   │   │   ├── Fields/
│   │   │   │   ├── TextFieldRenderer.razor
│   │   │   │   ├── DropDownRenderer.razor
│   │   │   │   ├── DatePickerRenderer.razor
│   │   │   │   ├── FileUploadRenderer.razor
│   │   │   │   ├── ModalTableRenderer.razor
│   │   │   │   └── ... (all field types)
│   │   │   └── Containers/
│   │   │       ├── SectionRenderer.razor
│   │   │       ├── TabRenderer.razor
│   │   │       └── PanelRenderer.razor
│   │   ├── Services/
│   │   │   ├── IFormRenderService.cs
│   │   │   ├── FormRenderService.cs
│   │   │   ├── ConditionalLogicEngine.cs
│   │   │   └── ValidationDisplayService.cs
│   │   └── Models/
│   │       ├── FormData.cs              (user input values)
│   │       └── RenderContext.cs         (runtime state)
│   │
│   ├── DynamicForms.Editor/               [NEW - Visual editor Blazor app]
│   │   ├── Pages/
│   │   │   ├── Index.razor                (landing/dashboard)
│   │   │   ├── Editor/
│   │   │   │   ├── ModuleEditor.razor     (edit single module)
│   │   │   │   ├── WorkflowEditor.razor   (edit multi-module workflow)
│   │   │   │   └── FieldEditor.razor      (edit field properties)
│   │   │   └── Preview/
│   │   │       └── FormPreview.razor      (preview tab)
│   │   ├── Components/
│   │   │   ├── Shared/
│   │   │   │   ├── EditorLayout.razor
│   │   │   │   ├── Toolbar.razor
│   │   │   │   └── StatusBar.razor
│   │   │   ├── FieldList/
│   │   │   │   ├── FieldListItem.razor
│   │   │   │   ├── FieldActionButtons.razor  (Edit/Delete/Clone/Move)
│   │   │   │   └── FieldTypeSelector.razor
│   │   │   ├── Properties/
│   │   │   │   ├── FieldPropertiesPanel.razor
│   │   │   │   ├── ConditionalRuleEditor.razor
│   │   │   │   └── ValidationRuleEditor.razor
│   │   │   └── Workflow/
│   │   │       ├── WorkflowModuleList.razor
│   │   │       └── ConditionalBranchEditor.razor
│   │   ├── Services/
│   │   │   ├── State/
│   │   │   │   ├── EditorStateService.cs       (current edit state)
│   │   │   │   ├── UndoRedoService.cs          (history management)
│   │   │   │   └── AutoSaveService.cs          (background save)
│   │   │   ├── Operations/
│   │   │   │   ├── FormBuilderService.cs       (create/modify forms)
│   │   │   │   ├── FieldOperationService.cs    (CRUD for fields)
│   │   │   │   └── WorkflowOperationService.cs (workflow logic)
│   │   │   └── Data/
│   │   │       ├── EditorRepository.cs         (DB access)
│   │   │       └── PublishService.cs           (publish to prod)
│   │   └── Data/
│   │       └── ApplicationDbContext.cs
│   │
│   └── DynamicForms.HostApp/                  [NEW - Example production app]
│       ├── Pages/
│       │   └── RenderForm.razor               (uses renderer only)
│       └── Services/
│           └── PublishedFormRepository.cs     (reads published forms)
│
└── Tests/
    ├── DynamicForms.Renderer.Tests/
    ├── DynamicForms.Editor.Tests/
    └── DynamicForms.Integration.Tests/
```

---

## Database Schema

### Tables Overview

```sql
-- ================================================================
-- EDITOR TABLES (Working/Draft Data)
-- ================================================================

-- Stores form modules being edited (draft versions)
CREATE TABLE EditorFormModules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ModuleId INT NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    TitleFr NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL,
    DescriptionFr NVARCHAR(MAX) NULL,
    SchemaJson NVARCHAR(MAX) NOT NULL,  -- Full FormModuleSchema as JSON
    Version INT NOT NULL DEFAULT 1,
    Status NVARCHAR(50) NOT NULL,       -- Draft, Published, Archived
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(256) NULL,      -- Future: username

    INDEX IX_ModuleId (ModuleId),
    INDEX IX_Status (Status),
    INDEX IX_ModifiedAt (ModifiedAt)
);

-- Stores multi-module workflows
CREATE TABLE EditorWorkflows (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    TitleFr NVARCHAR(500) NULL,
    Description NVARCHAR(MAX) NULL,
    SchemaJson NVARCHAR(MAX) NOT NULL,  -- Full FormWorkflowSchema as JSON
    Version INT NOT NULL DEFAULT 1,
    Status NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedBy NVARCHAR(256) NULL,

    INDEX IX_WorkflowId (WorkflowId),
    INDEX IX_Status (Status)
);

-- Stores undo/redo history snapshots
CREATE TABLE EditorHistory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    EditorSessionId UNIQUEIDENTIFIER NOT NULL,  -- Groups edits in one session
    EntityType NVARCHAR(50) NOT NULL,            -- Module, Workflow
    EntityId INT NOT NULL,                       -- ModuleId or WorkflowId
    SnapshotJson NVARCHAR(MAX) NOT NULL,        -- Full snapshot
    ActionDescription NVARCHAR(500) NULL,        -- "Added field 'Email'"
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    SequenceNumber INT NOT NULL,                 -- Order within session (1, 2, 3...)

    INDEX IX_EditorSessionId (EditorSessionId),
    INDEX IX_EntityTypeId (EntityType, EntityId),
    INDEX IX_CreatedAt (CreatedAt)
);

-- ================================================================
-- PRODUCTION TABLES (Published Data)
-- ================================================================

-- Stores published form modules (production versions)
CREATE TABLE PublishedFormModules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ModuleId INT NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    TitleFr NVARCHAR(500) NULL,
    SchemaJson NVARCHAR(MAX) NOT NULL,  -- Full FormModuleSchema as JSON
    Version INT NOT NULL,
    PublishedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PublishedBy NVARCHAR(256) NULL,
    IsActive BIT NOT NULL DEFAULT 1,     -- Allows soft delete

    INDEX IX_ModuleId_Version (ModuleId, Version DESC),
    INDEX IX_IsActive (IsActive)
);

-- Stores published workflows (production versions)
CREATE TABLE PublishedWorkflows (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    WorkflowId INT NOT NULL,
    Title NVARCHAR(500) NOT NULL,
    TitleFr NVARCHAR(500) NULL,
    SchemaJson NVARCHAR(MAX) NOT NULL,  -- Full FormWorkflowSchema as JSON
    Version INT NOT NULL,
    PublishedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PublishedBy NVARCHAR(256) NULL,
    IsActive BIT NOT NULL DEFAULT 1,

    INDEX IX_WorkflowId_Version (WorkflowId, Version DESC),
    INDEX IX_IsActive (IsActive)
);

-- ================================================================
-- CONFIGURATION TABLE
-- ================================================================

-- Stores editor configuration (auto-save interval, undo limit, etc.)
CREATE TABLE EditorConfiguration (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ConfigKey NVARCHAR(100) NOT NULL UNIQUE,
    ConfigValue NVARCHAR(500) NOT NULL,
    ConfigType NVARCHAR(50) NOT NULL,    -- Int, String, Bool
    Description NVARCHAR(500) NULL,
    ModifiedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    INDEX IX_ConfigKey (ConfigKey)
);

-- Insert default configurations
INSERT INTO EditorConfiguration (ConfigKey, ConfigValue, ConfigType, Description)
VALUES
    ('AutoSave.IntervalSeconds', '30', 'Int', 'Auto-save interval in seconds'),
    ('UndoRedo.MaxActions', '100', 'Int', 'Maximum number of undo/redo actions to store'),
    ('History.RetentionDays', '90', 'Int', 'Days to keep history snapshots before cleanup');
```

### Entity Relationships

```
EditorFormModules 1──────* EditorHistory
EditorWorkflows   1──────* EditorHistory

EditorFormModules 1──────* PublishedFormModules (via ModuleId)
EditorWorkflows   1──────* PublishedWorkflows   (via WorkflowId)
```

---

## State Management Strategy

### Core Principles

1. **Snapshot-Based State**
   - Every edit creates a new immutable snapshot
   - Current state is always the latest snapshot
   - Undo/redo navigates through snapshots

2. **Service Ownership**
   - `EditorStateService` owns current edit state
   - `UndoRedoService` owns history stack
   - `AutoSaveService` owns persistence timing

3. **Change Detection**
   - Compare current snapshot with last saved snapshot
   - Track "dirty" state for UI indicators
   - Prevent navigation if unsaved changes exist

### EditorStateService

```csharp
/// <summary>
/// Manages the current editing state for a form module or workflow.
/// </summary>
public class EditorStateService
{
    // Current editing context
    public Guid EditorSessionId { get; private set; }
    public EditorEntityType EntityType { get; private set; }  // Module or Workflow

    // Current state (mutable during editing)
    public FormModuleSchema? CurrentModule { get; private set; }
    public FormWorkflowSchema? CurrentWorkflow { get; private set; }

    // Metadata
    public bool IsDirty { get; private set; }
    public DateTime LastModified { get; private set; }
    public DateTime? LastSaved { get; private set; }

    // Events for UI reactivity
    public event EventHandler? StateChanged;
    public event EventHandler? ModuleChanged;
    public event EventHandler? WorkflowChanged;

    // Operations
    public void LoadModule(FormModuleSchema module);
    public void LoadWorkflow(FormWorkflowSchema workflow);
    public void UpdateModule(FormModuleSchema module, string actionDescription);
    public void UpdateWorkflow(FormWorkflowSchema workflow, string actionDescription);
    public FormModuleSchema? GetCurrentModule();
    public FormWorkflowSchema? GetCurrentWorkflow();
    public void MarkAsSaved();
    public void ResetSession();
}
```

### UndoRedoService

```csharp
/// <summary>
/// Manages undo/redo functionality using snapshot stack.
/// </summary>
public class UndoRedoService
{
    private readonly Stack<EditorSnapshot> _undoStack;
    private readonly Stack<EditorSnapshot> _redoStack;
    private readonly int _maxActions;  // Configurable, default 100

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public int UndoCount => _undoStack.Count;
    public int RedoCount => _redoStack.Count;

    public event EventHandler? StackChanged;

    // Operations
    public void PushSnapshot(EditorSnapshot snapshot, string actionDescription);
    public EditorSnapshot? Undo();
    public EditorSnapshot? Redo();
    public void Clear();
    public List<string> GetUndoActionHistory();  // For UI display
    public List<string> GetRedoActionHistory();
}

/// <summary>
/// Immutable snapshot of editor state at a point in time.
/// </summary>
public record EditorSnapshot
{
    public Guid SessionId { get; init; }
    public EditorEntityType EntityType { get; init; }
    public string SnapshotJson { get; init; } = string.Empty;  // Serialized state
    public string ActionDescription { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public int SequenceNumber { get; init; }
}
```

### AutoSaveService

```csharp
/// <summary>
/// Handles automatic background saving of editor state.
/// </summary>
public class AutoSaveService : IDisposable
{
    private readonly EditorStateService _stateService;
    private readonly EditorRepository _repository;
    private readonly ILogger<AutoSaveService> _logger;
    private Timer? _autoSaveTimer;
    private readonly int _intervalSeconds;  // Configurable, default 30

    public bool IsEnabled { get; private set; }
    public DateTime? LastAutoSave { get; private set; }
    public bool IsSaving { get; private set; }

    public event EventHandler<AutoSaveEventArgs>? AutoSaveCompleted;
    public event EventHandler<AutoSaveErrorEventArgs>? AutoSaveError;

    // Operations
    public void Start();
    public void Stop();
    public Task SaveNowAsync();  // Manual trigger
    private async Task PerformAutoSaveAsync();
}
```

### State Flow Diagram

```
User Action (e.g., Add Field)
    │
    ├──> EditorStateService.UpdateModule(newModule, "Added field 'Email'")
    │        │
    │        ├──> Create new FormModuleSchema with field added
    │        ├──> Set IsDirty = true
    │        ├──> Trigger StateChanged event
    │        │
    │        └──> UndoRedoService.PushSnapshot(snapshot, "Added field 'Email'")
    │                 │
    │                 ├──> Add to undo stack
    │                 ├──> Clear redo stack
    │                 └──> Trigger StackChanged event
    │
    └──> UI updates (Blazor re-renders)

Auto-save Timer Fires (every 30 seconds)
    │
    └──> AutoSaveService.PerformAutoSaveAsync()
             │
             ├──> Check if IsDirty
             ├──> Get current snapshot from EditorStateService
             ├──> Save to database (EditorFormModules)
             ├──> EditorStateService.MarkAsSaved()
             └──> Trigger AutoSaveCompleted event
```

---

## UI/UX Design Patterns

### Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│  Toolbar: [Save] [Publish] [Import] [Export] [Undo] [Redo] │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Tabs: [Editor] [Preview] [JSON]                            │
│  ──────────────────────────────────────────────────────────│
│                                                              │
│  ┌─────────────────────┬──────────────────────────────────┐│
│  │  Field List         │  Properties Panel                ││
│  │  (30% width)        │  (70% width)                     ││
│  │                     │                                  ││
│  │  □ Section: Org     │  Field ID: [org_name______]     ││
│  │    ├ Text: Name     │  Label (EN): [Organization Name]││
│  │    │  [↑][↓][✏][⎘][🗑]  Label (FR): [Nom organisation]││
│  │    ├ Drop: Type     │  Field Type: [TextBox ▼]        ││
│  │    │  [↑][↓][✏][⎘][🗑]  Required: [✓]                  ││
│  │    └ Text: Email    │  Max Length: [200__]            ││
│  │       [↑][↓][✏][⎘][🗑]  Validation: [+ Add Rule]       ││
│  │                     │  Conditional: [+ Add Rule]      ││
│  │  □ Section: Project │  ...                            ││
│  │    └ ...            │                                  ││
│  │                     │                                  ││
│  │  [+ Add Field ▼]    │  [Apply Changes]                ││
│  └─────────────────────┴──────────────────────────────────┘│
│                                                              │
├─────────────────────────────────────────────────────────────┤
│  Status: Auto-saved at 10:34 AM | Modified 2 min ago       │
└─────────────────────────────────────────────────────────────┘
```

### Button Icons Legend
- **↑** = Move up
- **↓** = Move down
- **✏** = Edit
- **⎘** = Clone/Copy
- **🗑** = Delete

### Key UI Components

#### 1. Field List Item Component
```razor
<div class="field-list-item @GetDepthClass()">
    <div class="field-header">
        <span class="field-icon">@GetFieldIcon()</span>
        <span class="field-label">@Field.Label</span>
        <span class="field-id text-muted">(@Field.FieldId)</span>
    </div>
    <div class="field-actions">
        <button @onclick="MoveUp" disabled="@IsFirst" title="Move Up">↑</button>
        <button @onclick="MoveDown" disabled="@IsLast" title="Move Down">↓</button>
        <button @onclick="Edit" title="Edit">✏</button>
        <button @onclick="Clone" title="Clone">⎘</button>
        <button @onclick="Delete" title="Delete" class="btn-danger">🗑</button>
    </div>
</div>
```

#### 2. Field Properties Panel
```razor
<div class="properties-panel">
    @if (SelectedField != null)
    {
        <h4>Field Properties</h4>

        <div class="form-group">
            <label>Field ID</label>
            <input @bind="SelectedField.FieldId" class="form-control" />
        </div>

        <div class="form-group">
            <label>Label (EN)</label>
            <input @bind="SelectedField.Label" class="form-control" />
        </div>

        <div class="form-group">
            <label>Label (FR)</label>
            <input @bind="SelectedField.LabelFr" class="form-control" />
        </div>

        <div class="form-group">
            <label>Field Type</label>
            <select @bind="SelectedFieldType" class="form-select">
                <option value="TextBox">Text Box</option>
                <option value="DropDown">Drop Down</option>
                <option value="DatePicker">Date Picker</option>
                <!-- ... -->
            </select>
        </div>

        <!-- Dynamic properties based on field type -->
        @if (SelectedFieldType == "TextBox")
        {
            <div class="form-group">
                <label>Max Length</label>
                <input type="number" @bind="TextConfig.MaxLength" />
            </div>
        }

        <!-- Validation rules section -->
        <div class="validation-section">
            <h5>Validation Rules</h5>
            @foreach (var rule in SelectedField.ValidationRules)
            {
                <ValidationRuleEditor Rule="@rule" OnRemove="RemoveRule" />
            }
            <button @onclick="AddValidationRule">+ Add Rule</button>
        </div>

        <!-- Conditional logic section -->
        <div class="conditional-section">
            <h5>Conditional Rules</h5>
            @foreach (var rule in SelectedField.ConditionalRules)
            {
                <ConditionalRuleEditor Rule="@rule" OnRemove="RemoveRule" />
            }
            <button @onclick="AddConditionalRule">+ Add Rule</button>
        </div>

        <button @onclick="ApplyChanges" class="btn btn-primary">Apply Changes</button>
        <button @onclick="CancelChanges" class="btn btn-secondary">Cancel</button>
    }
    else
    {
        <p class="text-muted">Select a field to edit properties</p>
    }
</div>
```

#### 3. Tabbed Editor/Preview
```razor
<div class="editor-tabs">
    <ul class="nav nav-tabs">
        <li class="nav-item">
            <a class="nav-link @(ActiveTab == "editor" ? "active" : "")"
               @onclick="() => SetActiveTab(\"editor\")">Editor</a>
        </li>
        <li class="nav-item">
            <a class="nav-link @(ActiveTab == "preview" ? "active" : "")"
               @onclick="() => SetActiveTab(\"preview\")">Preview</a>
        </li>
        <li class="nav-item">
            <a class="nav-link @(ActiveTab == "json" ? "active" : "")"
               @onclick="() => SetActiveTab(\"json\")">JSON</a>
        </li>
    </ul>

    <div class="tab-content">
        @if (ActiveTab == "editor")
        {
            <!-- Field list + properties panel -->
        }
        else if (ActiveTab == "preview")
        {
            <FormPreview ModuleSchema="@CurrentModule" />
        }
        else if (ActiveTab == "json")
        {
            <pre>@GetFormattedJson()</pre>
        }
    </div>
</div>
```

### Styling Guidelines

**Modern, Clean Design:**
- Use Bootstrap 5 base with custom CSS variables
- Color scheme: Primary (blue), Success (green), Danger (red), Warning (orange)
- Font: System UI fonts (San Francisco, Segoe UI, Roboto)
- Spacing: Consistent 8px grid system
- Shadows: Subtle elevation for panels and buttons
- Animations: Smooth transitions (200ms ease-in-out)

**Button States:**
- Default: Gray background
- Hover: Slight elevation + color change
- Active: Darker background
- Disabled: 50% opacity, no pointer events

**Field List Indentation:**
- Level 0 (root): 0px padding
- Level 1 (child): 20px padding-left
- Level 2 (grandchild): 40px padding-left
- Visual guide: Dotted left border for hierarchy

---

## Form Renderer Architecture

### Design Goals

1. **Standalone Library** - Can be used in any Blazor app without editor dependencies
2. **JSON-Driven** - Takes FormModuleSchema JSON, renders complete form
3. **No State Coupling** - Renderer has no knowledge of editor state
4. **Conditional Logic** - Evaluates conditional rules dynamically
5. **Validation Display** - Shows validation errors inline
6. **Accessibility** - WCAG 2.1 AA compliant

### Renderer Component Hierarchy

```
DynamicFormRenderer (root component)
    │
    ├── Receives: FormModuleSchema (deserialized from JSON)
    ├── Maintains: FormData (user input values)
    ├── Provides: Submit event, Validation event
    │
    ├──> SectionRenderer (for Section field types)
    │       │
    │       └──> [Recursive child fields]
    │
    ├──> TabRenderer (for Tab field types)
    │       │
    │       └──> [Tab panels with child fields]
    │
    ├──> TextFieldRenderer (for TextBox fields)
    │
    ├──> DropDownRenderer (for DropDown fields)
    │
    ├──> DatePickerRenderer (for DatePicker fields)
    │
    ├──> FileUploadRenderer (for FileUpload fields)
    │
    ├──> ModalTableRenderer (for ModalTable fields)
    │
    └──> [Other field renderers...]
```

### Core Renderer Interface

```csharp
/// <summary>
/// Main component for rendering forms from schema.
/// </summary>
public partial class DynamicFormRenderer : ComponentBase
{
    [Parameter] public FormModuleSchema Schema { get; set; } = null!;
    [Parameter] public FormData? InitialData { get; set; }
    [Parameter] public EventCallback<FormData> OnSubmit { get; set; }
    [Parameter] public EventCallback<ValidationResult> OnValidationChanged { get; set; }
    [Parameter] public bool ReadOnly { get; set; } = false;
    [Parameter] public string CssClass { get; set; } = string.Empty;

    [Inject] private IFormRenderService RenderService { get; set; } = null!;
    [Inject] private IFormValidationService ValidationService { get; set; } = null!;

    private FormModuleRuntime _runtime = null!;
    private FormData _formData = new();
    private Dictionary<string, List<string>> _validationErrors = new();

    protected override async Task OnParametersSetAsync()
    {
        // Build hierarchy from schema
        _runtime = await RenderService.BuildRuntimeAsync(Schema);

        // Initialize form data
        _formData = InitialData ?? new FormData();

        // Evaluate initial conditional logic
        await EvaluateConditionalLogicAsync();
    }

    private async Task HandleFieldValueChanged(string fieldId, object? value)
    {
        _formData.SetValue(fieldId, value);
        await EvaluateConditionalLogicAsync();
        await ValidateFieldAsync(fieldId);
        await OnValidationChanged.InvokeAsync(await GetValidationResultAsync());
    }

    private async Task HandleSubmit()
    {
        var validationResult = await ValidationService.ValidateModuleAsync(Schema, _formData);

        if (validationResult.IsValid)
        {
            await OnSubmit.InvokeAsync(_formData);
        }
        else
        {
            _validationErrors = validationResult.Errors
                .GroupBy(e => e.FieldId)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToList());
            StateHasChanged();
        }
    }
}
```

### FormData Model

```csharp
/// <summary>
/// Holds user-entered values for all fields in a form.
/// </summary>
public class FormData
{
    private readonly Dictionary<string, object?> _values = new();

    public object? GetValue(string fieldId)
    {
        return _values.TryGetValue(fieldId, out var value) ? value : null;
    }

    public T? GetValue<T>(string fieldId)
    {
        var value = GetValue(fieldId);
        return value is T typed ? typed : default;
    }

    public void SetValue(string fieldId, object? value)
    {
        _values[fieldId] = value;
    }

    public bool HasValue(string fieldId)
    {
        return _values.ContainsKey(fieldId) && _values[fieldId] != null;
    }

    public Dictionary<string, object?> GetAllValues()
    {
        return new Dictionary<string, object?>(_values);
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(_values);
    }

    public static FormData FromJson(string json)
    {
        var data = new FormData();
        var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
        if (values != null)
        {
            foreach (var kvp in values)
            {
                data.SetValue(kvp.Key, kvp.Value);
            }
        }
        return data;
    }
}
```

### Conditional Logic Engine

```csharp
/// <summary>
/// Evaluates conditional rules and determines field visibility/enablement.
/// </summary>
public class ConditionalLogicEngine
{
    public bool EvaluateCondition(ConditionalRule rule, FormData formData)
    {
        var targetValue = formData.GetValue(rule.TargetFieldId);

        return rule.Operator switch
        {
            ConditionalOperator.Equals => AreEqual(targetValue, rule.Value),
            ConditionalOperator.NotEquals => !AreEqual(targetValue, rule.Value),
            ConditionalOperator.Contains => Contains(targetValue, rule.Value),
            ConditionalOperator.GreaterThan => Compare(targetValue, rule.Value) > 0,
            ConditionalOperator.LessThan => Compare(targetValue, rule.Value) < 0,
            ConditionalOperator.IsEmpty => targetValue == null || string.IsNullOrWhiteSpace(targetValue?.ToString()),
            ConditionalOperator.IsNotEmpty => targetValue != null && !string.IsNullOrWhiteSpace(targetValue?.ToString()),
            _ => false
        };
    }

    public bool ShouldShowField(FormFieldSchema field, FormData formData)
    {
        if (field.ConditionalRules.Count == 0)
            return true;  // No rules = always visible

        // Evaluate all conditional rules
        foreach (var rule in field.ConditionalRules)
        {
            if (!EvaluateCondition(rule, formData))
                return false;  // Any rule fails = hide field
        }

        return true;
    }

    public bool IsFieldEnabled(FormFieldSchema field, FormData formData)
    {
        // Similar logic for enabled/disabled state
        return ShouldShowField(field, formData);  // Simplified for now
    }
}
```

### Field Renderer Example: TextFieldRenderer

```razor
@inherits FieldRendererBase

<div class="form-group @CssClass">
    <label for="@FieldId" class="form-label">
        @Label
        @if (IsRequired)
        {
            <span class="text-danger">*</span>
        }
    </label>

    <input type="text"
           id="@FieldId"
           class="form-control @(HasErrors ? "is-invalid" : "")"
           value="@CurrentValue"
           @onchange="HandleValueChanged"
           disabled="@IsDisabled"
           maxlength="@MaxLength"
           placeholder="@Placeholder" />

    @if (HasErrors)
    {
        <div class="invalid-feedback d-block">
            @foreach (var error in Errors)
            {
                <div>@error</div>
            }
        </div>
    }

    @if (!string.IsNullOrEmpty(HelpText))
    {
        <small class="form-text text-muted">@HelpText</small>
    }
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = null!;
    [Parameter] public object? Value { get; set; }
    [Parameter] public EventCallback<object?> OnValueChanged { get; set; }
    [Parameter] public List<string> Errors { get; set; } = new();
    [Parameter] public bool IsDisabled { get; set; }

    private string FieldId => Field.FieldId;
    private string Label => /* Get localized label */;
    private bool IsRequired => Field.ValidationRules.Any(r => r.RuleType == "required");
    private int MaxLength => /* Extract from TextBoxConfig */;
    private string? Placeholder => /* Extract from TextBoxConfig */;
    private string CurrentValue => Value?.ToString() ?? string.Empty;
    private bool HasErrors => Errors.Any();

    private async Task HandleValueChanged(ChangeEventArgs e)
    {
        await OnValueChanged.InvokeAsync(e.Value?.ToString());
    }
}
```

---

## Deployment Model

### Publishing Workflow

```
┌────────────────────────────────────────────────────────────┐
│  1. User edits form in Editor                              │
│     - Changes auto-saved to EditorFormModules table        │
│     - Draft status                                         │
└────────────────┬───────────────────────────────────────────┘
                 │
┌────────────────▼───────────────────────────────────────────┐
│  2. User clicks "Publish" button                           │
│     - Validation runs (schema validation)                  │
│     - User confirms publish action                         │
└────────────────┬───────────────────────────────────────────┘
                 │
┌────────────────▼───────────────────────────────────────────┐
│  3. PublishService.PublishModuleAsync()                    │
│     - Increment version number                             │
│     - Copy schema to PublishedFormModules table            │
│     - Set IsActive = 1                                     │
│     - Mark previous version IsActive = 0 (if exists)       │
│     - Update EditorFormModules status = "Published"        │
└────────────────┬───────────────────────────────────────────┘
                 │
┌────────────────▼───────────────────────────────────────────┐
│  4. Production app reads from PublishedFormModules         │
│     - PublishedFormRepository.GetActiveModuleAsync(id)     │
│     - Returns latest active version                        │
│     - Passes schema to DynamicFormRenderer                 │
└────────────────────────────────────────────────────────────┘
```

### PublishService Implementation

```csharp
public class PublishService
{
    private readonly ApplicationDbContext _context;
    private readonly IFormValidationService _validationService;
    private readonly ILogger<PublishService> _logger;

    public async Task<PublishResult> PublishModuleAsync(int moduleId)
    {
        // 1. Load draft module
        var draftModule = await _context.EditorFormModules
            .FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.Status == "Draft");

        if (draftModule == null)
            return PublishResult.Failure("Module not found");

        // 2. Validate schema
        var schema = JsonSerializer.Deserialize<FormModuleSchema>(draftModule.SchemaJson);
        var validationResult = await ValidateSchemaForPublish(schema);

        if (!validationResult.IsValid)
            return PublishResult.Failure(validationResult.Errors);

        // 3. Get next version number
        var latestVersion = await _context.PublishedFormModules
            .Where(p => p.ModuleId == moduleId)
            .MaxAsync(p => (int?)p.Version) ?? 0;

        var newVersion = latestVersion + 1;

        // 4. Deactivate previous versions
        var previousVersions = await _context.PublishedFormModules
            .Where(p => p.ModuleId == moduleId && p.IsActive)
            .ToListAsync();

        foreach (var prev in previousVersions)
        {
            prev.IsActive = false;
        }

        // 5. Create published version
        var published = new PublishedFormModule
        {
            ModuleId = moduleId,
            Title = draftModule.Title,
            TitleFr = draftModule.TitleFr,
            SchemaJson = draftModule.SchemaJson,
            Version = newVersion,
            PublishedAt = DateTime.UtcNow,
            PublishedBy = "System",  // TODO: User tracking
            IsActive = true
        };

        _context.PublishedFormModules.Add(published);

        // 6. Update draft status
        draftModule.Status = "Published";
        draftModule.Version = newVersion;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Published module {ModuleId} version {Version}",
            moduleId, newVersion);

        return PublishResult.Success(newVersion);
    }
}
```

### Production App Repository

```csharp
/// <summary>
/// Repository for production apps to read published forms.
/// Read-only access to PublishedFormModules table.
/// </summary>
public class PublishedFormRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Gets the active (latest published) version of a module.
    /// Results are cached for 5 minutes.
    /// </summary>
    public async Task<FormModuleSchema?> GetActiveModuleAsync(int moduleId)
    {
        var cacheKey = $"published_module_{moduleId}";

        if (_cache.TryGetValue<FormModuleSchema>(cacheKey, out var cached))
            return cached;

        var published = await _context.PublishedFormModules
            .Where(p => p.ModuleId == moduleId && p.IsActive)
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync();

        if (published == null)
            return null;

        var schema = JsonSerializer.Deserialize<FormModuleSchema>(published.SchemaJson);

        _cache.Set(cacheKey, schema, TimeSpan.FromMinutes(5));

        return schema;
    }

    /// <summary>
    /// Gets a specific version of a module.
    /// </summary>
    public async Task<FormModuleSchema?> GetModuleVersionAsync(int moduleId, int version)
    {
        var published = await _context.PublishedFormModules
            .FirstOrDefaultAsync(p => p.ModuleId == moduleId && p.Version == version);

        return published == null
            ? null
            : JsonSerializer.Deserialize<FormModuleSchema>(published.SchemaJson);
    }
}
```

---

## Security Considerations

### Current Scope (No Authentication)

Since authentication is deferred, security focuses on:

1. **Input Validation**
   - Sanitize all user input in editor
   - Validate JSON schema structure before saving
   - Prevent script injection in labels/descriptions

2. **SQL Injection Prevention**
   - Use Entity Framework Core (parameterized queries)
   - Never concatenate user input into SQL

3. **JSON Injection Prevention**
   - Validate JSON structure before deserialization
   - Use `JsonSerializer` with strict options
   - Reject malformed JSON

### Future Authentication Requirements

When adding authentication:

```csharp
// Add to EditorFormModules table
CreatedBy NVARCHAR(256) NOT NULL,
ModifiedBy NVARCHAR(256) NOT NULL,

// Add to PublishedFormModules table
PublishedBy NVARCHAR(256) NOT NULL,

// Add authorization policies
services.AddAuthorization(options =>
{
    options.AddPolicy("CanEditForms", policy =>
        policy.RequireRole("FormEditor", "Admin"));

    options.AddPolicy("CanPublishForms", policy =>
        policy.RequireRole("FormPublisher", "Admin"));
});
```

### Data Protection

1. **Connection String Security**
   - Store in User Secrets (development)
   - Use Azure Key Vault (production)
   - Never commit to source control

2. **Encryption at Rest**
   - Enable SQL Server Transparent Data Encryption (TDE)
   - Encrypt sensitive field configurations

3. **Audit Trail**
   - Log all publish actions
   - Track who modified what and when
   - Retain history for compliance

---

## Performance Optimization

### Database Optimizations

1. **Indexing Strategy**
   ```sql
   -- Frequently queried columns
   CREATE INDEX IX_ModuleId ON EditorFormModules(ModuleId);
   CREATE INDEX IX_Status ON EditorFormModules(Status);
   CREATE INDEX IX_ModuleId_Version ON PublishedFormModules(ModuleId, Version DESC);
   CREATE INDEX IX_IsActive ON PublishedFormModules(IsActive);
   ```

2. **JSON Column Optimization**
   - Use NVARCHAR(MAX) with compression
   - Consider JSON indexes for large schemas (SQL Server 2022+)

3. **History Cleanup**
   ```sql
   -- Scheduled job to clean old history (retention: 90 days)
   DELETE FROM EditorHistory
   WHERE CreatedAt < DATEADD(DAY, -90, GETUTCDATE());
   ```

### Blazor Server Optimizations

1. **SignalR Configuration**
   ```csharp
   services.AddSignalR(options =>
   {
       options.MaximumReceiveMessageSize = 1024 * 1024;  // 1MB max
       options.EnableDetailedErrors = false;  // Production
   });
   ```

2. **Circuit Configuration**
   ```csharp
   services.AddServerSideBlazor(options =>
   {
       options.DetailedErrors = false;
       options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
       options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
   });
   ```

3. **Component Virtualization**
   - Use `<Virtualize>` for large field lists (>50 fields)
   - Lazy-load properties panel content

4. **Caching**
   ```csharp
   // Cache published forms in production apps
   services.AddMemoryCache(options =>
   {
       options.SizeLimit = 100;  // Limit cache entries
   });
   ```

### Renderer Optimizations

1. **Conditional Rendering**
   - Only render visible fields (skip hidden by conditional logic)
   - Use `@key` directives for list stability

2. **Debounced Validation**
   - Debounce field validation by 300ms
   - Prevent excessive validation on rapid typing

3. **Async Rendering**
   - Use `OnAfterRenderAsync` for expensive operations
   - Avoid blocking UI thread

---

## Future Enhancements

### Phase 2 Features (Next 6 months)

1. **Authentication & Authorization**
   - ASP.NET Core Identity integration
   - Role-based access control (Editor, Publisher, Viewer)
   - Audit trail with user tracking

2. **Multi-User Collaboration**
   - Optimistic locking for concurrent edits
   - Visual indicators when others are editing
   - Merge conflict resolution

3. **Advanced Workflow Features**
   - Visual workflow designer (flowchart view)
   - Complex branching with OR/AND conditions
   - Loop-back to previous modules

4. **Import/Export Enhancements**
   - Import from Excel templates
   - Export to PDF (printable forms)
   - Bulk import/export multiple forms

### Phase 3 Features (Next 12 months)

1. **Mobile Editor Support**
   - Responsive design for tablets
   - Touch-optimized field list

2. **Advanced Validation**
   - Cross-field validation rules
   - Custom JavaScript validators
   - Real-time validation preview

3. **Localization Management**
   - Support for additional languages
   - In-editor translation interface
   - Import/export translation files

4. **Analytics & Insights**
   - Track which forms are most used
   - Identify validation errors causing user drop-off
   - Performance metrics dashboard

5. **Version Comparison**
   - Visual diff between form versions
   - Rollback to previous version
   - Branch/merge workflows

---

## Implementation Roadmap

### Estimated Timeline: 12-16 weeks

#### **Weeks 1-2: Foundation**
- Database schema setup
- Project structure creation
- Core services implementation (EditorStateService, UndoRedoService, AutoSaveService)

#### **Weeks 3-4: Module Editor**
- Field list component
- Properties panel component
- Add/Edit/Delete/Clone operations
- Basic validation

#### **Weeks 5-6: Form Renderer**
- Core renderer component
- Field renderers (Text, DropDown, Date, etc.)
- Conditional logic engine
- Validation display

#### **Weeks 7-8: Workflow Editor**
- Multi-module workflow UI
- Conditional branching editor
- Workflow preview

#### **Weeks 9-10: Import/Export & Publish**
- JSON import/export
- Publish service
- Published form repository
- Production app integration

#### **Weeks 11-12: Polish & Testing**
- Unit tests
- Integration tests
- UI/UX refinements
- Performance optimization

#### **Weeks 13-14: Documentation**
- User guide
- Developer documentation
- API documentation

#### **Weeks 15-16: Deployment & Training**
- Production deployment
- User training
- Bug fixes

---

## Success Metrics

### Technical Metrics
- **Build Time**: < 30 seconds
- **Page Load**: < 2 seconds (editor page)
- **Form Render**: < 500ms (100 fields)
- **Auto-Save Latency**: < 1 second
- **Test Coverage**: > 80%

### User Experience Metrics
- **Time to Create Simple Form**: < 5 minutes
- **Time to Create Complex Form**: < 20 minutes
- **Undo/Redo Response**: < 100ms
- **Preview Refresh**: < 300ms

### Business Metrics
- **Forms Created**: Track monthly
- **Forms Published**: Track monthly
- **User Adoption**: % of target users using editor
- **Support Tickets**: Trend downward over time

---

## Appendix A: Configuration Options

### appsettings.json

```json
{
  "EditorSettings": {
    "AutoSave": {
      "Enabled": true,
      "IntervalSeconds": 30
    },
    "UndoRedo": {
      "MaxActions": 100
    },
    "History": {
      "RetentionDays": 90
    },
    "Validation": {
      "DebounceMilliseconds": 300
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "DynamicForms.Editor": "Debug"
    }
  }
}
```

---

## Appendix B: Database Migration Scripts

### Initial Migration

```sql
-- Run this script to create the initial database schema

-- Create database
CREATE DATABASE DynamicFormsEditor;
GO

USE DynamicFormsEditor;
GO

-- Create tables (see Database Schema section for full SQL)
-- ... (full CREATE TABLE statements)

-- Insert default configuration
INSERT INTO EditorConfiguration (ConfigKey, ConfigValue, ConfigType, Description)
VALUES
    ('AutoSave.IntervalSeconds', '30', 'Int', 'Auto-save interval in seconds'),
    ('UndoRedo.MaxActions', '100', 'Int', 'Maximum number of undo/redo actions'),
    ('History.RetentionDays', '90', 'Int', 'Days to keep history before cleanup');
GO
```

---

**Document Version**: 1.0
**Last Updated**: January 2025
**Status**: Draft for Implementation
