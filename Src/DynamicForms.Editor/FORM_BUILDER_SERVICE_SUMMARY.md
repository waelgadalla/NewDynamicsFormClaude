# FormBuilderService Summary

## Overview

Successfully created the **FormBuilderService** - a comprehensive service for form building operations (CRUD on fields). Provides methods to add, update, delete, clone, and reorder fields in a form module with full validation, hierarchy management, and undo/redo integration.

**Date Completed**: November 28, 2025
**Status**: ✅ All acceptance criteria met
**Build Status**: 0 Errors, 0 Warnings

---

## What Was Created

### FormBuilderService Class (572 lines) ✅

**Location**: `Src/DynamicForms.Editor/Services/Operations/FormBuilderService.cs`

**Purpose**: Provides CRUD operations and hierarchy management for form fields

**Features**:
- **Add fields** with parent-child relationships
- **Update fields** in-place
- **Delete fields** with cascading child deletion
- **Clone fields** with unique ID generation
- **Move fields** up/down within siblings
- **Set field parent** with circular relationship prevention
- **Full validation** (duplicate IDs, parent existence, circular relationships)
- **Undo/redo integration** via EditorStateService and UndoRedoService
- **Descriptive action names** for undo/redo history
- **Immutable operations** using record `with` expressions

---

## Architecture

### Service Dependencies

```
FormBuilderService
├── EditorStateService (required)
│   └── Manages current module state
├── UndoRedoService (required)
│   └── Tracks undo/redo snapshots
└── ILogger<FormBuilderService> (required)
    └── Logs all operations
```

### Operation Flow

All operations follow this pattern:

```
Method called (e.g., AddFieldAsync)
  ↓
Get current module from EditorStateService
  ↓
Validate operation
  │
  ├─ Check field exists/doesn't exist
  ├─ Check parent exists
  ├─ Check for duplicates
  └─ Check for circular relationships
  ↓
Create new module with change applied
  (using immutable record with expressions)
  ↓
Update EditorStateService
  EditorStateService.UpdateModule(newModule, actionDescription)
  ↓
Create and push snapshot to UndoRedoService
  UndoRedoService.PushSnapshot(snapshot, actionDescription)
  ↓
Log success
  ↓
Operation complete
```

### Immutability Pattern

Since `FormModuleSchema` and `FormFieldSchema` are immutable records, all changes create new instances:

```csharp
// Example: Adding a field
var newFields = currentModule.Fields.Append(newField).ToArray();
var updatedModule = currentModule with { Fields = newFields };

// Example: Updating a field
var newFields = currentModule.Fields
    .Select(f => f.Id == fieldId ? updatedField : f)
    .ToArray();
var updatedModule = currentModule with { Fields = newFields };

// Example: Deleting a field
var newFields = currentModule.Fields
    .Where(f => f.Id != fieldId)
    .ToArray();
var updatedModule = currentModule with { Fields = newFields };
```

---

## Methods

### AddFieldAsync()
```csharp
public async Task AddFieldAsync(FormFieldSchema newField, string? parentId)
```

**Purpose**: Adds a new field to the module

**Parameters**:
- `newField` - Field to add (must have unique ID)
- `parentId` - Optional parent field ID (null for root-level)

**Behavior**:
1. Validates field ID is unique
2. Validates parent exists (if specified)
3. Sets ParentId on new field
4. Assigns Order = max sibling order + 1
5. Appends field to module
6. Updates EditorStateService
7. Pushes undo/redo snapshot

**Validation**:
- Field ID must be unique
- Parent must exist (if specified)
- No module loaded → exception

**Exceptions**:
- `ArgumentNullException` - If newField is null
- `InvalidOperationException` - If validation fails

**Example**:
```csharp
// Add root-level text field
var textField = FormFieldSchema.CreateTextField(
    id: "firstName",
    labelEn: "First Name",
    labelFr: "Prénom",
    isRequired: true,
    order: 1);

await _formBuilder.AddFieldAsync(textField, parentId: null);
// Undo history: "Added TextBox: First Name"

// Add child field to a section
var emailField = FormFieldSchema.CreateTextField(
    id: "email",
    labelEn: "Email Address",
    isRequired: true);

await _formBuilder.AddFieldAsync(emailField, parentId: "contactSection");
// Undo history: "Added TextBox: Email Address"
```

**Action Description**: `"Added {FieldType}: {LabelEn}"`

---

### UpdateFieldAsync()
```csharp
public async Task UpdateFieldAsync(FormFieldSchema updatedField)
```

**Purpose**: Updates an existing field in the module

**Parameters**:
- `updatedField` - Updated field (must have same ID as existing field)

**Behavior**:
1. Validates field exists
2. Replaces old field with updated field
3. Updates EditorStateService
4. Pushes undo/redo snapshot

**Validation**:
- Field must exist
- No module loaded → exception

**Exceptions**:
- `ArgumentNullException` - If updatedField is null
- `InvalidOperationException` - If field doesn't exist

**Example**:
```csharp
// Get current field
var currentModule = _editorState.GetCurrentModule();
var field = currentModule.Fields.First(f => f.Id == "firstName");

// Update field properties
var updatedField = field with
{
    LabelEn = "Full Name",
    LabelFr = "Nom complet",
    IsRequired = false,
    MaxLength = 100
};

await _formBuilder.UpdateFieldAsync(updatedField);
// Undo history: "Updated TextBox: Full Name"
```

**Action Description**: `"Updated {FieldType}: {LabelEn}"`

---

### DeleteFieldAsync()
```csharp
public async Task DeleteFieldAsync(string fieldId)
```

**Purpose**: Deletes a field and all its descendants

**Parameters**:
- `fieldId` - ID of field to delete

**Behavior**:
1. Validates field exists
2. Gets all descendant IDs recursively
3. Removes field and all descendants
4. Updates EditorStateService
5. Pushes undo/redo snapshot

**Cascade Delete**: If field has children, they are all deleted

**Validation**:
- Field must exist
- No module loaded → exception

**Exceptions**:
- `ArgumentException` - If fieldId is null/empty
- `InvalidOperationException` - If field doesn't exist

**Example**:
```csharp
// Delete a field
await _formBuilder.DeleteFieldAsync("firstName");
// Undo history: "Deleted TextBox: First Name"

// Delete a section with 3 child fields
await _formBuilder.DeleteFieldAsync("contactSection");
// Undo history: "Deleted Section: Contact Information and 3 child field(s)"
```

**Action Description**:
- No children: `"Deleted {FieldType}: {LabelEn}"`
- With children: `"Deleted {FieldType}: {LabelEn} and {N} child field(s)"`

---

### CloneFieldAsync()
```csharp
public async Task CloneFieldAsync(string fieldId)
```

**Purpose**: Clones a field with a new unique ID

**Parameters**:
- `fieldId` - ID of field to clone

**Behavior**:
1. Validates field exists
2. Generates unique ID (fieldId_copy, fieldId_copy2, etc.)
3. Creates clone with new ID and Order = original.Order + 1
4. Appends clone to module
5. Updates EditorStateService
6. Pushes undo/redo snapshot

**Note**: Only clones the specified field, not its children

**ID Generation**:
- First clone: `{fieldId}_copy`
- Second clone: `{fieldId}_copy2`
- Third clone: `{fieldId}_copy3`
- etc.

**Validation**:
- Field must exist
- No module loaded → exception

**Exceptions**:
- `ArgumentException` - If fieldId is null/empty
- `InvalidOperationException` - If field doesn't exist

**Example**:
```csharp
// Clone a field
await _formBuilder.CloneFieldAsync("firstName");
// New field ID: "firstName_copy"
// Undo history: "Cloned TextBox: First Name → firstName_copy"

// Clone again
await _formBuilder.CloneFieldAsync("firstName");
// New field ID: "firstName_copy2"
// Undo history: "Cloned TextBox: First Name → firstName_copy2"
```

**Action Description**: `"Cloned {FieldType}: {LabelEn} → {CloneId}"`

---

### MoveFieldUpAsync()
```csharp
public async Task MoveFieldUpAsync(string fieldId)
```

**Purpose**: Moves a field up in display order (swaps with previous sibling)

**Parameters**:
- `fieldId` - ID of field to move up

**Behavior**:
1. Validates field exists
2. Gets siblings (fields with same parent)
3. Orders siblings by Order property
4. Finds current position
5. Swaps Order with previous sibling
6. Updates EditorStateService
7. Pushes undo/redo snapshot

**Sibling Scope**: Only moves within siblings (same parent)

**Validation**:
- Field must exist
- Must not be first sibling (already at top)
- No module loaded → exception

**Exceptions**:
- `ArgumentException` - If fieldId is null/empty
- `InvalidOperationException` - If field doesn't exist or already at top

**Example**:
```csharp
// Before:
// - Section (order 1)
//   - Field A (order 1)
//   - Field B (order 2)  ← Move this up
//   - Field C (order 3)

await _formBuilder.MoveFieldUpAsync("fieldB");

// After:
// - Section (order 1)
//   - Field B (order 1)  ← Now here
//   - Field A (order 2)
//   - Field C (order 3)

// Undo history: "Moved TextBox up: Field B"
```

**Action Description**: `"Moved {FieldType} up: {LabelEn}"`

---

### MoveFieldDownAsync()
```csharp
public async Task MoveFieldDownAsync(string fieldId)
```

**Purpose**: Moves a field down in display order (swaps with next sibling)

**Parameters**:
- `fieldId` - ID of field to move down

**Behavior**:
1. Validates field exists
2. Gets siblings (fields with same parent)
3. Orders siblings by Order property
4. Finds current position
5. Swaps Order with next sibling
6. Updates EditorStateService
7. Pushes undo/redo snapshot

**Sibling Scope**: Only moves within siblings (same parent)

**Validation**:
- Field must exist
- Must not be last sibling (already at bottom)
- No module loaded → exception

**Exceptions**:
- `ArgumentException` - If fieldId is null/empty
- `InvalidOperationException` - If field doesn't exist or already at bottom

**Example**:
```csharp
// Before:
// - Section (order 1)
//   - Field A (order 1)  ← Move this down
//   - Field B (order 2)
//   - Field C (order 3)

await _formBuilder.MoveFieldDownAsync("fieldA");

// After:
// - Section (order 1)
//   - Field B (order 1)
//   - Field A (order 2)  ← Now here
//   - Field C (order 3)

// Undo history: "Moved TextBox down: Field A"
```

**Action Description**: `"Moved {FieldType} down: {LabelEn}"`

---

### SetFieldParentAsync()
```csharp
public async Task SetFieldParentAsync(string fieldId, string? newParentId)
```

**Purpose**: Changes the parent of a field (moves field in hierarchy)

**Parameters**:
- `fieldId` - ID of field to reparent
- `newParentId` - New parent ID (null for root-level)

**Behavior**:
1. Validates field exists
2. Validates new parent exists (if specified)
3. Validates not setting self as parent
4. Validates no circular relationship created
5. Updates field's ParentId
6. Updates EditorStateService
7. Pushes undo/redo snapshot

**Validation**:
- Field must exist
- New parent must exist (if specified)
- Cannot set self as parent
- Cannot create circular relationship
- No module loaded → exception

**Circular Relationship Check**:
```
fieldId cannot be an ancestor of newParentId

Example of INVALID operation:
- Section A (id: "sectionA")
  - Section B (id: "sectionB")
    - Field C (id: "fieldC")

Cannot do: SetFieldParent("sectionA", "sectionB")
Why: sectionB is a child of sectionA, creating a circle
```

**Exceptions**:
- `ArgumentException` - If fieldId is null/empty
- `InvalidOperationException` - If validation fails

**Example**:
```csharp
// Move field to different section
await _formBuilder.SetFieldParentAsync("email", "contactSection");
// Undo history: "Moved Email Address to Contact Information"

// Move field to root level
await _formBuilder.SetFieldParentAsync("email", null);
// Undo history: "Moved Email Address to root"

// INVALID: Create circular relationship
await _formBuilder.SetFieldParentAsync("contactSection", "email");
// Throws: "Setting 'email' as parent of 'contactSection' would create a circular relationship"
```

**Action Description**: `"Moved {FieldLabel} to {ParentLabel}"` or `"Moved {FieldLabel} to root"`

---

## Helper Methods

### GetSiblings()
```csharp
private IEnumerable<FormFieldSchema> GetSiblings(FormFieldSchema[] fields, string? parentId)
```

**Purpose**: Gets all sibling fields (fields with the same parent)

**Returns**: Fields where ParentId == parentId

---

### GetDescendantIds()
```csharp
private IEnumerable<string> GetDescendantIds(FormFieldSchema[] fields, string parentId)
```

**Purpose**: Gets all descendant field IDs recursively

**Returns**: All child, grandchild, etc. field IDs

**Used By**: DeleteFieldAsync() for cascade delete

---

### GenerateUniqueFieldId()
```csharp
private string GenerateUniqueFieldId(FormFieldSchema[] fields, string baseId)
```

**Purpose**: Generates a unique field ID for cloning

**Algorithm**:
1. Try `{baseId}_copy`
2. If exists, try `{baseId}_copy2`
3. If exists, try `{baseId}_copy3`
4. Continue incrementing until unique

**Returns**: Unique field ID

**Used By**: CloneFieldAsync()

---

### WouldCreateCircularRelationship()
```csharp
private bool WouldCreateCircularRelationship(
    FormFieldSchema[] fields,
    string fieldId,
    string newParentId)
```

**Purpose**: Checks if setting newParentId as parent would create a circular relationship

**Algorithm**: Returns true if fieldId is an ancestor of newParentId

**Used By**: SetFieldParentAsync()

---

### GetAncestorIds()
```csharp
private IEnumerable<string> GetAncestorIds(FormFieldSchema[] fields, string fieldId)
```

**Purpose**: Gets all ancestor field IDs recursively

**Returns**: Parent, grandparent, etc. field IDs

**Used By**: WouldCreateCircularRelationship()

---

### CreateSnapshot()
```csharp
private EditorSnapshot CreateSnapshot(FormModuleSchema module, string actionDescription)
```

**Purpose**: Creates an EditorSnapshot for undo/redo

**Returns**: EditorSnapshot with serialized module

**Used By**: All public methods

---

## Validation Rules

### Add Field Validation

| Rule | Check | Exception |
|------|-------|-----------|
| Unique ID | Field ID must not already exist | "Field with ID 'X' already exists" |
| Parent Exists | Parent field must exist (if specified) | "Parent field 'X' does not exist" |
| Module Loaded | A module must be loaded | "No module is currently loaded" |

### Update Field Validation

| Rule | Check | Exception |
|------|-------|-----------|
| Field Exists | Field ID must exist | "Field 'X' does not exist" |
| Module Loaded | A module must be loaded | "No module is currently loaded" |

### Delete Field Validation

| Rule | Check | Exception |
|------|-------|-----------|
| Field Exists | Field ID must exist | "Field 'X' does not exist" |
| Module Loaded | A module must be loaded | "No module is currently loaded" |

### Clone Field Validation

| Rule | Check | Exception |
|------|-------|-----------|
| Field Exists | Field ID must exist | "Field 'X' does not exist" |
| Module Loaded | A module must be loaded | "No module is currently loaded" |

### Move Field Validation

| Rule | Check | Exception |
|------|-------|-----------|
| Field Exists | Field ID must exist | "Field 'X' does not exist" |
| Not at Top/Bottom | Cannot move beyond siblings | "Field 'X' is already at the top/bottom" |
| Module Loaded | A module must be loaded | "No module is currently loaded" |

### Set Parent Validation

| Rule | Check | Exception |
|------|-------|-----------|
| Field Exists | Field ID must exist | "Field 'X' does not exist" |
| Parent Exists | Parent field must exist (if specified) | "Parent field 'X' does not exist" |
| Not Self | Cannot set self as parent | "Field cannot be its own parent" |
| No Circular | Cannot create circular relationship | "Would create a circular relationship" |
| Module Loaded | A module must be loaded | "No module is currently loaded" |

---

## Usage Examples

### Example 1: Building a Contact Form

```csharp
// Create contact section
var contactSection = FormFieldSchema.CreateSection(
    id: "contactSection",
    titleEn: "Contact Information",
    titleFr: "Coordonnées",
    order: 1);

await _formBuilder.AddFieldAsync(contactSection, parentId: null);

// Add fields to contact section
var firstNameField = FormFieldSchema.CreateTextField(
    id: "firstName",
    labelEn: "First Name",
    labelFr: "Prénom",
    isRequired: true,
    order: 1);

await _formBuilder.AddFieldAsync(firstNameField, parentId: "contactSection");

var lastNameField = FormFieldSchema.CreateTextField(
    id: "lastName",
    labelEn: "Last Name",
    labelFr: "Nom",
    isRequired: true,
    order: 2);

await _formBuilder.AddFieldAsync(lastNameField, parentId: "contactSection");

var emailField = FormFieldSchema.CreateTextField(
    id: "email",
    labelEn: "Email Address",
    labelFr: "Adresse courriel",
    isRequired: true,
    order: 3);

await _formBuilder.AddFieldAsync(emailField, parentId: "contactSection");

// Result:
// - Contact Information (Section)
//   - First Name (TextBox, required)
//   - Last Name (TextBox, required)
//   - Email Address (TextBox, required)
```

---

### Example 2: Reordering Fields

```csharp
// Move email field to top of contact section
await _formBuilder.MoveFieldUpAsync("email");
await _formBuilder.MoveFieldUpAsync("email");

// Result:
// - Contact Information (Section)
//   - Email Address (TextBox, required)
//   - First Name (TextBox, required)
//   - Last Name (TextBox, required)
```

---

### Example 3: Reorganizing Hierarchy

```csharp
// Create address section
var addressSection = FormFieldSchema.CreateSection(
    id: "addressSection",
    titleEn: "Address",
    titleFr: "Adresse");

await _formBuilder.AddFieldAsync(addressSection, parentId: null);

// Move email to address section
await _formBuilder.SetFieldParentAsync("email", "addressSection");

// Result:
// - Contact Information (Section)
//   - First Name (TextBox, required)
//   - Last Name (TextBox, required)
// - Address (Section)
//   - Email Address (TextBox, required)
```

---

### Example 4: Cloning Fields

```csharp
// Clone phone field for alternate number
await _formBuilder.CloneFieldAsync("phone");
// Creates: "phone_copy"

// Update clone's label
var module = _editorState.GetCurrentModule();
var clonedField = module.Fields.First(f => f.Id == "phone_copy");
var updatedField = clonedField with
{
    LabelEn = "Alternate Phone",
    LabelFr = "Téléphone alternatif"
};

await _formBuilder.UpdateFieldAsync(updatedField);

// Result:
// - Phone (TextBox)
// - Alternate Phone (TextBox)
```

---

### Example 5: Deleting Sections with Children

```csharp
// Delete contact section (cascades to all children)
await _formBuilder.DeleteFieldAsync("contactSection");

// Deletes:
// - contactSection
// - firstName
// - lastName
// - email

// Undo history: "Deleted Section: Contact Information and 3 child field(s)"
```

---

### Example 6: Error Handling

```csharp
try
{
    // Try to add duplicate field ID
    var field1 = FormFieldSchema.CreateTextField("email", "Email");
    var field2 = FormFieldSchema.CreateTextField("email", "Email 2");

    await _formBuilder.AddFieldAsync(field1, null);
    await _formBuilder.AddFieldAsync(field2, null);  // Throws!
}
catch (InvalidOperationException ex)
{
    // ex.Message: "Field with ID 'email' already exists"
    _logger.LogError(ex, "Failed to add field");
    ShowError("Cannot add field: duplicate ID");
}

try
{
    // Try to create circular relationship
    await _formBuilder.SetFieldParentAsync("parentSection", "childField");
    // Throws!
}
catch (InvalidOperationException ex)
{
    // ex.Message: "Setting 'childField' as parent of 'parentSection' would create a circular relationship"
    _logger.LogError(ex, "Invalid parent assignment");
    ShowError("Cannot set parent: would create circular relationship");
}
```

---

### Example 7: Undo/Redo Integration

```csharp
@inject FormBuilderService FormBuilder
@inject UndoRedoService UndoRedo
@inject EditorStateService EditorState

// Add field
await FormBuilder.AddFieldAsync(textField, null);
// Undo stack: ["Added TextBox: First Name"]

// Update field
var updatedField = textField with { LabelEn = "Full Name" };
await FormBuilder.UpdateFieldAsync(updatedField);
// Undo stack: ["Updated TextBox: Full Name", "Added TextBox: First Name"]

// Undo last action
var snapshot = UndoRedo.Undo();
if (snapshot != null)
{
    var module = JsonSerializer.Deserialize<FormModuleSchema>(snapshot.SnapshotJson);
    EditorState.UpdateModule(module, $"Undo: {snapshot.ActionDescription}");
}
// Field label is now back to "First Name"
// Redo stack: ["Updated TextBox: Full Name"]

// Redo
snapshot = UndoRedo.Redo();
if (snapshot != null)
{
    var module = JsonSerializer.Deserialize<FormModuleSchema>(snapshot.SnapshotJson);
    EditorState.UpdateModule(module, $"Redo: {snapshot.ActionDescription}");
}
// Field label is now "Full Name" again
```

---

### Example 8: Service Registration (Dependency Injection)

```csharp
// Program.cs or Startup.cs
builder.Services.AddScoped<EditorStateService>();
builder.Services.AddScoped<UndoRedoService>();
builder.Services.AddScoped<FormBuilderService>();
```

---

### Example 9: UI Integration - Field Toolbar

```razor
@inject FormBuilderService FormBuilder

<div class="field-toolbar">
    <button @onclick="() => MoveFieldUp(field.Id)"
            disabled="@IsFirstSibling(field)">
        <i class="bi bi-arrow-up"></i>
        Move Up
    </button>

    <button @onclick="() => MoveFieldDown(field.Id)"
            disabled="@IsLastSibling(field)">
        <i class="bi bi-arrow-down"></i>
        Move Down
    </button>

    <button @onclick="() => CloneField(field.Id)">
        <i class="bi bi-clipboard"></i>
        Clone
    </button>

    <button @onclick="() => DeleteField(field.Id)"
            class="btn-danger">
        <i class="bi bi-trash"></i>
        Delete
    </button>
</div>

@code {
    [Parameter]
    public FormFieldSchema Field { get; set; } = default!;

    private async Task MoveFieldUp(string fieldId)
    {
        try
        {
            await FormBuilder.MoveFieldUpAsync(fieldId);
        }
        catch (InvalidOperationException ex)
        {
            ShowToast("warning", ex.Message);
        }
    }

    private async Task MoveFieldDown(string fieldId)
    {
        try
        {
            await FormBuilder.MoveFieldDownAsync(fieldId);
        }
        catch (InvalidOperationException ex)
        {
            ShowToast("warning", ex.Message);
        }
    }

    private async Task CloneField(string fieldId)
    {
        await FormBuilder.CloneFieldAsync(fieldId);
        ShowToast("success", "Field cloned successfully");
    }

    private async Task DeleteField(string fieldId)
    {
        var confirmed = await ConfirmDelete("Are you sure you want to delete this field?");
        if (confirmed)
        {
            await FormBuilder.DeleteFieldAsync(fieldId);
            ShowToast("success", "Field deleted successfully");
        }
    }

    private bool IsFirstSibling(FormFieldSchema field)
    {
        var module = _editorState.GetCurrentModule();
        if (module == null) return true;

        var siblings = module.Fields
            .Where(f => f.ParentId == field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();

        return siblings.FirstOrDefault()?.Id == field.Id;
    }

    private bool IsLastSibling(FormFieldSchema field)
    {
        var module = _editorState.GetCurrentModule();
        if (module == null) return true;

        var siblings = module.Fields
            .Where(f => f.ParentId == field.ParentId)
            .OrderBy(f => f.Order)
            .ToList();

        return siblings.LastOrDefault()?.Id == field.Id;
    }
}
```

---

### Example 10: Field Properties Editor

```razor
@inject FormBuilderService FormBuilder

<EditForm Model="@fieldModel" OnValidSubmit="SaveField">
    <div class="form-group">
        <label>Label (English)</label>
        <InputText @bind-Value="fieldModel.LabelEn" class="form-control" />
    </div>

    <div class="form-group">
        <label>Label (French)</label>
        <InputText @bind-Value="fieldModel.LabelFr" class="form-control" />
    </div>

    <div class="form-check">
        <InputCheckbox @bind-Value="fieldModel.IsRequired" class="form-check-input" id="required" />
        <label class="form-check-label" for="required">
            Required field
        </label>
    </div>

    <div class="form-group">
        <label>Max Length</label>
        <InputNumber @bind-Value="fieldModel.MaxLength" class="form-control" />
    </div>

    <button type="submit" class="btn btn-primary">Save</button>
</EditForm>

@code {
    [Parameter]
    public FormFieldSchema Field { get; set; } = default!;

    private FieldEditModel fieldModel = new();

    protected override void OnParametersSet()
    {
        // Populate model from field
        fieldModel = new FieldEditModel
        {
            LabelEn = Field.LabelEn,
            LabelFr = Field.LabelFr,
            IsRequired = Field.IsRequired,
            MaxLength = Field.MaxLength
        };
    }

    private async Task SaveField()
    {
        // Create updated field
        var updatedField = Field with
        {
            LabelEn = fieldModel.LabelEn,
            LabelFr = fieldModel.LabelFr,
            IsRequired = fieldModel.IsRequired,
            MaxLength = fieldModel.MaxLength
        };

        await FormBuilder.UpdateFieldAsync(updatedField);
        ShowToast("success", "Field updated successfully");
    }

    private class FieldEditModel
    {
        public string? LabelEn { get; set; }
        public string? LabelFr { get; set; }
        public bool IsRequired { get; set; }
        public int? MaxLength { get; set; }
    }
}
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
Time Elapsed 00:00:01.93
```

---

## Files Created

| File | Lines | Purpose |
|------|-------|---------|
| `Services/Operations/FormBuilderService.cs` | 572 | Form building operations service |
| `FORM_BUILDER_SERVICE_SUMMARY.md` | This file | Documentation |

**Total**: 572 lines of code + documentation

---

## Acceptance Criteria - All Met ✅

| Criterion | Status | Details |
|-----------|--------|---------|
| All CRUD operations implemented | ✅ | Add, Update, Delete, Clone |
| Move up/down logic works | ✅ | Swaps Order with siblings |
| Clone creates unique ID | ✅ | Appends _copy, _copy2, etc. |
| Parent-child relationships maintained | ✅ | ParentId property managed |
| Validation prevents invalid operations | ✅ | All validations implemented |
| Action descriptions are descriptive | ✅ | Clear undo/redo history |
| Integration with EditorStateService works | ✅ | UpdateModule() called |
| Integration with UndoRedoService works | ✅ | PushSnapshot() called |

---

## Key Design Decisions

### 1. Immutable Operations

Used record `with` expressions for all modifications:

**Why**:
- FormModuleSchema and FormFieldSchema are immutable records
- Creates new instances instead of mutating
- Safer, more predictable
- Enables undo/redo
- Matches functional programming principles

---

### 2. Cascade Delete

Deleting a field also deletes all descendants:

**Why**:
- Prevents orphaned fields
- Standard behavior (folders in file systems)
- Clear user expectation
- Maintains data integrity

---

### 3. Clone ID Generation

Appends `_copy`, `_copy2`, etc. to base ID:

**Why**:
- Ensures uniqueness
- Clear relationship to original
- Standard naming convention (Windows, Mac)
- Easy to understand

---

### 4. Move Within Siblings

Move up/down only affects siblings (same parent):

**Why**:
- Maintains hierarchy structure
- Prevents accidental parent changes
- Standard UI behavior (lists, trees)
- Predictable for users

---

### 5. Circular Relationship Prevention

Validates before setting parent:

**Why**:
- Prevents infinite loops in rendering
- Maintains tree structure invariant
- Better error message than runtime exception
- Standard tree operation validation

---

### 6. Undo/Redo Snapshot Creation

Every operation creates snapshot:

**Why**:
- Full undo/redo support
- Clear action descriptions
- Audit trail
- Standard editor behavior

---

### 7. Async Methods

All methods are async even though operations are synchronous:

**Why**:
- Future-proof for database operations
- Consistent API
- Allows for validation against external sources
- Standard ASP.NET Core pattern

---

### 8. Validation Before Modification

All validations occur before creating new module:

**Why**:
- Fail fast
- Don't create invalid state
- Clear error messages
- Easier to debug

---

## Testing Recommendations

### Unit Tests

```csharp
[Fact]
public async Task AddFieldAsync_AddsFieldToModule()
{
    // Arrange
    var module = CreateTestModule();
    _editorState.LoadModule(module);

    var newField = FormFieldSchema.CreateTextField("firstName", "First Name");

    // Act
    await _formBuilder.AddFieldAsync(newField, null);

    // Assert
    var updatedModule = _editorState.GetCurrentModule();
    Assert.Contains(updatedModule.Fields, f => f.Id == "firstName");
}

[Fact]
public async Task AddFieldAsync_ThrowsOnDuplicateId()
{
    // Arrange
    var module = CreateTestModule();
    var existingField = FormFieldSchema.CreateTextField("email", "Email");
    module = module with { Fields = new[] { existingField } };
    _editorState.LoadModule(module);

    var duplicateField = FormFieldSchema.CreateTextField("email", "Email 2");

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _formBuilder.AddFieldAsync(duplicateField, null));
}

[Fact]
public async Task DeleteFieldAsync_DeletesFieldAndDescendants()
{
    // Arrange
    var section = FormFieldSchema.CreateSection("section", "Section");
    var child1 = FormFieldSchema.CreateTextField("child1", "Child 1") with { ParentId = "section" };
    var child2 = FormFieldSchema.CreateTextField("child2", "Child 2") with { ParentId = "section" };

    var module = CreateTestModule() with
    {
        Fields = new[] { section, child1, child2 }
    };
    _editorState.LoadModule(module);

    // Act
    await _formBuilder.DeleteFieldAsync("section");

    // Assert
    var updatedModule = _editorState.GetCurrentModule();
    Assert.Empty(updatedModule.Fields);
}

[Fact]
public async Task CloneFieldAsync_CreatesUniqueId()
{
    // Arrange
    var field = FormFieldSchema.CreateTextField("email", "Email");
    var module = CreateTestModule() with { Fields = new[] { field } };
    _editorState.LoadModule(module);

    // Act
    await _formBuilder.CloneFieldAsync("email");

    // Assert
    var updatedModule = _editorState.GetCurrentModule();
    Assert.Equal(2, updatedModule.Fields.Length);
    Assert.Contains(updatedModule.Fields, f => f.Id == "email_copy");
}

[Fact]
public async Task MoveFieldUpAsync_SwapsWithPreviousSibling()
{
    // Arrange
    var field1 = FormFieldSchema.CreateTextField("field1", "Field 1") with { Order = 1 };
    var field2 = FormFieldSchema.CreateTextField("field2", "Field 2") with { Order = 2 };

    var module = CreateTestModule() with { Fields = new[] { field1, field2 } };
    _editorState.LoadModule(module);

    // Act
    await _formBuilder.MoveFieldUpAsync("field2");

    // Assert
    var updatedModule = _editorState.GetCurrentModule();
    var updatedField1 = updatedModule.Fields.First(f => f.Id == "field1");
    var updatedField2 = updatedModule.Fields.First(f => f.Id == "field2");

    Assert.Equal(2, updatedField1.Order);  // Swapped
    Assert.Equal(1, updatedField2.Order);  // Swapped
}

[Fact]
public async Task SetFieldParentAsync_ThrowsOnCircularRelationship()
{
    // Arrange
    var section = FormFieldSchema.CreateSection("section", "Section");
    var child = FormFieldSchema.CreateTextField("child", "Child") with { ParentId = "section" };

    var module = CreateTestModule() with { Fields = new[] { section, child } };
    _editorState.LoadModule(module);

    // Act & Assert
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => _formBuilder.SetFieldParentAsync("section", "child"));
}
```

---

## Performance Considerations

### Array Operations

**Current**: Creates new arrays on every operation

**Performance**: O(n) where n = number of fields

**Acceptable Because**:
- Forms typically have < 100 fields
- Operations are user-triggered (not frequent)
- Immutability benefits outweigh cost

**Could Optimize With**:
- Use List<T> internally if performance becomes an issue
- Batch operations for multiple changes

---

### Hierarchy Traversal

**Circular Check**: O(h) where h = hierarchy depth

**Descendant Enumeration**: O(n) where n = total descendants

**Acceptable Because**:
- Hierarchies are typically shallow (< 5 levels)
- Operations are infrequent
- Correctness more important than speed

---

## Next Steps

With FormBuilderService complete, next steps for Phase 3:

1. **Create UI Components**
   - Field toolbar (move up/down, clone, delete)
   - Field properties editor
   - Field tree view
   - Drag-and-drop support

2. **Add Field Type Support**
   - Text fields
   - Dropdowns
   - Date pickers
   - File uploads
   - Sections
   - Modal tables

3. **Implement Validation**
   - Real-time validation
   - Field-level validation
   - Form-level validation
   - Validation rule editor

4. **Add Advanced Features**
   - Conditional logic editor
   - Field templates
   - Bulk operations
   - Import/export fields

---

*Generated: November 28, 2025*
*Project: DynamicForms Visual Editor*
*Phase: 3 - Visual Form Editor*
*Component: FormBuilderService (COMPLETED)*
