# Visual Form Editor - Implementation Prompts

## Overview

This document contains **step-by-step prompts** for implementing the Visual Form Editor for DynamicForms.Core.V2. Each prompt is designed to be copy-pasted directly to Claude for execution.

### Estimated Total Time: 12-16 weeks (240-320 hours)

### Prerequisites
- ✅ DynamicForms.Core.V2 implemented and working
- ✅ .NET 9.0 SDK installed
- ✅ SQL Server available (LocalDB or full instance)
- ✅ Visual Studio 2022 or Rider

---

## Implementation Phases

| Phase | Focus | Prompts | Estimated Time |
|-------|-------|---------|----------------|
| **Phase 1** | Database & Infrastructure | 1.1 - 1.5 | 2 weeks |
| **Phase 2** | Form Renderer Library | 2.1 - 2.8 | 3 weeks |
| **Phase 3** | Editor State Services | 3.1 - 3.6 | 2 weeks |
| **Phase 4** | Editor UI Components | 4.1 - 4.10 | 4 weeks |
| **Phase 5** | Workflow Editor | 5.1 - 5.4 | 2 weeks |
| **Phase 6** | Import/Export & Publish | 6.1 - 6.4 | 1 week |
| **Phase 7** | Testing | 7.1 - 7.5 | 2 weeks |
| **Phase 8** | Polish & Documentation | 8.1 - 8.3 | 1 week |
| **TOTAL** | | **45 prompts** | **~16 weeks** |

---

## Phase 1: Database & Infrastructure

### Prompt 1.1: Create SQL Server Database Schema

```
I need you to create the SQL Server database schema for the Visual Form Editor.

Requirements:
- Create a new SQL script: Src/Database/CreateEditorDatabase.sql
- Implement all tables from VISUAL_EDITOR_DESIGN_PROPOSAL.md database section:
  * EditorFormModules
  * EditorWorkflows
  * EditorHistory
  * PublishedFormModules
  * PublishedWorkflows
  * EditorConfiguration
- Include all indexes specified in the design
- Insert default configuration values
- Add helpful comments explaining each table's purpose

The script should be idempotent (safe to run multiple times).

Acceptance Criteria:
✓ SQL script creates database if not exists
✓ All 6 tables created with correct columns and types
✓ All indexes created
✓ Default configuration rows inserted
✓ Script has clear sections and comments
✓ Can run script successfully against SQL Server
```

### Prompt 1.2: Create Entity Framework Core DbContext

```
Create the Entity Framework Core DbContext and entity classes for the editor database.

Requirements:
- Create new project: Src/DynamicForms.Editor.Data/DynamicForms.Editor.Data.csproj
- Target: .NET 9.0
- Add NuGet packages:
  * Microsoft.EntityFrameworkCore.SqlServer (latest)
  * Microsoft.EntityFrameworkCore.Tools (latest)
- Create entity classes in Entities/ folder:
  * EditorFormModule.cs
  * EditorWorkflow.cs
  * EditorHistorySnapshot.cs
  * PublishedFormModule.cs
  * PublishedWorkflow.cs
  * EditorConfigurationItem.cs
- Create ApplicationDbContext.cs with:
  * DbSet properties for all entities
  * OnModelCreating with proper configurations (column types, indexes, relationships)
  * Connection string from configuration

Acceptance Criteria:
✓ Project builds successfully
✓ All entity classes have correct properties matching database schema
✓ DbContext configured with all DbSets
✓ Entity configurations match database (varchar lengths, required fields, etc.)
✓ Indexes defined in Fluent API
```

### Prompt 1.3: Create EF Core Migrations

```
Create Entity Framework Core migrations for the editor database.

Requirements:
- Add design-time DbContext factory for migrations
- Create initial migration: "InitialEditorDatabase"
- Verify migration matches the SQL script from Prompt 1.1
- Create a README.md in the Database/ folder explaining:
  * How to run migrations
  * How to create new migrations
  * Connection string configuration

Commands to run:
dotnet ef migrations add InitialEditorDatabase --project Src/DynamicForms.Editor.Data
dotnet ef database update --project Src/DynamicForms.Editor.Data

Acceptance Criteria:
✓ Migration files created
✓ Migration Up() method creates all tables correctly
✓ Migration Down() method drops all tables
✓ dotnet ef database update succeeds
✓ Database created with all tables and data
✓ README.md is clear and helpful
```

### Prompt 1.4: Create Repository Interfaces

```
Create repository interfaces for data access.

Requirements:
- Create Src/DynamicForms.Editor.Data/Repositories/ folder
- Create interfaces:
  * IEditorModuleRepository.cs - CRUD for draft modules
  * IPublishedModuleRepository.cs - Read-only access to published modules
  * IEditorHistoryRepository.cs - Save/retrieve undo/redo snapshots
  * IEditorConfigurationRepository.cs - Get/set configuration
- Each interface should have:
  * Standard CRUD methods (GetByIdAsync, SaveAsync, DeleteAsync, etc.)
  * Specific query methods (GetByStatusAsync, GetActiveVersionAsync, etc.)
  * XML documentation comments
- Follow async/await patterns
- Use CancellationToken parameters

Acceptance Criteria:
✓ 4 interface files created
✓ All methods have XML documentation
✓ Methods use Task<T> return types
✓ CancellationToken parameters included
✓ Interfaces are comprehensive and cover all use cases
```

### Prompt 1.5: Implement Repository Classes

```
Implement the repository interfaces using Entity Framework Core.

Requirements:
- Create implementations in Repositories/ folder:
  * EditorModuleRepository.cs
  * PublishedModuleRepository.cs
  * EditorHistoryRepository.cs
  * EditorConfigurationRepository.cs
- Inject ApplicationDbContext via constructor
- Implement all interface methods
- Use async/await properly
- Include error handling and logging
- Add XML documentation
- Follow repository pattern best practices

Special requirements:
- EditorModuleRepository: Include auto-increment version logic
- PublishedModuleRepository: Implement caching (MemoryCache) for performance
- EditorHistoryRepository: Implement cleanup of old snapshots (>90 days)
- EditorConfigurationRepository: Type-safe Get<T> methods

Acceptance Criteria:
✓ All 4 repositories implemented
✓ All interface methods implemented correctly
✓ Proper error handling
✓ Logging added with ILogger
✓ Code compiles without warnings
✓ Methods are well-tested (manual verification for now)
```

---

## Phase 2: Form Renderer Library

### Prompt 2.1: Create Renderer Project Structure

```
Create the DynamicForms.Renderer project for rendering forms from JSON schemas.

Requirements:
- Create new Blazor Class Library: Src/DynamicForms.Renderer/
- Target: .NET 9.0
- Add project references:
  * DynamicForms.Core.V2 (schemas and services)
- Add NuGet packages:
  * Microsoft.AspNetCore.Components.Web (latest)
- Create folder structure:
  * Components/
    - Fields/ (field renderers)
    - Containers/ (section, tab renderers)
  * Services/
  * Models/
  * wwwroot/css/
- Create _Imports.razor with common usings

Acceptance Criteria:
✓ Project created and builds successfully
✓ Folder structure created
✓ Project references added
✓ _Imports.razor has common namespaces
✓ Ready for component development
```

### Prompt 2.2: Create FormData and RenderContext Models

```
Create the FormData model for holding user input and RenderContext for runtime state.

Requirements:
- Create Models/FormData.cs:
  * Dictionary-based storage for field values
  * GetValue<T>(fieldId) method
  * SetValue(fieldId, value) method
  * ToJson() / FromJson() serialization
  * GetAllValues() method
- Create Models/RenderContext.cs:
  * FormModuleRuntime reference
  * FormData reference
  * Dictionary for field visibility state
  * Dictionary for field enabled state
  * Methods to check if field should be visible/enabled

Acceptance Criteria:
✓ FormData class is complete and functional
✓ Type-safe value retrieval works
✓ JSON serialization/deserialization works
✓ RenderContext tracks visibility and enabled state
✓ Code is well-documented with XML comments
```

### Prompt 2.3: Create Conditional Logic Engine

```
Create the ConditionalLogicEngine service to evaluate conditional rules.

Requirements:
- Create Services/ConditionalLogicEngine.cs
- Implement methods:
  * EvaluateCondition(ConditionalRule rule, FormData formData) -> bool
  * ShouldShowField(FormFieldSchema field, FormData formData) -> bool
  * IsFieldEnabled(FormFieldSchema field, FormData formData) -> bool
  * EvaluateAllConditions(FormModuleRuntime runtime, FormData formData) -> Dictionary<string, bool>
- Support all conditional operators:
  * Equals, NotEquals, Contains, GreaterThan, LessThan, IsEmpty, IsNotEmpty
- Handle different value types (string, int, decimal, DateTime, bool)
- Add comprehensive error handling

Acceptance Criteria:
✓ All conditional operators implemented correctly
✓ Type conversions handled properly
✓ Null values handled gracefully
✓ EvaluateAllConditions efficiently processes entire form
✓ Unit tests added for each operator (create test project if needed)
```

### Prompt 2.4: Create Base Field Renderer

```
Create a base class for all field renderers.

Requirements:
- Create Components/FieldRendererBase.cs
- Inherit from ComponentBase
- Define common parameters:
  * [Parameter] FormFieldNode Node
  * [Parameter] object? Value
  * [Parameter] List<string> Errors
  * [Parameter] EventCallback<object?> OnValueChanged
  * [Parameter] bool IsDisabled
- Add helper methods:
  * GetLabel() - with localization support
  * GetHelpText() - with localization support
  * IsRequired() - check validation rules
  * GetCssClasses() - build CSS class string
- Add localization support (check for French labels)

Acceptance Criteria:
✓ Base class is abstract and well-designed
✓ Common parameters defined
✓ Helper methods implemented
✓ Localization supported (EN/FR)
✓ Easy to inherit from for specific field types
```

### Prompt 2.5: Create Field Renderers (Text, TextArea, DropDown)

```
Create field renderers for TextBox, TextArea, and DropDown field types.

Requirements:
- Create Components/Fields/TextFieldRenderer.razor
  * Inherits from FieldRendererBase
  * Renders <input type="text">
  * Supports maxlength, placeholder from TypeConfig
  * Shows validation errors
  * Shows help text if provided
- Create Components/Fields/TextAreaRenderer.razor
  * Renders <textarea>
  * Supports rows, maxlength from TypeConfig
- Create Components/Fields/DropDownRenderer.razor
  * Renders <select> with options
  * Loads options from FieldOptions
  * Supports placeholder option ("-- Select --")
  * Handles EN/FR labels for options

Acceptance Criteria:
✓ All 3 renderers created
✓ Renderers follow Bootstrap 5 styling
✓ Validation errors displayed inline
✓ All TypeConfig properties respected
✓ Localization works for labels and help text
✓ OnValueChanged events fire correctly
```

### Prompt 2.6: Create Field Renderers (Date, File, Checkbox)

```
Create field renderers for DatePicker, FileUpload, and CheckBox field types.

Requirements:
- Create Components/Fields/DatePickerRenderer.razor
  * Renders <input type="date">
  * Supports min/max dates from TypeConfig
  * Handles date format properly
- Create Components/Fields/FileUploadRenderer.razor
  * Renders <input type="file">
  * Supports multiple files if configured
  * Shows allowed file types
  * Shows max file size
  * Displays selected file names
- Create Components/Fields/CheckBoxRenderer.razor
  * Renders <input type="checkbox">
  * Handles boolean value
  * Shows label next to checkbox (not above)

Acceptance Criteria:
✓ All 3 renderers created
✓ Date picker handles date conversion properly
✓ File upload shows file selection UI
✓ Checkbox follows proper Bootstrap styling
✓ All renderers integrate with FormData correctly
```

### Prompt 2.7: Create Container Renderers (Section, Tab, Panel)

```
Create container renderers for Section, Tab, and Panel field types.

Requirements:
- Create Components/Containers/SectionRenderer.razor
  * Renders children in a bordered section
  * Shows section title
  * Collapsible (optional)
  * Recursively renders child fields
- Create Components/Containers/TabRenderer.razor
  * Renders children in tab panels
  * Shows tab headers
  * Supports Bootstrap tabs
- Create Components/Containers/PanelRenderer.razor
  * Renders children in a panel with header
  * Similar to Section but different styling

All containers must:
- Recursively render child fields using DynamicFieldRenderer
- Pass FormData and validation errors down
- Support conditional visibility

Acceptance Criteria:
✓ All 3 container renderers created
✓ Recursive rendering works correctly
✓ Tabs are functional and styled with Bootstrap
✓ Section collapsing works (if implemented)
✓ Hierarchy is preserved visually
```

### Prompt 2.8: Create Main DynamicFormRenderer Component

```
Create the main DynamicFormRenderer component that ties everything together.

Requirements:
- Create Components/DynamicFormRenderer.razor
- Parameters:
  * FormModuleSchema Schema (required)
  * FormData? InitialData
  * EventCallback<FormData> OnSubmit
  * EventCallback OnCancel
  * bool ReadOnly = false
  * string CssClass
- Inject services:
  * IFormHierarchyService (from Core.V2)
  * IFormValidationService (from Core.V2)
  * ConditionalLogicEngine
- OnParametersSetAsync:
  * Build hierarchy from schema
  * Initialize FormData
  * Evaluate initial conditional logic
- Render:
  * Loop through root nodes
  * Use DynamicFieldRenderer for each
  * Show submit/cancel buttons if not ReadOnly
- Handle form submission with validation
- Create DynamicFieldRenderer.razor (routing component)
  * Switch on field type
  * Route to appropriate renderer
  * Handle visibility based on conditional logic

Acceptance Criteria:
✓ DynamicFormRenderer is complete and functional
✓ Form loads from schema correctly
✓ All field types render properly
✓ Conditional logic evaluated on load and field change
✓ Validation runs on submit
✓ Submit/Cancel events fire correctly
✓ Can render a complete form end-to-end
```

---

## Phase 3: Editor State Services

### Prompt 3.1: Create EditorStateService

```
Create the EditorStateService to manage current editing state.

Requirements:
- Create Src/DynamicForms.Editor/Services/State/EditorStateService.cs
- Properties:
  * Guid EditorSessionId
  * EditorEntityType EntityType (Module or Workflow enum)
  * FormModuleSchema? CurrentModule
  * FormWorkflowSchema? CurrentWorkflow
  * bool IsDirty
  * DateTime LastModified
  * DateTime? LastSaved
- Events:
  * event EventHandler? StateChanged
  * event EventHandler? ModuleChanged
  * event EventHandler? WorkflowChanged
- Methods:
  * LoadModule(FormModuleSchema module)
  * LoadWorkflow(FormWorkflowSchema workflow)
  * UpdateModule(FormModuleSchema module, string actionDescription)
  * UpdateWorkflow(FormWorkflowSchema workflow, string actionDescription)
  * FormModuleSchema? GetCurrentModule()
  * FormWorkflowSchema? GetCurrentWorkflow()
  * MarkAsSaved()
  * ResetSession()
- When updating, fire appropriate events and set IsDirty = true

Acceptance Criteria:
✓ Service class created with all properties and methods
✓ Events defined and fire correctly
✓ IsDirty tracking works
✓ Session ID generated on load
✓ Thread-safe if needed
✓ Well-documented with XML comments
```

### Prompt 3.2: Create UndoRedoService

```
Create the UndoRedoService to manage undo/redo functionality.

Requirements:
- Create Services/State/UndoRedoService.cs
- Use two stacks: _undoStack and _redoStack
- Store EditorSnapshot records:
  * Guid SessionId
  * EditorEntityType EntityType
  * string SnapshotJson (serialized schema)
  * string ActionDescription
  * DateTime Timestamp
  * int SequenceNumber
- Properties:
  * bool CanUndo
  * bool CanRedo
  * int UndoCount
  * int RedoCount
- Methods:
  * PushSnapshot(EditorSnapshot snapshot, string actionDescription)
  * EditorSnapshot? Undo()
  * EditorSnapshot? Redo()
  * Clear()
  * List<string> GetUndoActionHistory() (for UI display)
  * List<string> GetRedoActionHistory()
- Configuration:
  * MaxActions limit (default 100, configurable)
  * When limit exceeded, remove oldest from bottom of stack
- Event:
  * event EventHandler? StackChanged

Acceptance Criteria:
✓ Undo/Redo stacks work correctly
✓ PushSnapshot adds to undo stack and clears redo stack
✓ Undo moves snapshot from undo to redo
✓ Redo moves snapshot from redo to undo
✓ MaxActions limit enforced
✓ Action history methods return correct lists
✓ StackChanged event fires on modifications
```

### Prompt 3.3: Create AutoSaveService

```
Create the AutoSaveService for automatic background saving.

Requirements:
- Create Services/State/AutoSaveService.cs
- Inject:
  * EditorStateService
  * IEditorModuleRepository
  * ILogger<AutoSaveService>
  * IConfiguration (for interval setting)
- Use Timer for periodic execution
- Properties:
  * bool IsEnabled
  * DateTime? LastAutoSave
  * bool IsSaving
- Methods:
  * Start() - start auto-save timer
  * Stop() - stop timer
  * Task SaveNowAsync() - manual save trigger
  * private Task PerformAutoSaveAsync() - actual save logic
- Events:
  * event EventHandler<AutoSaveEventArgs>? AutoSaveCompleted
  * event EventHandler<AutoSaveErrorEventArgs>? AutoSaveError
- Configuration:
  * Read interval from EditorConfiguration table (default 30 seconds)
- Logic:
  * Check if IsDirty before saving
  * Get current snapshot from EditorStateService
  * Save to database (EditorFormModules or EditorWorkflows)
  * Call EditorStateService.MarkAsSaved()
  * Fire AutoSaveCompleted event
  * Catch errors and fire AutoSaveError event

Acceptance Criteria:
✓ Service starts/stops timer correctly
✓ Auto-save runs at configured interval
✓ Only saves if IsDirty is true
✓ Saves to correct database table
✓ Marks state as saved after successful save
✓ Error handling works and logs errors
✓ Events fire appropriately
✓ Implements IDisposable to clean up timer
```

### Prompt 3.4: Create FormBuilderService

```
Create the FormBuilderService for form building operations.

Requirements:
- Create Services/Operations/FormBuilderService.cs
- Inject:
  * EditorStateService
  * UndoRedoService
  * ILogger
- Methods:
  * Task AddFieldAsync(FormFieldSchema newField, string? parentId)
  * Task UpdateFieldAsync(FormFieldSchema updatedField)
  * Task DeleteFieldAsync(string fieldId)
  * Task CloneFieldAsync(string fieldId)
  * Task MoveFieldUpAsync(string fieldId)
  * Task MoveFieldDownAsync(string fieldId)
  * Task SetFieldParentAsync(string fieldId, string? newParentId)
- Each method should:
  * Get current module from EditorStateService
  * Create new module with the change applied
  * Call EditorStateService.UpdateModule(newModule, actionDescription)
  * UndoRedoService automatically captures snapshot via integration
- Validation:
  * Check for duplicate field IDs
  * Ensure parent exists when setting parent
  * Prevent circular parent relationships

Acceptance Criteria:
✓ All CRUD operations implemented
✓ Move up/down logic works correctly (reorders siblings)
✓ Clone creates new field with unique ID (e.g., fieldId_copy)
✓ Parent-child relationships maintained
✓ Validation prevents invalid operations
✓ Action descriptions are descriptive
✓ Integration with EditorStateService works
```

### Prompt 3.5: Create FieldOperationService

```
Create the FieldOperationService for field-specific operations.

Requirements:
- Create Services/Operations/FieldOperationService.cs
- Methods:
  * FormFieldSchema CreateTextField(string fieldId, string label, string? labelFr)
  * FormFieldSchema CreateDropDown(string fieldId, string label, List<FieldOption> options)
  * FormFieldSchema CreateDatePicker(string fieldId, string label)
  * FormFieldSchema CreateFileUpload(string fieldId, string label, FileUploadConfig config)
  * FormFieldSchema CreateSection(string fieldId, string label)
  * ... (factory methods for all field types)
  * ValidationRule CreateValidationRule(string ruleType, string? parameter, string errorMessage)
  * ConditionalRule CreateConditionalRule(string targetFieldId, ConditionalOperator op, string value)
- All factory methods should return properly configured FormFieldSchema instances
- Use FormFieldSchema factory methods from Core.V2

Acceptance Criteria:
✓ Factory methods for all field types
✓ Methods use Core.V2 factory methods where available
✓ Proper default values set
✓ Methods are easy to use
✓ XML documentation for each method
```

### Prompt 3.6: Create PublishService

```
Create the PublishService for publishing forms to production.

Requirements:
- Create Services/PublishService.cs
- Inject:
  * ApplicationDbContext
  * IFormValidationService
  * ILogger
- Methods:
  * Task<PublishResult> PublishModuleAsync(int moduleId)
  * Task<PublishResult> PublishWorkflowAsync(int workflowId)
  * Task<ValidationResult> ValidateSchemaForPublish(FormModuleSchema schema)
- PublishResult record:
  * bool Success
  * int? Version (if successful)
  * List<string> Errors
- Publish logic:
  1. Load draft module from EditorFormModules
  2. Validate schema (no errors, all required fields set, etc.)
  3. Get next version number (max(version) + 1)
  4. Deactivate previous versions (set IsActive = false)
  5. Create new PublishedFormModule record
  6. Update draft status to "Published"
  7. Save changes
  8. Return PublishResult
- Validation checks:
  * Schema is valid JSON
  * All fields have unique IDs
  * No circular parent references
  * All conditional target fields exist
  * At least one field exists

Acceptance Criteria:
✓ Publishing creates new version correctly
✓ Previous versions deactivated
✓ Validation prevents invalid schemas from publishing
✓ Draft status updated to "Published"
✓ Transaction ensures atomicity
✓ Errors logged and returned in result
✓ PublishResult provides clear success/failure info
```

---

## Phase 4: Editor UI Components

### Prompt 4.1: Create Blazor Server Editor Project

```
Create the main Blazor Server project for the editor.

Requirements:
- Create new Blazor Server App: Src/DynamicForms.Editor/
- Target: .NET 9.0
- Add project references:
  * DynamicForms.Core.V2
  * DynamicForms.Renderer
  * DynamicForms.Editor.Data
- Add NuGet packages:
  * Microsoft.EntityFrameworkCore.SqlServer
  * Microsoft.EntityFrameworkCore.Tools
  * Microsoft.Extensions.Caching.Memory
- Configure Program.cs:
  * Add DbContext with connection string
  * Register all services (EditorStateService, UndoRedoService, AutoSaveService, etc.)
  * Register repositories
  * Add Blazor Server services
  * Add memory cache
- Create appsettings.json with:
  * Connection string
  * Editor settings (auto-save interval, undo limit, etc.)
  * Logging configuration
- Update _Imports.razor with common namespaces

Acceptance Criteria:
✓ Project created and builds successfully
✓ All references and packages added
✓ Program.cs configures all services
✓ appsettings.json has all configuration
✓ App runs without errors (shows default page)
```

### Prompt 4.2: Create Main Layout and Navigation

```
Create the main layout and navigation for the editor.

Requirements:
- Update Shared/MainLayout.razor:
  * Two-column layout (sidebar + content)
  * Responsive design
  * Header with app title and user info area (for future)
- Create Shared/NavMenu.razor:
  * Links to:
    - Dashboard (/)
    - Browse Modules (/modules)
    - Browse Workflows (/workflows)
    - Create New Module (/editor/module)
    - Create New Workflow (/editor/workflow)
    - Settings (/settings)
  * Highlight active link
  * Collapsible on mobile
- Create Shared/EditorLayout.razor (for editor pages):
  * Full-width layout (no sidebar)
  * Sticky toolbar at top
  * Status bar at bottom
  * Content area in between
- Apply Bootstrap 5 styling
- Add custom CSS for editor-specific styles

Acceptance Criteria:
✓ MainLayout is clean and modern
✓ NavMenu has all required links
✓ Navigation works correctly
✓ EditorLayout is suitable for full-screen editing
✓ Responsive design works on different screen sizes
✓ Custom CSS is organized and well-structured
```

### Prompt 4.3: Create Module List Page

```
Create the module list/browse page.

Requirements:
- Create Pages/ModuleList.razor:
  * Route: /modules
  * Inject IEditorModuleRepository
  * Load all modules on initialization
  * Display in a table with columns:
    - ModuleId
    - Title
    - Status (Draft/Published/Archived)
    - Version
    - Last Modified
    - Actions (Edit, Delete, Clone)
  * Add search/filter functionality
  * Add "Create New" button
  * Add pagination (if >50 modules)
- Create Components/ModuleListItem.razor:
  * Displays one row in the table
  * Handles Edit/Delete/Clone actions
- Implement actions:
  * Edit: Navigate to /editor/module/{id}
  * Delete: Show confirmation, then delete
  * Clone: Create copy with new ID

Acceptance Criteria:
✓ Page loads and displays modules correctly
✓ Table is sortable by columns
✓ Search/filter works
✓ Actions (Edit/Delete/Clone) work correctly
✓ "Create New" navigates to editor
✓ Loading state shown while fetching data
✓ Empty state shown if no modules
```

### Prompt 4.4: Create EditorToolbar Component

```
Create the editor toolbar component.

Requirements:
- Create Components/Shared/EditorToolbar.razor
- Buttons:
  * Save (primary, disabled if not dirty)
  * Publish (success)
  * Undo (disabled if can't undo, show undo count)
  * Redo (disabled if can't redo, show redo count)
  * Import (load JSON)
  * Export (download JSON)
- Parameters:
  * bool CanUndo
  * bool CanRedo
  * bool IsDirty
  * EventCallback OnSave, OnPublish, OnUndo, OnRedo, OnImport, OnExport
- Styling:
  * Fixed at top of editor
  * Grouped buttons with separators
  * Icons from Bootstrap Icons
  * Tooltips on hover
- Show keyboard shortcuts in tooltips:
  * Ctrl+S for Save
  * Ctrl+Z for Undo
  * Ctrl+Y for Redo

Acceptance Criteria:
✓ Toolbar renders correctly
✓ All buttons are functional
✓ Disabled states work properly
✓ Tooltips show shortcuts
✓ Styling matches design mockup
✓ Buttons fire correct events
```

### Prompt 4.5: Create EditorStatusBar Component

```
Create the editor status bar component.

Requirements:
- Create Components/Shared/EditorStatusBar.razor
- Display information:
  * "Saved" or "Unsaved changes" indicator
  * Last saved time (e.g., "Auto-saved 2 minutes ago")
  * Last modified time
  * Saving indicator (spinner when saving)
  * Field count
  * Validation error count (if any)
- Parameters:
  * DateTime? LastSaved
  * DateTime LastModified
  * bool IsDirty
  * bool IsSaving
  * int FieldCount
  * int ErrorCount
- Styling:
  * Fixed at bottom of editor
  * Subtle gray background
  * Small text
  * Success/warning color coding
- Use relative time display ("2 minutes ago") using JavaScript interop

Acceptance Criteria:
✓ Status bar renders correctly
✓ Information updates in real-time
✓ Saving indicator shows during save
✓ Color coding works (green=saved, orange=dirty)
✓ Relative time updates periodically
```

### Prompt 4.6: Create FieldListPanel Component

```
Create the field list panel component.

Requirements:
- Create Components/FieldList/FieldListPanel.razor
- Layout:
  * Header with "Fields" title and count
  * Scrollable list area
  * Footer with "Add Field" button
- Props:
  * List<FormFieldSchema> Fields
  * string? SelectedFieldId
  * EventCallback<string> OnFieldSelected
  * EventCallback<FormFieldSchema> OnFieldMoveUp/Down/Edit/Clone/Delete
  * EventCallback OnAddField
- Render:
  * Loop through root fields (ParentId == null)
  * Use FieldListItem component (recursive)
  * Show empty state if no fields
- Styling:
  * 30% width of editor
  * Border on right
  * Light gray background

Acceptance Criteria:
✓ Panel renders correctly
✓ Field list displays hierarchically
✓ Selected field is highlighted
✓ Add Field button works
✓ Scrolling works for long lists
✓ Empty state shown when appropriate
```

### Prompt 4.7: Create FieldListItem Component (Recursive)

```
Create the field list item component with recursive rendering.

Requirements:
- Create Components/FieldList/FieldListItem.razor
- Displays:
  * Field icon (based on type)
  * Field label
  * Field ID (small, muted)
  * Action buttons: ↑ ↓ ✏ ⎘ 🗑
  * Expand/collapse icon for containers
- Props:
  * FormFieldSchema Field
  * List<FormFieldSchema> AllFields (to find children)
  * string? SelectedFieldId
  * Event callbacks for all actions
- Features:
  * Indentation based on depth (20px per level)
  * Expand/collapse for containers (Section, Tab, Panel)
  * Highlight when selected
  * Action buttons visible on hover
  * Disable move up/down if first/last sibling
- Recursively render children when expanded

Acceptance Criteria:
✓ Renders field info correctly
✓ Recursive rendering works for nested fields
✓ Expand/collapse works
✓ Indentation shows hierarchy visually
✓ Action buttons work correctly
✓ Move up/down disabled appropriately
✓ Selection highlighting works
✓ Icons match field types
```

### Prompt 4.8: Create PropertiesPanel Component

```
Create the properties panel component.

Requirements:
- Create Components/Properties/PropertiesPanel.razor
- Layout:
  * Header with "Field Properties" title and close button
  * Scrollable content area with sections:
    - Basic Properties (uses BasicPropertiesSection)
    - Type Configuration (uses TypeConfigSection)
    - Validation Rules (uses ValidationRulesSection)
    - Conditional Rules (uses ConditionalRulesSection)
  * Footer with "Apply Changes" and "Cancel" buttons
- Props:
  * FormFieldSchema? Field (selected field)
  * EventCallback<FormFieldSchema> OnFieldChanged
  * EventCallback OnCancel
- Behavior:
  * Create working copy of field (don't mutate original)
  * Apply changes only when "Apply" clicked
  * Cancel discards changes
  * Show empty state if no field selected
- Styling:
  * 70% width of editor
  * White background
  * Padded content

Acceptance Criteria:
✓ Panel renders correctly
✓ Working copy pattern implemented
✓ Apply saves changes correctly
✓ Cancel discards changes
✓ Empty state shown when no selection
✓ All subsections render properly
✓ Scrolling works for long content
```

### Prompt 4.9: Create BasicPropertiesSection Component

```
Create the basic properties section for editing field properties.

Requirements:
- Create Components/Properties/BasicPropertiesSection.razor
- Form fields:
  * Field ID (text input, required)
  * Label EN (text input, required)
  * Label FR (text input, optional)
  * Field Type (dropdown, all types)
  * Description EN (textarea, optional)
  * Description FR (textarea, optional)
  * Enabled checkbox
- Props:
  * FormFieldSchema Field
  * EventCallback<FormFieldSchema> OnFieldChanged
- Behavior:
  * Two-way binding to field properties
  * Fire OnFieldChanged on blur (not on every keystroke)
  * Validate field ID (alphanumeric and underscore only)
  * When field type changes, reset TypeConfig
- Styling:
  * Bootstrap form groups
  * Clear labels
  * Help text where appropriate

Acceptance Criteria:
✓ All fields render correctly
✓ Two-way binding works
✓ Field ID validation works
✓ Type change resets config
✓ OnFieldChanged fires appropriately
✓ Form is clean and user-friendly
```

### Prompt 4.10: Create Validation & Conditional Rule Editors

```
Create the validation and conditional rule editor components.

Requirements:
- Create Components/Properties/ValidationRulesSection.razor:
  * List of validation rules
  * Add rule button
  * Uses ValidationRuleEditor for each rule
- Create Components/Properties/ValidationRuleEditor.razor:
  * Inline editor for one rule
  * Dropdown for rule type (required, minLength, maxLength, pattern, email, etc.)
  * Parameter input (if needed for rule type)
  * Error message input
  * Remove button
- Create Components/Properties/ConditionalRulesSection.razor:
  * List of conditional rules
  * Add rule button
  * Uses ConditionalRuleEditor for each rule
- Create Components/Properties/ConditionalRuleEditor.razor:
  * Target field dropdown (all fields in module)
  * Operator dropdown (Equals, NotEquals, Contains, etc.)
  * Value input
  * Action dropdown (Show/Hide, Enable/Disable)
  * Remove button
- Both sections:
  * Compact inline editing
  * Add new rule creates default rule
  * Remove confirmation

Acceptance Criteria:
✓ All 4 components created
✓ Validation rule editor covers all rule types
✓ Parameter input shows/hides based on rule type
✓ Conditional rule editor shows available fields
✓ Adding/removing rules works correctly
✓ UI is clean and intuitive
✓ Changes fire OnRulesChanged event
```

---

## Phase 5: Workflow Editor

### Prompt 5.1: Create WorkflowEditor Page

```
Create the workflow editor page for multi-module workflows.

Requirements:
- Create Pages/Editor/WorkflowEditor.razor
- Route: /editor/workflow/{WorkflowId:int?}
- Similar structure to ModuleEditor:
  * Toolbar (save, publish, undo, redo, import, export)
  * Main content (module list + properties)
  * Status bar
- Uses WorkflowStateService (similar to EditorStateService but for workflows)
- Content area shows:
  * Left: List of modules in workflow (ordered)
  * Right: Module properties (when selected)
- Can add existing modules to workflow
- Can define conditional branching between modules

Acceptance Criteria:
✓ Page loads and displays workflow
✓ Can load existing workflow or create new
✓ Toolbar works (save, publish, undo, redo)
✓ Module list shows modules in order
✓ Can select module to edit properties
✓ Auto-save works
```

### Prompt 5.2: Create WorkflowModuleList Component

```
Create the workflow module list component.

Requirements:
- Create Components/Workflow/WorkflowModuleList.razor
- Displays list of modules in workflow (FormWorkflowSchema.Modules)
- For each module:
  * Module title
  * Module ID
  * Order number
  * Conditional branch info (if any)
  * Action buttons: ↑ ↓ ✏ 🗑
- Can add module:
  * Show dialog with available modules
  * Select module to add
  * Adds to end of list
- Can reorder modules (up/down)
- Can remove module from workflow
- Can edit conditional branching

Acceptance Criteria:
✓ List renders modules correctly
✓ Add module dialog works
✓ Reordering works
✓ Remove confirmation works
✓ Conditional branching UI shows branch info
```

### Prompt 5.3: Create ConditionalBranchEditor Component

```
Create the conditional branch editor for workflow transitions.

Requirements:
- Create Components/Workflow/ConditionalBranchEditor.razor
- Allows defining:
  * Condition: "If field X equals Y, then..."
  * Next module: Which module to show next
  * Else: Which module to show if condition fails
- UI:
  * Field selector (from current module)
  * Operator dropdown
  * Value input
  * Next module dropdown (available modules in workflow)
  * Else module dropdown
- Can have multiple conditions (AND logic)
- Visual indication of branching paths

Acceptance Criteria:
✓ Editor renders correctly
✓ Can define conditions
✓ Can select next modules
✓ Multiple conditions supported
✓ Changes saved to workflow schema
✓ UI is intuitive
```

### Prompt 5.4: Create Workflow Preview Component

```
Create a preview component for workflows.

Requirements:
- Create Components/Workflow/WorkflowPreview.razor
- Shows visual representation of workflow:
  * Modules as boxes
  * Arrows showing flow
  * Conditional branches shown as decision diamonds
  * Labels on arrows indicating conditions
- Can click through workflow to see each module's form
- Shows current module indicator
- Navigation buttons: Next, Previous, Submit
- Evaluates conditional logic to determine next module

Acceptance Criteria:
✓ Visual workflow diagram renders
✓ Can navigate through workflow
✓ Conditional branching works correctly
✓ Each module renders its form
✓ Submit at end captures all data
✓ Visual is clear and helpful
```

---

## Phase 6: Import/Export & Publish

### Prompt 6.1: Implement JSON Import Functionality

```
Implement JSON import functionality for the editor.

Requirements:
- Create Services/ImportExportService.cs
- Method: Task<ImportResult> ImportModuleFromJsonAsync(string json)
- Validation:
  * JSON is valid
  * Schema deserializes to FormModuleSchema
  * All required fields present
  * No duplicate field IDs
  * No circular parent references
- If valid:
  * Load into EditorStateService
  * Create initial undo snapshot
  * Return success result
- If invalid:
  * Return error result with validation messages
- Add import button handler in ModuleEditor:
  * Show file picker dialog
  * Read JSON file
  * Call ImportModuleFromJsonAsync
  * Show success/error message

Acceptance Criteria:
✓ Import service created
✓ Validation checks all requirements
✓ Successful import loads module into editor
✓ Validation errors shown clearly
✓ File picker works
✓ User feedback provided
```

### Prompt 6.2: Implement JSON Export Functionality

```
Implement JSON export functionality for the editor.

Requirements:
- Add to ImportExportService:
  * Method: string ExportModuleToJson(FormModuleSchema module, bool prettyPrint = true)
  * Serialize module to JSON
  * Optionally format with indentation
- Add export button handler in ModuleEditor:
  * Get current module from EditorStateService
  * Call ExportModuleToJson
  * Trigger browser download with filename: "{ModuleTitle}_{ModuleId}.json"
  * Use JavaScript interop for download
- Create wwwroot/js/fileDownload.js:
  * Function to trigger download in browser

Acceptance Criteria:
✓ Export service method created
✓ JSON serialization works correctly
✓ Export button triggers download
✓ Filename is meaningful
✓ Pretty-printed JSON is readable
✓ JavaScript interop works
```

### Prompt 6.3: Implement Publish Functionality

```
Implement publish functionality in the editor.

Requirements:
- Add publish button handler in ModuleEditor:
  * Call PublishService.PublishModuleAsync
  * Show confirmation dialog before publish
  * Run validation before publish
  * Show validation errors if any
  * On success:
    - Show success message with version number
    - Mark as saved
    - Update status to "Published"
  * On failure:
    - Show error messages
- Create Components/Dialogs/PublishDialog.razor:
  * Confirmation dialog
  * Shows what will be published
  * Shows version number that will be created
  * Confirm/Cancel buttons
- Create Components/Dialogs/ValidationErrorsDialog.razor:
  * Shows list of validation errors
  * Prevents publish if errors exist
  * Helps user find and fix errors

Acceptance Criteria:
✓ Publish button works correctly
✓ Confirmation dialog shows
✓ Validation runs before publish
✓ Success message shows new version
✓ Error handling works
✓ Status updates correctly
✓ Can publish successfully
```

### Prompt 6.4: Implement Publish History View

```
Create a publish history view to see all published versions.

Requirements:
- Create Pages/PublishHistory.razor
- Route: /modules/{ModuleId}/history
- Shows table of all published versions:
  * Version number
  * Published date/time
  * Published by (future: username)
  * Status (Active/Inactive)
  * Actions: View, Restore, Download JSON
- Can view previous version (read-only)
- Can restore previous version to drafts (creates new draft)
- Can download JSON of any version

Acceptance Criteria:
✓ History page loads and shows versions
✓ Table displays correct information
✓ View opens read-only preview
✓ Restore creates new draft correctly
✓ Download JSON works for all versions
✓ Active version is clearly marked
```

---

## Phase 7: Testing

### Prompt 7.1: Create Unit Tests for Services

```
Create comprehensive unit tests for editor services.

Requirements:
- Create test project: Tests/DynamicForms.Editor.Tests/
- Add NuGet packages:
  * xUnit
  * Moq
  * FluentAssertions
  * Microsoft.EntityFrameworkCore.InMemory
- Create test classes:
  * EditorStateServiceTests.cs
  * UndoRedoServiceTests.cs
  * AutoSaveServiceTests.cs
  * FormBuilderServiceTests.cs
  * PublishServiceTests.cs
- Each test class should cover:
  * Happy path scenarios
  * Edge cases
  * Error conditions
  * State changes
  * Event firing
- Aim for >80% code coverage

Acceptance Criteria:
✓ Test project builds successfully
✓ All service classes have test coverage
✓ Tests are well-organized and named
✓ Tests pass reliably
✓ Code coverage >80%
✓ Mocks used appropriately
```

### Prompt 7.2: Create Unit Tests for Renderer

```
Create unit tests for form renderer components.

Requirements:
- Create test project: Tests/DynamicForms.Renderer.Tests/
- Use bUnit for Blazor component testing
- Add NuGet package: bUnit
- Create test classes:
  * DynamicFormRendererTests.cs
  * TextFieldRendererTests.cs
  * DropDownRendererTests.cs
  * ConditionalLogicEngineTests.cs
  * FormDataTests.cs
- Test scenarios:
  * Component renders correctly
  * Field value binding works
  * Validation errors display
  * Conditional logic hides/shows fields
  * Form submission works
  * Events fire correctly

Acceptance Criteria:
✓ Test project builds successfully
✓ bUnit configured correctly
✓ All renderer components tested
✓ ConditionalLogicEngine thoroughly tested
✓ Tests pass reliably
✓ Code coverage >80%
```

### Prompt 7.3: Create Integration Tests

```
Create integration tests for end-to-end scenarios.

Requirements:
- Create test project: Tests/DynamicForms.Integration.Tests/
- Use WebApplicationFactory for integration testing
- Add NuGet package: Microsoft.AspNetCore.Mvc.Testing
- Test scenarios:
  1. Create module → Edit fields → Save → Publish → Verify in PublishedFormModules
  2. Load module → Make changes → Undo → Verify changes reverted
  3. Auto-save triggers → Verify saved to database
  4. Import JSON → Verify loaded correctly
  5. Export JSON → Verify JSON is valid
  6. Publish workflow → Verify all modules published
- Use in-memory database for tests
- Clean up database after each test

Acceptance Criteria:
✓ Integration test project builds
✓ WebApplicationFactory configured
✓ All scenarios tested end-to-end
✓ Tests are reliable and repeatable
✓ Database cleanup works
✓ Tests pass consistently
```

### Prompt 7.4: Create Renderer UI Tests (Manual Test Plan)

```
Create a manual test plan document for UI testing the renderer.

Requirements:
- Create Tests/ManualTestPlan.md
- Document test cases for:
  * All field types render correctly
  * All container types (Section, Tab, Panel) work
  * Conditional logic shows/hides fields
  * Validation displays errors correctly
  * Form submission works
  * Responsive design on different screen sizes
  * Accessibility (keyboard navigation, screen readers)
  * Localization (EN/FR labels)
- For each test case:
  * Test name
  * Steps to execute
  * Expected result
  * Pass/Fail checkbox
  * Notes section
- Organize by feature area

Acceptance Criteria:
✓ Manual test plan document created
✓ All features have test cases
✓ Test cases are clear and detailed
✓ Easy to follow for testers
✓ Covers both editor and renderer
✓ Accessibility included
```

### Prompt 7.5: Create Performance Tests

```
Create performance tests for renderer and editor.

Requirements:
- Add to Tests/DynamicForms.Integration.Tests/
- Create PerformanceTests.cs
- Use BenchmarkDotNet for benchmarking
- Test scenarios:
  * Render form with 100 fields (should be <500ms)
  * Render form with 500 fields (measure time)
  * Build hierarchy from large schema (should be <200ms)
  * Auto-save large module (measure time)
  * Evaluate conditional logic on 100 fields (should be <100ms)
- Create test schemas:
  * SmallForm.json (10 fields)
  * MediumForm.json (50 fields)
  * LargeForm.json (100 fields)
  * HugeForm.json (500 fields)
- Measure:
  * Execution time
  * Memory allocation
  * Garbage collection

Acceptance Criteria:
✓ Performance test project configured
✓ BenchmarkDotNet integrated
✓ Test schemas created
✓ Benchmarks run successfully
✓ Results documented
✓ Performance targets met
```

---

## Phase 8: Polish & Documentation

### Prompt 8.1: Create User Documentation

```
Create comprehensive user documentation for the visual editor.

Requirements:
- Create Docs/UserGuide.md
- Sections:
  1. Introduction - What is the visual editor?
  2. Getting Started - First form in 5 minutes
  3. Creating Forms - Step-by-step guide
  4. Field Types - Overview of all field types
  5. Validation Rules - How to add validation
  6. Conditional Logic - Show/hide fields dynamically
  7. Workflows - Creating multi-module workflows
  8. Publishing - Deploy forms to production
  9. Import/Export - Backup and restore
  10. Troubleshooting - Common issues
- Include screenshots (placeholders for now)
- Include examples for common scenarios:
  * Contact form
  * Grant application
  * Survey
  * Multi-step registration

Acceptance Criteria:
✓ User guide is comprehensive
✓ All features documented
✓ Examples are clear and helpful
✓ Screenshots placeholders added
✓ Troubleshooting covers common issues
✓ Document is well-organized
```

### Prompt 8.2: Create Developer Documentation

```
Create developer documentation for extending the editor.

Requirements:
- Create Docs/DeveloperGuide.md
- Sections:
  1. Architecture Overview
  2. Project Structure
  3. State Management
  4. Creating Custom Field Types
  5. Creating Custom Validation Rules
  6. Extending the Renderer
  7. Database Schema
  8. API Reference (service methods)
  9. Testing Guide
  10. Deployment Guide
- Include code examples
- Include architecture diagrams (ASCII or Mermaid)
- Document extension points

Acceptance Criteria:
✓ Developer guide is technical and detailed
✓ Architecture explained clearly
✓ Extension points documented
✓ Code examples provided
✓ Diagrams included
✓ Useful for developers extending the system
```

### Prompt 8.3: UI/UX Polish

```
Polish the UI/UX of the editor for production readiness.

Requirements:
- Review all components and improve:
  * Consistent spacing and alignment
  * Consistent color scheme
  * Consistent button styles
  * Consistent icons
  * Smooth transitions and animations
  * Loading states (spinners)
  * Error states (red highlights, error messages)
  * Empty states (helpful messages and actions)
  * Success states (green checkmarks, success messages)
- Add keyboard shortcuts:
  * Ctrl+S: Save
  * Ctrl+Z: Undo
  * Ctrl+Y: Redo
  * Ctrl+E: Export
  * Escape: Cancel edit
- Add tooltips on all buttons
- Add confirmation dialogs for destructive actions (delete, etc.)
- Improve accessibility:
  * Proper ARIA labels
  * Keyboard navigation
  * Focus management
  * Screen reader support
- Test on different browsers:
  * Chrome
  * Edge
  * Firefox
  * Safari

Acceptance Criteria:
✓ UI is visually consistent throughout
✓ Keyboard shortcuts work
✓ Tooltips are helpful
✓ Confirmations prevent mistakes
✓ Accessibility improvements made
✓ Works on all major browsers
✓ Loading/error/empty/success states handled
```

---

## Quick Start Guide

### First 3 Days (Getting Started)

**Day 1: Database Setup**
- Execute Prompts 1.1, 1.2, 1.3
- Verify database created and migrations work

**Day 2: Repositories**
- Execute Prompts 1.4, 1.5
- Test CRUD operations manually

**Day 3: Renderer Foundation**
- Execute Prompts 2.1, 2.2, 2.3
- Test conditional logic engine

### Weeks 1-2: Renderer Complete

- Execute all Phase 2 prompts (2.1 - 2.8)
- Test rendering forms from JSON
- Verify all field types work

### Weeks 3-4: Editor Services

- Execute all Phase 3 prompts (3.1 - 3.6)
- Test undo/redo functionality
- Test auto-save

### Weeks 5-8: Editor UI

- Execute all Phase 4 prompts (4.1 - 4.10)
- Build editor page component by component
- Test editing workflows

### Weeks 9-10: Workflow & Publish

- Execute Phase 5 and 6 prompts
- Test workflow creation
- Test publish process

### Weeks 11-12: Testing

- Execute Phase 7 prompts
- Run all tests
- Fix any issues found

### Weeks 13-14: Polish

- Execute Phase 8 prompts
- Final UI polish
- Documentation

---

## Success Criteria

### After Phase 2 (Renderer)
✓ Can render a complete form from JSON schema
✓ All field types display correctly
✓ Conditional logic works
✓ Validation displays errors
✓ Form submission captures data

### After Phase 4 (Editor UI)
✓ Can create a new form module
✓ Can add/edit/delete fields
✓ Can reorder fields
✓ Can save and load forms
✓ Undo/redo works
✓ Auto-save works

### After Phase 6 (Complete)
✓ Can create multi-module workflows
✓ Can import/export JSON
✓ Can publish to production
✓ Production apps can render published forms
✓ Version history works

### Production Ready
✓ All tests passing
✓ UI is polished and professional
✓ Documentation complete
✓ Performance targets met
✓ Accessibility requirements met
✓ Works on all major browsers

---

**Document Version**: 1.0
**Last Updated**: January 2025
**Next**: Execute prompts in order, verify each phase before proceeding
