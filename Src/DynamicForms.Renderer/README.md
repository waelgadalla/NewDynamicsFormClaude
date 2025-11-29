# DynamicForms.Renderer - Blazor Form Renderer Library

## Overview

**DynamicForms.Renderer** is a Blazor Class Library that provides runtime rendering of dynamic forms based on JSON schemas defined in **DynamicForms.Core.V2**.

This library is responsible for:
- Rendering form fields from schema definitions
- Managing form state and user input
- Evaluating conditional logic (show/hide fields)
- Validating user input
- Handling form submission

**Technology**: Blazor Components (.NET 9.0)
**Created**: November 28, 2025
**Status**: ✅ Project structure created, ready for component development

---

## Project Structure

```
DynamicForms.Renderer/
├── Components/
│   ├── Fields/               # Individual field renderers
│   │   ├── TextFieldRenderer.razor
│   │   ├── TextAreaRenderer.razor
│   │   ├── DropDownRenderer.razor
│   │   ├── DatePickerRenderer.razor
│   │   ├── FileUploadRenderer.razor
│   │   ├── CheckBoxRenderer.razor
│   │   └── ...
│   ├── Containers/           # Container renderers (sections, tabs, panels)
│   │   ├── SectionRenderer.razor
│   │   ├── TabRenderer.razor
│   │   ├── PanelRenderer.razor
│   │   └── ...
│   ├── DynamicFormRenderer.razor    # Main form renderer component
│   └── DynamicFieldRenderer.razor   # Field routing component
├── Services/
│   ├── ConditionalLogicEngine.cs    # Evaluates conditional rules
│   └── ...
├── Models/
│   ├── FormData.cs                  # Form data storage
│   ├── RenderContext.cs             # Runtime rendering context
│   └── ...
├── wwwroot/
│   └── css/
│       └── dynamicforms.css         # Custom styles
├── _Imports.razor                   # Common Razor imports
└── DynamicForms.Renderer.csproj     # Project file
```

---

## Dependencies

### Project References
- **DynamicForms.Core.V2** - Schema definitions, runtime models, validation services

### NuGet Packages
- **Microsoft.AspNetCore.Components.Web** (9.0.10) - Blazor component framework

---

## Key Components (To Be Implemented)

### 1. Models

#### FormData.cs
Stores user input for all form fields:
- `Dictionary<string, object?>` for field values
- `GetValue<T>(fieldId)` - Type-safe value retrieval
- `SetValue(fieldId, value)` - Update field value
- `ToJson()` / `FromJson()` - Serialization

#### RenderContext.cs
Runtime rendering state:
- Field visibility state
- Field enabled state
- Form hierarchy reference
- Conditional logic evaluation

### 2. Services

#### ConditionalLogicEngine.cs
Evaluates conditional logic rules:
- `EvaluateCondition(rule, formData)` - Evaluate single condition
- `ShouldShowField(field, formData)` - Check if field should be visible
- `IsFieldEnabled(field, formData)` - Check if field should be enabled
- Supports all operators: Equals, NotEquals, Contains, GreaterThan, LessThan, IsEmpty, IsNotEmpty

### 3. Components

#### Field Renderers
Individual components for each field type:
- **TextFieldRenderer** - Text input
- **TextAreaRenderer** - Multi-line text
- **DropDownRenderer** - Select dropdown
- **DatePickerRenderer** - Date picker
- **FileUploadRenderer** - File upload
- **CheckBoxRenderer** - Checkbox

#### Container Renderers
Components for grouping fields:
- **SectionRenderer** - Bordered section with title
- **TabRenderer** - Tabbed interface
- **PanelRenderer** - Panel with header

#### Main Components
- **DynamicFormRenderer** - Top-level form component
- **DynamicFieldRenderer** - Routes to appropriate field renderer based on type

---

## Usage Example (Future)

```razor
@page "/render-form"
@inject IFormModuleService ModuleService

<DynamicFormRenderer
    Schema="@formSchema"
    InitialData="@existingData"
    OnSubmit="@HandleSubmit"
    OnCancel="@HandleCancel" />

@code {
    private FormModuleSchema? formSchema;
    private FormData? existingData;

    protected override async Task OnInitializedAsync()
    {
        // Load schema from service
        formSchema = await ModuleService.GetModuleSchemaAsync(moduleId);

        // Load existing data if editing
        existingData = await LoadExistingData();
    }

    private async Task HandleSubmit(FormData data)
    {
        // Process submitted data
        await SaveFormData(data);
    }

    private void HandleCancel()
    {
        // Handle cancel action
        NavigationManager.NavigateTo("/forms");
    }
}
```

---

## Styling

The renderer uses **Bootstrap 5** for styling with custom CSS classes:

### Custom CSS Classes

All custom styles are prefixed with `dynamic-`:

- `.dynamic-form-container` - Main form wrapper
- `.dynamic-field-group` - Individual field wrapper
- `.dynamic-field-label` - Field label
- `.dynamic-field-label.required` - Required field indicator (*)
- `.dynamic-field-input` - Input element
- `.dynamic-field-help` - Help text
- `.dynamic-field-error` - Validation error message
- `.dynamic-field-input.is-invalid` - Invalid input state
- `.dynamic-section` - Section container
- `.dynamic-panel` - Panel container
- `.dynamic-tabs` - Tab container
- `.dynamic-file-upload` - File upload wrapper
- `.dynamic-checkbox-wrapper` - Checkbox wrapper
- `.dynamic-form-actions` - Submit/Cancel buttons wrapper

### Using Custom Styles

Include the CSS in your Blazor app:

```html
<!-- In App.razor or _Layout.cshtml -->
<link href="_content/DynamicForms.Renderer/css/dynamicforms.css" rel="stylesheet" />
```

---

## Features (Planned)

### ✅ Completed
- [x] Project structure created
- [x] Folder organization
- [x] Dependencies configured
- [x] Custom CSS styles created
- [x] _Imports.razor configured

### ⏭️ To Be Implemented
- [ ] FormData model
- [ ] RenderContext model
- [ ] ConditionalLogicEngine service
- [ ] Base field renderer
- [ ] Text field renderers (TextBox, TextArea, DropDown)
- [ ] Date/File/Checkbox renderers
- [ ] Container renderers (Section, Tab, Panel)
- [ ] DynamicFormRenderer main component
- [ ] DynamicFieldRenderer routing component

---

## Development Workflow

### Adding a New Field Renderer

1. Create new component in `Components/Fields/`
2. Inherit from `FieldRendererBase` (when created)
3. Implement rendering logic
4. Add to `DynamicFieldRenderer` switch statement
5. Style with Bootstrap 5 + custom CSS

### Adding a New Container Renderer

1. Create new component in `Components/Containers/`
2. Implement recursive rendering of child fields
3. Add conditional visibility support
4. Style appropriately

---

## Build Status

**Last Build**: November 28, 2025
**Status**: ✅ Build succeeded (0 errors, 0 warnings)

### Build Command
```bash
dotnet build Src/DynamicForms.Renderer/DynamicForms.Renderer.csproj
```

---

## Next Steps

Follow the implementation prompts in sequence:

1. **Prompt 2.1** ✅ - Create Renderer Project Structure (COMPLETED)
2. **Prompt 2.2** ⏭️ - Create FormData and RenderContext Models
3. **Prompt 2.3** - Create Conditional Logic Engine
4. **Prompt 2.4** - Create Base Field Renderer
5. **Prompt 2.5** - Create Field Renderers (Text, TextArea, DropDown)
6. **Prompt 2.6** - Create Field Renderers (Date, File, Checkbox)
7. **Prompt 2.7** - Create Container Renderers (Section, Tab, Panel)
8. **Prompt 2.8** - Create Main DynamicFormRenderer Component

---

## References

- [DynamicForms.Core.V2 README](../DynamicForms.Core.V2/README.md) - Schema and runtime models
- [Visual Editor Design](../../VISUAL_EDITOR_DESIGN_PROPOSAL.md) - Overall architecture
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/) - Styling framework

---

*Generated: November 28, 2025*
*Project: DynamicForms Visual Editor*
*Phase: 2 - Form Renderer Library*
*Prompt: 2.1 - Project Structure Setup*
