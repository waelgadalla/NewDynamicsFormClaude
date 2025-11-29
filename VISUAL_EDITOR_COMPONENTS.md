# Visual Form Editor - Component Design

## Overview

This document provides detailed component specifications, code examples, and implementation guidance for all Blazor components in the Visual Form Editor. Each component is designed to be modular, testable, and follows Blazor best practices.

---

## Table of Contents

1. [Component Hierarchy](#component-hierarchy)
2. [Page Components](#page-components)
3. [Editor Components](#editor-components)
4. [Renderer Components](#renderer-components)
5. [Shared Components](#shared-components)
6. [State Management Integration](#state-management-integration)
7. [Event Handling Patterns](#event-handling-patterns)
8. [Styling Guide](#styling-guide)

---

## Component Hierarchy

```
App.razor
└── MainLayout.razor
    ├── NavMenu.razor
    └── Pages/
        ├── Index.razor (Dashboard)
        ├── ModuleList.razor (Browse modules)
        │
        ├── Editor/
        │   ├── ModuleEditor.razor ⭐ Main editor page
        │   │   ├── EditorToolbar.razor
        │   │   ├── EditorTabs.razor
        │   │   │   ├── [Editor Tab]
        │   │   │   │   ├── FieldListPanel.razor
        │   │   │   │   │   ├── FieldListItem.razor (recursive)
        │   │   │   │   │   └── AddFieldButton.razor
        │   │   │   │   └── PropertiesPanel.razor
        │   │   │   │       ├── BasicPropertiesSection.razor
        │   │   │   │       ├── TypeConfigSection.razor (dynamic based on type)
        │   │   │   │       ├── ValidationRulesSection.razor
        │   │   │   │       │   └── ValidationRuleEditor.razor
        │   │   │   │       └── ConditionalRulesSection.razor
        │   │   │   │           └── ConditionalRuleEditor.razor
        │   │   │   ├── [Preview Tab]
        │   │   │   │   └── DynamicFormRenderer.razor
        │   │   │   └── [JSON Tab]
        │   │   │       └── JsonViewer.razor
        │   │   └── EditorStatusBar.razor
        │   │
        │   └── WorkflowEditor.razor ⭐ Workflow editor page
        │       ├── WorkflowToolbar.razor
        │       ├── WorkflowModuleList.razor
        │       │   └── WorkflowModuleItem.razor
        │       └── ConditionalBranchEditor.razor
        │
        └── Preview/
            └── FormPreview.razor (standalone preview)
```

---

## Page Components

### 1. ModuleEditor.razor

**Purpose**: Main editor page for creating/editing a single form module.

**File**: `Pages/Editor/ModuleEditor.razor`

```razor
@page "/editor/module/{ModuleId:int?}"
@using DynamicForms.Editor.Services
@using DynamicForms.Core.V2.Schemas
@inject EditorStateService StateService
@inject UndoRedoService UndoRedoService
@inject AutoSaveService AutoSaveService
@inject NavigationManager Navigation
@implements IDisposable

<PageTitle>@GetPageTitle()</PageTitle>

<div class="module-editor">
    <!-- Toolbar -->
    <EditorToolbar
        CanUndo="@UndoRedoService.CanUndo"
        CanRedo="@UndoRedoService.CanRedo"
        IsDirty="@StateService.IsDirty"
        OnSave="HandleSaveAsync"
        OnPublish="HandlePublishAsync"
        OnUndo="HandleUndoAsync"
        OnRedo="HandleRedoAsync"
        OnImport="HandleImportAsync"
        OnExport="HandleExportAsync" />

    <!-- Tabbed content area -->
    <EditorTabs
        ActiveTab="@_activeTab"
        OnTabChanged="HandleTabChanged">

        <EditorTabContent>
            <div class="editor-content">
                <!-- Left: Field List -->
                <FieldListPanel
                    Fields="@GetFieldList()"
                    SelectedFieldId="@_selectedFieldId"
                    OnFieldSelected="HandleFieldSelected"
                    OnFieldMoveUp="HandleFieldMoveUpAsync"
                    OnFieldMoveDown="HandleFieldMoveDownAsync"
                    OnFieldEdit="HandleFieldEdit"
                    OnFieldClone="HandleFieldCloneAsync"
                    OnFieldDelete="HandleFieldDeleteAsync"
                    OnAddField="HandleAddFieldAsync" />

                <!-- Right: Properties Panel -->
                <PropertiesPanel
                    Field="@GetSelectedField()"
                    OnFieldChanged="HandleFieldChangedAsync"
                    OnCancel="HandleCancelEdit" />
            </div>
        </EditorTabContent>

        <PreviewTabContent>
            <FormPreview ModuleSchema="@StateService.CurrentModule" />
        </PreviewTabContent>

        <JsonTabContent>
            <JsonViewer Json="@GetModuleJson()" />
        </JsonTabContent>

    </EditorTabs>

    <!-- Status Bar -->
    <EditorStatusBar
        LastSaved="@AutoSaveService.LastAutoSave"
        LastModified="@StateService.LastModified"
        IsDirty="@StateService.IsDirty"
        IsSaving="@AutoSaveService.IsSaving" />
</div>

@code {
    [Parameter] public int? ModuleId { get; set; }

    private string _activeTab = "editor";
    private string? _selectedFieldId;

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to state changes
        StateService.StateChanged += OnStateChanged;
        UndoRedoService.StackChanged += OnStackChanged;
        AutoSaveService.AutoSaveCompleted += OnAutoSaveCompleted;

        // Load existing module or create new
        if (ModuleId.HasValue)
        {
            var module = await LoadModuleAsync(ModuleId.Value);
            if (module != null)
            {
                StateService.LoadModule(module);
            }
        }
        else
        {
            // Create new blank module
            var newModule = FormModuleSchema.Create(0, "New Form");
            StateService.LoadModule(newModule);
        }

        // Start auto-save
        AutoSaveService.Start();
    }

    private async Task HandleSaveAsync()
    {
        await AutoSaveService.SaveNowAsync();
        // Show toast notification
    }

    private async Task HandlePublishAsync()
    {
        // Validate before publish
        var validationResult = await ValidateModuleAsync();

        if (!validationResult.IsValid)
        {
            // Show validation errors
            return;
        }

        // Confirm publish
        var confirmed = await ShowConfirmDialogAsync("Publish this form?");
        if (!confirmed) return;

        // Publish
        await PublishModuleAsync();

        // Show success message
    }

    private async Task HandleUndoAsync()
    {
        var snapshot = UndoRedoService.Undo();
        if (snapshot != null)
        {
            var module = DeserializeSnapshot(snapshot);
            StateService.LoadModule(module);
        }
    }

    private async Task HandleRedoAsync()
    {
        var snapshot = UndoRedoService.Redo();
        if (snapshot != null)
        {
            var module = DeserializeSnapshot(snapshot);
            StateService.LoadModule(module);
        }
    }

    private async Task HandleFieldChangedAsync(FormFieldSchema updatedField)
    {
        var currentModule = StateService.CurrentModule!;

        // Create new module with updated field
        var updatedModule = UpdateFieldInModule(currentModule, updatedField);

        // Update state (this creates undo snapshot)
        StateService.UpdateModule(updatedModule, $"Updated field '{updatedField.Label}'");

        await InvokeAsync(StateHasChanged);
    }

    private List<FormFieldSchema> GetFieldList()
    {
        return StateService.CurrentModule?.Fields ?? new List<FormFieldSchema>();
    }

    private FormFieldSchema? GetSelectedField()
    {
        if (string.IsNullOrEmpty(_selectedFieldId))
            return null;

        return StateService.CurrentModule?.Fields
            .FirstOrDefault(f => f.FieldId == _selectedFieldId);
    }

    private string GetPageTitle()
    {
        var title = StateService.CurrentModule?.Title ?? "New Form";
        return $"{title} - Form Editor";
    }

    private string GetModuleJson()
    {
        if (StateService.CurrentModule == null)
            return "{}";

        return JsonSerializer.Serialize(
            StateService.CurrentModule,
            new JsonSerializerOptions { WriteIndented = true });
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnStackChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnAutoSaveCompleted(object? sender, AutoSaveEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        StateService.StateChanged -= OnStateChanged;
        UndoRedoService.StackChanged -= OnStackChanged;
        AutoSaveService.AutoSaveCompleted -= OnAutoSaveCompleted;
        AutoSaveService.Stop();
    }

    // Helper methods omitted for brevity
}
```

---

## Editor Components

### 2. FieldListPanel.razor

**Purpose**: Displays hierarchical list of fields with action buttons.

**File**: `Components/FieldList/FieldListPanel.razor`

```razor
<div class="field-list-panel">
    <div class="panel-header">
        <h5>Fields</h5>
        <span class="field-count">@Fields.Count total</span>
    </div>

    <div class="field-list-scroll">
        @if (Fields.Any())
        {
            @foreach (var field in GetRootFields())
            {
                <FieldListItem
                    Field="@field"
                    AllFields="@Fields"
                    SelectedFieldId="@SelectedFieldId"
                    OnSelected="@OnFieldSelected"
                    OnMoveUp="@OnFieldMoveUp"
                    OnMoveDown="@OnFieldMoveDown"
                    OnEdit="@OnFieldEdit"
                    OnClone="@OnFieldClone"
                    OnDelete="@OnFieldDelete" />
            }
        }
        else
        {
            <div class="empty-state">
                <p class="text-muted">No fields yet. Click "Add Field" to start.</p>
            </div>
        }
    </div>

    <div class="panel-footer">
        <AddFieldButton OnAddField="@OnAddField" />
    </div>
</div>

@code {
    [Parameter] public List<FormFieldSchema> Fields { get; set; } = new();
    [Parameter] public string? SelectedFieldId { get; set; }
    [Parameter] public EventCallback<string> OnFieldSelected { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldMoveUp { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldMoveDown { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldEdit { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldClone { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldDelete { get; set; }
    [Parameter] public EventCallback OnAddField { get; set; }

    private List<FormFieldSchema> GetRootFields()
    {
        return Fields.Where(f => f.ParentId == null).ToList();
    }
}
```

**CSS**: `wwwroot/css/field-list-panel.css`

```css
.field-list-panel {
    display: flex;
    flex-direction: column;
    height: 100%;
    border-right: 1px solid #dee2e6;
    background-color: #f8f9fa;
}

.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px;
    border-bottom: 1px solid #dee2e6;
    background-color: white;
}

.panel-header h5 {
    margin: 0;
    font-size: 16px;
    font-weight: 600;
}

.field-count {
    font-size: 12px;
    color: #6c757d;
}

.field-list-scroll {
    flex: 1;
    overflow-y: auto;
    padding: 8px;
}

.empty-state {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    padding: 32px;
    text-align: center;
}

.panel-footer {
    padding: 16px;
    border-top: 1px solid #dee2e6;
    background-color: white;
}
```

### 3. FieldListItem.razor (Recursive)

**Purpose**: Individual field item with hierarchy support and action buttons.

**File**: `Components/FieldList/FieldListItem.razor`

```razor
<div class="field-list-item @GetCssClasses()" @onclick="HandleClick">
    <div class="field-content" style="padding-left: @GetIndentPx()px">
        <!-- Expand/collapse icon for containers -->
        @if (HasChildren())
        {
            <button class="expand-toggle" @onclick="ToggleExpand" @onclick:stopPropagation="true">
                <i class="bi bi-@(IsExpanded ? "chevron-down" : "chevron-right")"></i>
            </button>
        }

        <!-- Field icon -->
        <span class="field-icon">@GetFieldIcon()</span>

        <!-- Field label & ID -->
        <div class="field-info">
            <span class="field-label">@Field.Label</span>
            <span class="field-id">@Field.FieldId</span>
        </div>

        <!-- Action buttons -->
        <div class="field-actions" @onclick:stopPropagation="true">
            <button class="btn-icon" @onclick="() => OnMoveUp.InvokeAsync(Field)"
                    disabled="@IsFirst()" title="Move Up">
                <i class="bi bi-arrow-up"></i>
            </button>

            <button class="btn-icon" @onclick="() => OnMoveDown.InvokeAsync(Field)"
                    disabled="@IsLast()" title="Move Down">
                <i class="bi bi-arrow-down"></i>
            </button>

            <button class="btn-icon" @onclick="() => OnEdit.InvokeAsync(Field)" title="Edit">
                <i class="bi bi-pencil"></i>
            </button>

            <button class="btn-icon" @onclick="() => OnClone.InvokeAsync(Field)" title="Clone">
                <i class="bi bi-files"></i>
            </button>

            <button class="btn-icon btn-danger" @onclick="() => OnDelete.InvokeAsync(Field)" title="Delete">
                <i class="bi bi-trash"></i>
            </button>
        </div>
    </div>

    <!-- Render children recursively -->
    @if (HasChildren() && IsExpanded)
    {
        <div class="field-children">
            @foreach (var child in GetChildren())
            {
                <FieldListItem
                    Field="@child"
                    AllFields="@AllFields"
                    SelectedFieldId="@SelectedFieldId"
                    OnSelected="@OnSelected"
                    OnMoveUp="@OnMoveUp"
                    OnMoveDown="@OnMoveDown"
                    OnEdit="@OnEdit"
                    OnClone="@OnClone"
                    OnDelete="@OnDelete" />
            }
        </div>
    }
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = null!;
    [Parameter] public List<FormFieldSchema> AllFields { get; set; } = new();
    [Parameter] public string? SelectedFieldId { get; set; }
    [Parameter] public EventCallback<string> OnSelected { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnMoveUp { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnMoveDown { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnEdit { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnClone { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnDelete { get; set; }

    private bool IsExpanded { get; set; } = true;
    private int Level => CalculateLevel();

    private void HandleClick()
    {
        OnSelected.InvokeAsync(Field.FieldId);
    }

    private void ToggleExpand()
    {
        IsExpanded = !IsExpanded;
    }

    private bool HasChildren()
    {
        return AllFields.Any(f => f.ParentId == Field.FieldId);
    }

    private List<FormFieldSchema> GetChildren()
    {
        return AllFields.Where(f => f.ParentId == Field.FieldId).ToList();
    }

    private int CalculateLevel()
    {
        int level = 0;
        var current = Field;

        while (current.ParentId != null)
        {
            level++;
            current = AllFields.FirstOrDefault(f => f.FieldId == current.ParentId);
            if (current == null) break;
        }

        return level;
    }

    private int GetIndentPx() => Level * 20;

    private string GetCssClasses()
    {
        var classes = new List<string> { "field-item" };

        if (Field.FieldId == SelectedFieldId)
            classes.Add("selected");

        if (HasChildren())
            classes.Add("has-children");

        return string.Join(" ", classes);
    }

    private string GetFieldIcon()
    {
        return Field.FieldType switch
        {
            "Section" => "📁",
            "Tab" => "📑",
            "Panel" => "📋",
            "TextBox" => "📝",
            "DropDown" => "⬇️",
            "DatePicker" => "📅",
            "FileUpload" => "📎",
            "CheckBox" => "☑️",
            "RadioButtonList" => "🔘",
            "ModalTable" => "📊",
            _ => "❓"
        };
    }

    private bool IsFirst()
    {
        var siblings = GetSiblings();
        return siblings.FirstOrDefault()?.FieldId == Field.FieldId;
    }

    private bool IsLast()
    {
        var siblings = GetSiblings();
        return siblings.LastOrDefault()?.FieldId == Field.FieldId;
    }

    private List<FormFieldSchema> GetSiblings()
    {
        return AllFields.Where(f => f.ParentId == Field.ParentId).ToList();
    }
}
```

**CSS**: `wwwroot/css/field-list-item.css`

```css
.field-list-item {
    margin-bottom: 4px;
}

.field-item {
    display: flex;
    align-items: center;
    padding: 8px;
    background-color: white;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s ease;
}

.field-item:hover {
    background-color: #e9ecef;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}

.field-item.selected {
    background-color: #e7f3ff;
    border-color: #0d6efd;
}

.field-content {
    display: flex;
    align-items: center;
    width: 100%;
    gap: 8px;
}

.expand-toggle {
    background: none;
    border: none;
    padding: 4px;
    cursor: pointer;
    color: #6c757d;
    font-size: 12px;
}

.field-icon {
    font-size: 18px;
    flex-shrink: 0;
}

.field-info {
    display: flex;
    flex-direction: column;
    flex: 1;
    min-width: 0;
}

.field-label {
    font-size: 14px;
    font-weight: 500;
    color: #212529;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}

.field-id {
    font-size: 11px;
    color: #6c757d;
    font-family: 'Courier New', monospace;
}

.field-actions {
    display: flex;
    gap: 4px;
    opacity: 0;
    transition: opacity 0.2s ease;
}

.field-item:hover .field-actions,
.field-item.selected .field-actions {
    opacity: 1;
}

.btn-icon {
    background: none;
    border: 1px solid #dee2e6;
    border-radius: 3px;
    padding: 4px 6px;
    cursor: pointer;
    color: #495057;
    font-size: 12px;
    transition: all 0.15s ease;
}

.btn-icon:hover:not(:disabled) {
    background-color: #f8f9fa;
    border-color: #adb5bd;
}

.btn-icon:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}

.btn-icon.btn-danger {
    color: #dc3545;
}

.btn-icon.btn-danger:hover:not(:disabled) {
    background-color: #fff5f5;
    border-color: #dc3545;
}

.field-children {
    margin-top: 4px;
    border-left: 2px dotted #dee2e6;
    margin-left: 20px;
}
```

### 4. PropertiesPanel.razor

**Purpose**: Displays and edits properties of selected field.

**File**: `Components/Properties/PropertiesPanel.razor`

```razor
<div class="properties-panel">
    @if (Field != null)
    {
        <div class="panel-header">
            <h5>Field Properties</h5>
            <button class="btn btn-sm btn-link" @onclick="OnCancel">✕</button>
        </div>

        <div class="panel-content">
            <!-- Basic Properties -->
            <BasicPropertiesSection
                Field="@_workingField"
                OnFieldChanged="HandleFieldChanged" />

            <!-- Type-specific configuration -->
            <TypeConfigSection
                Field="@_workingField"
                OnFieldChanged="HandleFieldChanged" />

            <!-- Validation Rules -->
            <ValidationRulesSection
                Field="@_workingField"
                OnRulesChanged="HandleValidationRulesChanged" />

            <!-- Conditional Rules -->
            <ConditionalRulesSection
                Field="@_workingField"
                AllFields="@GetAvailableFields()"
                OnRulesChanged="HandleConditionalRulesChanged" />
        </div>

        <div class="panel-footer">
            <button class="btn btn-primary" @onclick="HandleApply">Apply Changes</button>
            <button class="btn btn-secondary" @onclick="OnCancel">Cancel</button>
        </div>
    }
    else
    {
        <div class="empty-state">
            <i class="bi bi-inbox" style="font-size: 48px; color: #adb5bd;"></i>
            <p class="text-muted mt-3">Select a field to edit its properties</p>
        </div>
    }
</div>

@code {
    [Parameter] public FormFieldSchema? Field { get; set; }
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private FormFieldSchema? _workingField;

    protected override void OnParametersSet()
    {
        // Create a working copy to avoid mutating the original
        if (Field != null)
        {
            _workingField = CloneField(Field);
        }
        else
        {
            _workingField = null;
        }
    }

    private void HandleFieldChanged(FormFieldSchema updatedField)
    {
        _workingField = updatedField;
    }

    private async Task HandleApply()
    {
        if (_workingField != null)
        {
            await OnFieldChanged.InvokeAsync(_workingField);
        }
    }

    private FormFieldSchema CloneField(FormFieldSchema field)
    {
        // Deep clone via JSON serialization
        var json = JsonSerializer.Serialize(field);
        return JsonSerializer.Deserialize<FormFieldSchema>(json)!;
    }

    private List<FormFieldSchema> GetAvailableFields()
    {
        // Get all fields from parent context
        // Implementation depends on how you pass the full field list
        return new List<FormFieldSchema>();
    }
}
```

### 5. BasicPropertiesSection.razor

**Purpose**: Edit basic field properties (ID, labels, type, etc.)

**File**: `Components/Properties/BasicPropertiesSection.razor`

```razor
<div class="properties-section">
    <h6>Basic Properties</h6>

    <div class="form-group">
        <label for="field-id">Field ID <span class="text-danger">*</span></label>
        <input type="text" id="field-id" class="form-control"
               @bind="@Field.FieldId"
               @bind:event="oninput"
               @onblur="NotifyChanged"
               placeholder="e.g., email_address" />
        <small class="form-text text-muted">
            Unique identifier (letters, numbers, underscores only)
        </small>
    </div>

    <div class="form-group">
        <label for="field-label-en">Label (English) <span class="text-danger">*</span></label>
        <input type="text" id="field-label-en" class="form-control"
               @bind="@Field.Label"
               @bind:event="oninput"
               @onblur="NotifyChanged"
               placeholder="e.g., Email Address" />
    </div>

    <div class="form-group">
        <label for="field-label-fr">Label (French)</label>
        <input type="text" id="field-label-fr" class="form-control"
               @bind="@Field.LabelFr"
               @bind:event="oninput"
               @onblur="NotifyChanged"
               placeholder="e.g., Adresse e-mail" />
    </div>

    <div class="form-group">
        <label for="field-type">Field Type <span class="text-danger">*</span></label>
        <select id="field-type" class="form-select"
                @bind="@Field.FieldType"
                @onchange="HandleTypeChanged">
            <option value="TextBox">Text Box</option>
            <option value="TextArea">Text Area</option>
            <option value="DropDown">Drop Down</option>
            <option value="DatePicker">Date Picker</option>
            <option value="DateRangePicker">Date Range Picker</option>
            <option value="FileUpload">File Upload</option>
            <option value="CheckBox">Check Box</option>
            <option value="RadioButtonList">Radio Button List</option>
            <option value="Section">Section (Container)</option>
            <option value="Tab">Tab (Container)</option>
            <option value="Panel">Panel (Container)</option>
            <option value="ModalTable">Modal Table</option>
        </select>
    </div>

    <div class="form-group">
        <label for="field-description-en">Description (English)</label>
        <textarea id="field-description-en" class="form-control" rows="2"
                  @bind="@Field.Description"
                  @bind:event="oninput"
                  @onblur="NotifyChanged"
                  placeholder="Optional help text"></textarea>
    </div>

    <div class="form-group">
        <label for="field-description-fr">Description (French)</label>
        <textarea id="field-description-fr" class="form-control" rows="2"
                  @bind="@Field.DescriptionFr"
                  @bind:event="oninput"
                  @onblur="NotifyChanged"
                  placeholder="Optional help text"></textarea>
    </div>

    <div class="form-group">
        <div class="form-check">
            <input type="checkbox" id="field-enabled" class="form-check-input"
                   @bind="@Field.IsEnabled"
                   @onchange="NotifyChanged" />
            <label for="field-enabled" class="form-check-label">
                Enabled (field is active)
            </label>
        </div>
    </div>
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = null!;
    [Parameter] public EventCallback<FormFieldSchema> OnFieldChanged { get; set; }

    private async Task NotifyChanged()
    {
        await OnFieldChanged.InvokeAsync(Field);
    }

    private async Task HandleTypeChanged(ChangeEventArgs e)
    {
        // When type changes, reset type-specific config
        Field = Field with
        {
            FieldType = e.Value?.ToString() ?? "TextBox",
            TypeConfig = null  // Clear old config
        };

        await NotifyChanged();
    }
}
```

### 6. ValidationRulesSection.razor

**Purpose**: Manage validation rules for a field.

**File**: `Components/Properties/ValidationRulesSection.razor`

```razor
<div class="properties-section">
    <h6>Validation Rules</h6>

    @if (Field.ValidationRules.Any())
    {
        <div class="rules-list">
            @foreach (var rule in Field.ValidationRules)
            {
                <ValidationRuleEditor
                    Rule="@rule"
                    OnRuleChanged="HandleRuleChanged"
                    OnRemove="() => HandleRemoveRule(rule)" />
            }
        </div>
    }
    else
    {
        <p class="text-muted small">No validation rules defined.</p>
    }

    <button class="btn btn-sm btn-outline-primary mt-2" @onclick="HandleAddRule">
        <i class="bi bi-plus-circle"></i> Add Validation Rule
    </button>
</div>

@code {
    [Parameter] public FormFieldSchema Field { get; set; } = null!;
    [Parameter] public EventCallback<List<ValidationRule>> OnRulesChanged { get; set; }

    private async Task HandleAddRule()
    {
        var newRule = new ValidationRule
        {
            RuleType = "required",
            ErrorMessage = "This field is required"
        };

        var updatedRules = Field.ValidationRules.ToList();
        updatedRules.Add(newRule);

        await OnRulesChanged.InvokeAsync(updatedRules);
    }

    private async Task HandleRemoveRule(ValidationRule rule)
    {
        var updatedRules = Field.ValidationRules.Where(r => r != rule).ToList();
        await OnRulesChanged.InvokeAsync(updatedRules);
    }

    private async Task HandleRuleChanged(ValidationRule updatedRule)
    {
        var updatedRules = Field.ValidationRules.Select(r =>
            r == updatedRule ? updatedRule : r
        ).ToList();

        await OnRulesChanged.InvokeAsync(updatedRules);
    }
}
```

### 7. ValidationRuleEditor.razor

**Purpose**: Edit a single validation rule inline.

**File**: `Components/Properties/ValidationRuleEditor.razor`

```razor
<div class="validation-rule-editor">
    <div class="rule-row">
        <select class="form-select form-select-sm" style="width: 150px;"
                @bind="@Rule.RuleType"
                @onchange="HandleRuleTypeChanged">
            <option value="required">Required</option>
            <option value="minLength">Min Length</option>
            <option value="maxLength">Max Length</option>
            <option value="pattern">Pattern (Regex)</option>
            <option value="email">Email</option>
            <option value="phone">Phone</option>
            <option value="url">URL</option>
            <option value="number">Number</option>
            <option value="range">Range</option>
        </select>

        @if (RequiresParameter())
        {
            <input type="text" class="form-control form-control-sm" style="width: 120px;"
                   @bind="@Rule.Parameter"
                   @bind:event="oninput"
                   @onblur="NotifyChanged"
                   placeholder="@GetParameterPlaceholder()" />
        }

        <input type="text" class="form-control form-control-sm flex-grow-1"
               @bind="@Rule.ErrorMessage"
               @bind:event="oninput"
               @onblur="NotifyChanged"
               placeholder="Error message" />

        <button class="btn btn-sm btn-outline-danger" @onclick="OnRemove" title="Remove">
            <i class="bi bi-trash"></i>
        </button>
    </div>
</div>

@code {
    [Parameter] public ValidationRule Rule { get; set; } = null!;
    [Parameter] public EventCallback<ValidationRule> OnRuleChanged { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }

    private bool RequiresParameter()
    {
        return Rule.RuleType is "minLength" or "maxLength" or "pattern" or "range";
    }

    private string GetParameterPlaceholder()
    {
        return Rule.RuleType switch
        {
            "minLength" => "Min length",
            "maxLength" => "Max length",
            "pattern" => "Regex pattern",
            "range" => "Min,Max",
            _ => "Value"
        };
    }

    private async Task HandleRuleTypeChanged(ChangeEventArgs e)
    {
        Rule.RuleType = e.Value?.ToString() ?? "required";
        Rule.ErrorMessage = GetDefaultErrorMessage(Rule.RuleType);
        await NotifyChanged();
    }

    private async Task NotifyChanged()
    {
        await OnRuleChanged.InvokeAsync(Rule);
    }

    private string GetDefaultErrorMessage(string ruleType)
    {
        return ruleType switch
        {
            "required" => "This field is required",
            "minLength" => "Minimum length not met",
            "maxLength" => "Maximum length exceeded",
            "pattern" => "Invalid format",
            "email" => "Invalid email address",
            "phone" => "Invalid phone number",
            "url" => "Invalid URL",
            "number" => "Must be a number",
            "range" => "Value out of range",
            _ => "Validation failed"
        };
    }
}
```

**CSS**: `wwwroot/css/validation-rule-editor.css`

```css
.validation-rule-editor {
    margin-bottom: 8px;
}

.rule-row {
    display: flex;
    gap: 8px;
    align-items: center;
    padding: 8px;
    background-color: #f8f9fa;
    border: 1px solid #dee2e6;
    border-radius: 4px;
}

.rule-row:hover {
    background-color: #e9ecef;
}
```

---

## Renderer Components

### 8. DynamicFormRenderer.razor

**Purpose**: Main renderer component that loads schema and renders form.

**File**: `DynamicForms.Renderer/Components/DynamicFormRenderer.razor`

```razor
@using DynamicForms.Core.V2.Schemas
@using DynamicForms.Core.V2.Services
@using DynamicForms.Renderer.Models
@inject IFormHierarchyService HierarchyService
@inject IFormValidationService ValidationService

<div class="dynamic-form-renderer @CssClass">
    <CascadingValue Value="this">
        @if (_runtime != null)
        {
            <EditForm Model="@_formData" OnValidSubmit="HandleSubmit">
                <!-- Render root fields -->
                @foreach (var rootNode in _runtime.RootNodes)
                {
                    <DynamicFieldRenderer
                        Node="@rootNode"
                        FormData="@_formData"
                        ValidationErrors="@_validationErrors"
                        OnValueChanged="HandleFieldValueChanged" />
                }

                @if (!ReadOnly)
                {
                    <div class="form-actions mt-4">
                        <button type="submit" class="btn btn-primary">@SubmitButtonText</button>
                        @if (ShowCancelButton)
                        {
                            <button type="button" class="btn btn-secondary" @onclick="HandleCancel">
                                @CancelButtonText
                            </button>
                        }
                    </div>
                }
            </EditForm>
        }
        else
        {
            <p>Loading form...</p>
        }
    </CascadingValue>
</div>

@code {
    [Parameter] public FormModuleSchema Schema { get; set; } = null!;
    [Parameter] public FormData? InitialData { get; set; }
    [Parameter] public EventCallback<FormData> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public bool ReadOnly { get; set; } = false;
    [Parameter] public string CssClass { get; set; } = string.Empty;
    [Parameter] public string SubmitButtonText { get; set; } = "Submit";
    [Parameter] public string CancelButtonText { get; set; } = "Cancel";
    [Parameter] public bool ShowCancelButton { get; set; } = false;

    private FormModuleRuntime? _runtime;
    private FormData _formData = new();
    private Dictionary<string, List<string>> _validationErrors = new();

    protected override async Task OnParametersSetAsync()
    {
        // Build hierarchy
        _runtime = await HierarchyService.BuildHierarchyAsync(Schema);

        // Initialize form data
        _formData = InitialData ?? new FormData();

        await EvaluateConditionalLogicAsync();
    }

    private async Task HandleFieldValueChanged(string fieldId, object? value)
    {
        _formData.SetValue(fieldId, value);
        await EvaluateConditionalLogicAsync();
        await ValidateFieldAsync(fieldId);
        StateHasChanged();
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

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }

    private Task EvaluateConditionalLogicAsync()
    {
        // Evaluate conditional rules for all fields
        // Update field visibility/enabled state
        return Task.CompletedTask;
    }

    private Task ValidateFieldAsync(string fieldId)
    {
        // Validate single field
        return Task.CompletedTask;
    }

    public FormData GetFormData() => _formData;
}
```

### 9. DynamicFieldRenderer.razor

**Purpose**: Renders a single field based on its type (routing component).

**File**: `DynamicForms.Renderer/Components/DynamicFieldRenderer.razor`

```razor
@using DynamicForms.Core.V2.Runtime
@using DynamicForms.Renderer.Components.Fields

@if (ShouldRender())
{
    <div class="field-wrapper" data-field-id="@Node.Field.FieldId">
        @switch (Node.Field.FieldType)
        {
            case "TextBox":
                <TextFieldRenderer
                    Node="@Node"
                    Value="@GetValue()"
                    Errors="@GetErrors()"
                    OnValueChanged="@HandleValueChanged"
                    IsDisabled="@IsDisabled()" />
                break;

            case "DropDown":
                <DropDownRenderer
                    Node="@Node"
                    Value="@GetValue()"
                    Errors="@GetErrors()"
                    OnValueChanged="@HandleValueChanged"
                    IsDisabled="@IsDisabled()" />
                break;

            case "DatePicker":
                <DatePickerRenderer
                    Node="@Node"
                    Value="@GetValue()"
                    Errors="@GetErrors()"
                    OnValueChanged="@HandleValueChanged"
                    IsDisabled="@IsDisabled()" />
                break;

            case "Section":
                <SectionRenderer
                    Node="@Node"
                    FormData="@FormData"
                    ValidationErrors="@ValidationErrors"
                    OnValueChanged="@OnValueChanged" />
                break;

            // ... other field types

            default:
                <div class="alert alert-warning">
                    Unknown field type: @Node.Field.FieldType
                </div>
                break;
        }
    </div>
}

@code {
    [Parameter] public FormFieldNode Node { get; set; } = null!;
    [Parameter] public FormData FormData { get; set; } = null!;
    [Parameter] public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
    [Parameter] public EventCallback<(string fieldId, object? value)> OnValueChanged { get; set; }

    private bool ShouldRender()
    {
        // Check conditional logic
        // For now, always render
        return true;
    }

    private object? GetValue()
    {
        return FormData.GetValue(Node.Field.FieldId);
    }

    private List<string> GetErrors()
    {
        return ValidationErrors.TryGetValue(Node.Field.FieldId, out var errors)
            ? errors
            : new List<string>();
    }

    private bool IsDisabled()
    {
        return !Node.Field.IsEnabled;
    }

    private async Task HandleValueChanged(object? value)
    {
        await OnValueChanged.InvokeAsync((Node.Field.FieldId, value));
    }
}
```

### 10. TextFieldRenderer.razor

**Purpose**: Renders a text input field.

**File**: `DynamicForms.Renderer/Components/Fields/TextFieldRenderer.razor`

```razor
@using DynamicForms.Core.V2.Runtime
@inherits FieldRendererBase

<div class="form-group mb-3">
    <label for="@FieldId" class="form-label">
        @GetLabel()
        @if (IsRequired)
        {
            <span class="text-danger">*</span>
        }
    </label>

    <input type="@GetInputType()"
           id="@FieldId"
           class="form-control @(HasErrors ? "is-invalid" : "")"
           value="@CurrentValue"
           @onchange="HandleValueChanged"
           disabled="@IsDisabled"
           maxlength="@GetMaxLength()"
           placeholder="@GetPlaceholder()" />

    @if (HasErrors)
    {
        <div class="invalid-feedback d-block">
            @foreach (var error in Errors)
            {
                <div>@error</div>
            }
        </div>
    }

    @if (!string.IsNullOrEmpty(GetHelpText()))
    {
        <small class="form-text text-muted">@GetHelpText()</small>
    }
</div>

@code {
    [Parameter] public FormFieldNode Node { get; set; } = null!;
    [Parameter] public object? Value { get; set; }
    [Parameter] public List<string> Errors { get; set; } = new();
    [Parameter] public EventCallback<object?> OnValueChanged { get; set; }
    [Parameter] public bool IsDisabled { get; set; }

    private string FieldId => Node.Field.FieldId;
    private string CurrentValue => Value?.ToString() ?? string.Empty;
    private bool HasErrors => Errors.Any();

    private bool IsRequired =>
        Node.Field.ValidationRules.Any(r => r.RuleType == "required");

    private string GetInputType()
    {
        // Check if TypeConfig has specific input type
        return "text";
    }

    private int GetMaxLength()
    {
        // Extract from TypeConfig if TextBoxConfig
        return 200;
    }

    private string GetPlaceholder()
    {
        // Extract from TypeConfig
        return string.Empty;
    }

    private string GetLabel()
    {
        // TODO: Localization
        return Node.Field.Label;
    }

    private string GetHelpText()
    {
        // TODO: Localization
        return Node.Field.Description ?? string.Empty;
    }

    private async Task HandleValueChanged(ChangeEventArgs e)
    {
        await OnValueChanged.InvokeAsync(e.Value?.ToString());
    }
}
```

---

## Shared Components

### 11. EditorToolbar.razor

**Purpose**: Top toolbar with save, publish, undo/redo actions.

**File**: `Components/Shared/EditorToolbar.razor`

```razor
<div class="editor-toolbar">
    <div class="toolbar-section">
        <button class="btn btn-primary" @onclick="OnSave" disabled="@(!IsDirty)">
            <i class="bi bi-save"></i> Save
        </button>

        <button class="btn btn-success" @onclick="OnPublish">
            <i class="bi bi-cloud-upload"></i> Publish
        </button>
    </div>

    <div class="toolbar-section">
        <button class="btn btn-outline-secondary" @onclick="OnUndo" disabled="@(!CanUndo)" title="Undo">
            <i class="bi bi-arrow-counterclockwise"></i>
        </button>

        <button class="btn btn-outline-secondary" @onclick="OnRedo" disabled="@(!CanRedo)" title="Redo">
            <i class="bi bi-arrow-clockwise"></i>
        </button>
    </div>

    <div class="toolbar-section">
        <button class="btn btn-outline-secondary" @onclick="OnImport">
            <i class="bi bi-upload"></i> Import
        </button>

        <button class="btn btn-outline-secondary" @onclick="OnExport">
            <i class="bi bi-download"></i> Export
        </button>
    </div>
</div>

@code {
    [Parameter] public bool CanUndo { get; set; }
    [Parameter] public bool CanRedo { get; set; }
    [Parameter] public bool IsDirty { get; set; }
    [Parameter] public EventCallback OnSave { get; set; }
    [Parameter] public EventCallback OnPublish { get; set; }
    [Parameter] public EventCallback OnUndo { get; set; }
    [Parameter] public EventCallback OnRedo { get; set; }
    [Parameter] public EventCallback OnImport { get; set; }
    [Parameter] public EventCallback OnExport { get; set; }
}
```

**CSS**: `wwwroot/css/editor-toolbar.css`

```css
.editor-toolbar {
    display: flex;
    align-items: center;
    gap: 24px;
    padding: 12px 16px;
    background-color: white;
    border-bottom: 1px solid #dee2e6;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.toolbar-section {
    display: flex;
    gap: 8px;
}

.editor-toolbar .btn {
    font-size: 14px;
}

.editor-toolbar .bi {
    margin-right: 4px;
}
```

---

## Event Handling Patterns

### Pattern 1: Field Value Change with Undo Support

```csharp
// In ModuleEditor.razor
private async Task HandleFieldValueChanged(FormFieldSchema updatedField)
{
    // 1. Get current module
    var currentModule = StateService.CurrentModule!;

    // 2. Create updated module with new field
    var updatedModule = UpdateFieldInModule(currentModule, updatedField);

    // 3. Update state (creates undo snapshot automatically)
    StateService.UpdateModule(
        updatedModule,
        $"Updated field '{updatedField.Label}'");

    // 4. UI will re-render automatically via StateChanged event
}

private FormModuleSchema UpdateFieldInModule(
    FormModuleSchema module,
    FormFieldSchema updatedField)
{
    var updatedFields = module.Fields
        .Select(f => f.FieldId == updatedField.FieldId ? updatedField : f)
        .ToList();

    return module with { Fields = updatedFields };
}
```

### Pattern 2: Debounced Auto-Save

```csharp
// In AutoSaveService.cs
private Timer? _debounceTimer;

public void TriggerAutoSave()
{
    _debounceTimer?.Dispose();

    _debounceTimer = new Timer(async _ =>
    {
        await PerformAutoSaveAsync();
    }, null, TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
}
```

---

## Styling Guide

### CSS Variables (Custom Properties)

```css
:root {
    /* Colors */
    --primary-color: #0d6efd;
    --secondary-color: #6c757d;
    --success-color: #198754;
    --danger-color: #dc3545;
    --warning-color: #ffc107;
    --info-color: #0dcaf0;

    /* Grays */
    --gray-100: #f8f9fa;
    --gray-200: #e9ecef;
    --gray-300: #dee2e6;
    --gray-400: #ced4da;
    --gray-500: #adb5bd;
    --gray-600: #6c757d;
    --gray-700: #495057;
    --gray-800: #343a40;
    --gray-900: #212529;

    /* Spacing */
    --spacing-xs: 4px;
    --spacing-sm: 8px;
    --spacing-md: 16px;
    --spacing-lg: 24px;
    --spacing-xl: 32px;

    /* Border radius */
    --border-radius-sm: 3px;
    --border-radius: 4px;
    --border-radius-lg: 8px;

    /* Shadows */
    --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
    --shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
    --shadow-lg: 0 4px 8px rgba(0, 0, 0, 0.15);

    /* Transitions */
    --transition-fast: 150ms ease;
    --transition: 200ms ease;
    --transition-slow: 300ms ease;
}
```

---

**Document Version**: 1.0
**Last Updated**: January 2025
**Next Steps**: Implement components following this specification
