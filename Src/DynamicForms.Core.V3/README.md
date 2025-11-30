# DynamicForms.Core.V2

A simplified, high-performance dynamic forms library for .NET 9.0 with multilingual support, hierarchical fields, and extensible validation.

## 🚀 Quick Start

### Installation

Reference the project in your application:

```xml
<ProjectReference Include="path/to/DynamicForms.Core.V2.csproj" />
```

### Register Services

```csharp
using DynamicForms.Core.V2.Extensions;

// In Startup.cs or Program.cs
services.AddDynamicFormsV2();
```

### Basic Usage

```csharp
using DynamicForms.Core.V2.Schemas;
using DynamicForms.Core.V2.Services;

// 1. Create a module schema
var module = FormModuleSchema.Create(1, "Grant Application");

var fields = new[]
{
    FormFieldSchema.CreateSection("contact_section", "Contact Information"),
    FormFieldSchema.CreateTextField("name", "Full Name", isRequired: true)
        with { ParentId = "contact_section" },
    FormFieldSchema.CreateTextField("email", "Email", isRequired: true)
        with { ParentId = "contact_section", ValidationRules = new[] { "email" } }
};

module = module with { Fields = fields };

// 2. Build the hierarchy
var runtime = await hierarchyService.BuildHierarchyAsync(module);

// 3. Validate form data
var formData = new Dictionary<string, object?>
{
    ["name"] = "John Doe",
    ["email"] = "john@example.com"
};

var result = await validationService.ValidateModuleAsync(runtime, formData);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.FieldId}: {error.Message}");
    }
}
```

## 📚 Core Concepts

### Schema vs Runtime

- **Schema** (`FormFieldSchema`, `FormModuleSchema`) - Immutable, serializable, stored in database
- **Runtime** (`FormFieldNode`, `FormModuleRuntime`) - Mutable navigation tree, built from schema

### Hierarchy

Fields are stored flat but organized hierarchically:

```csharp
// Flat storage
var fields = new[]
{
    CreateSection("section1", "Section"),      // Root
    CreateTextField("field1", "Field")         // Child
        with { ParentId = "section1" }
};

// Runtime hierarchy
var runtime = await hierarchyService.BuildHierarchyAsync(module);
// runtime.RootFields[0] has Children[0] pointing to field1
```

### Factory Methods

Convenient methods for common field types:

```csharp
// Text field
FormFieldSchema.CreateTextField(
    id: "name",
    labelEn: "Full Name",
    labelFr: "Nom complet",
    isRequired: true,
    order: 1
);

// Section (container)
FormFieldSchema.CreateSection(
    id: "section1",
    titleEn: "Personal Info",
    titleFr: "Informations personnelles",
    order: 1
);

// Dropdown
FormFieldSchema.CreateDropDown(
    id: "country",
    labelEn: "Country",
    options: new[]
    {
        new FieldOption("CA", "Canada", "Canada", IsDefault: true, Order: 0),
        new FieldOption("US", "United States", "États-Unis", Order: 1)
    },
    labelFr: "Pays",
    isRequired: true,
    order: 2
);
```

## 🎯 Features

### Multilingual Support

All text fields support English and French:

```csharp
var field = FormFieldSchema.CreateTextField("name", "Name", "Nom");
// field.LabelEn = "Name"
// field.LabelFr = "Nom"
```

### Validation

Built-in rules:
- `required` - Field must have a value
- `length` - Min/max length validation
- `pattern` - Regex pattern matching
- `email` - Email format validation

```csharp
var field = FormFieldSchema.CreateTextField("email", "Email", isRequired: true)
    with { ValidationRules = new[] { "email" } };
```

### Custom Validation Rules

```csharp
public class PhoneValidationRule : IValidationRule
{
    public string RuleId => "phone";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        // Your validation logic
        if (/* invalid */)
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(field.Schema.Id, "INVALID_PHONE", "Invalid phone number")
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

// Register the rule
validationService.RegisterRule("phone", new PhoneValidationRule());
```

### Type-Specific Configurations

```csharp
// File upload configuration
var fileField = new FormFieldSchema
{
    Id = "document",
    FieldType = "FileUpload",
    LabelEn = "Upload Document",
    TypeConfig = new FileUploadConfig(
        AllowedExtensions: new[] { ".pdf", ".docx" },
        MaxFileSizeBytes: 10_485_760, // 10 MB
        MaxFiles: 3,
        RequireVirusScan: true
    )
};

// Date range configuration
var dateField = new FormFieldSchema
{
    Id = "project_start",
    FieldType = "DatePicker",
    LabelEn = "Project Start Date",
    TypeConfig = new DateRangeConfig(
        MinDate: DateTime.Today,
        MaxDate: DateTime.Today.AddYears(1),
        AllowFutureDates: true,
        DateFormat: "yyyy-MM-dd"
    )
};
```

### Conditional Logic

```csharp
var charityNumber = FormFieldSchema.CreateTextField("charity_number", "Charity Number")
    with
    {
        ConditionalRules = new[]
        {
            new ConditionalRule(
                FieldId: "org_type",
                Operator: "equals",
                Value: "charity",
                Action: "show"
            )
        }
    };
// Field only shows when org_type = "charity"
```

## 🏗️ Advanced Usage

### Hierarchy Validation

```csharp
var validation = hierarchyService.ValidateHierarchy(module);

if (!validation.IsValid)
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}

foreach (var warning in validation.Warnings)
{
    Console.WriteLine($"Warning: {warning}");
}
```

### Auto-Fix Hierarchy Issues

```csharp
// Automatically repairs orphaned fields, circular references, etc.
var fixedModule = hierarchyService.FixHierarchyIssues(module);
```

### Calculate Metrics

```csharp
var metrics = hierarchyService.CalculateMetrics(module);

Console.WriteLine($"Total Fields: {metrics.TotalFields}");
Console.WriteLine($"Max Depth: {metrics.MaxDepth}");
Console.WriteLine($"Complexity Score: {metrics.ComplexityScore}");
```

### Navigation

```csharp
var runtime = await hierarchyService.BuildHierarchyAsync(module);

// Get a specific field
var field = runtime.GetField("email");

// Get all fields in depth-first order
foreach (var field in runtime.GetFieldsInOrder())
{
    Console.WriteLine($"{field.Path} - {field.Schema.LabelEn}");
}

// Navigate hierarchy
var node = runtime.GetField("some_field");
foreach (var ancestor in node.GetAllAncestors())
{
    Console.WriteLine($"Ancestor: {ancestor.Schema.Id}");
}

foreach (var descendant in node.GetAllDescendants())
{
    Console.WriteLine($"Descendant: {descendant.Schema.Id}");
}
```

## 💾 JSON Serialization

Schemas serialize cleanly to JSON:

```csharp
using System.Text.Json;

// Serialize
var json = JsonSerializer.Serialize(module, new JsonSerializerOptions
{
    WriteIndented = true
});

// Deserialize
var deserializedModule = JsonSerializer.Deserialize<FormModuleSchema>(json);
```

**Note:** Only schema classes serialize. Runtime classes (FormFieldNode, FormModuleRuntime) are NOT serialized.

## 🔌 Dependency Injection

### Basic Setup

```csharp
services.AddDynamicFormsV2();
```

This registers:
- `IFormHierarchyService` → `FormHierarchyService`
- `IFormValidationService` → `FormValidationService`
- All built-in validation rules

### With Custom Repository

```csharp
services.AddDynamicFormsV2<SqlServerFormModuleRepository>();
```

## 📊 Architecture

### Immutability

All schemas use `init-only` properties and C# records:

```csharp
// Create
var field = FormFieldSchema.CreateTextField("id", "Label");

// Modify with 'with' expression (creates new instance)
var modified = field with { IsRequired = true };

// Original is unchanged
Console.WriteLine(field.IsRequired); // false
Console.WriteLine(modified.IsRequired); // true
```

### Performance

- **O(1) field lookup** via `runtime.FieldNodes` dictionary
- **Efficient traversal** with depth-first iterator
- **Lazy hierarchy building** - only when needed
- **Immutable schemas** - thread-safe by default

## 🎓 Examples

See `Demos/QuickDemo/Program.cs` for a complete working example demonstrating:
- Schema creation
- JSON serialization/deserialization
- Hierarchy building
- Validation
- Navigation

## 📦 Package Information

- **Target Framework:** .NET 9.0
- **Dependencies:**
  - System.Text.Json 9.0.0
  - Microsoft.Extensions.Logging.Abstractions 9.0.0
  - Microsoft.Extensions.DependencyInjection.Abstractions 9.0.0

## 📝 Key Types Reference

| Type | Purpose | Serialized? |
|------|---------|-------------|
| `FormFieldSchema` | Field definition | ✅ Yes |
| `FormModuleSchema` | Module definition | ✅ Yes |
| `FormWorkflowSchema` | Workflow definition | ✅ Yes |
| `FormFieldNode` | Runtime navigation | ❌ No |
| `FormModuleRuntime` | Runtime module | ❌ No |
| `IFormHierarchyService` | Hierarchy management | N/A |
| `IFormValidationService` | Validation | N/A |
| `IValidationRule` | Custom rules | N/A |

## 🛠️ Best Practices

1. **Always build hierarchy before validation**
   ```csharp
   var runtime = await hierarchyService.BuildHierarchyAsync(module);
   await validationService.ValidateModuleAsync(runtime, formData);
   ```

2. **Use factory methods for common types**
   ```csharp
   FormFieldSchema.CreateTextField(...) // Good
   new FormFieldSchema { ... } // Also works, but more verbose
   ```

3. **Validate hierarchy during development**
   ```csharp
   var validation = hierarchyService.ValidateHierarchy(module);
   if (!validation.IsValid)
   {
       throw new InvalidOperationException($"Invalid hierarchy: {string.Join(", ", validation.Errors)}");
   }
   ```

4. **Register services as singletons** (done automatically with `AddDynamicFormsV2()`)

5. **Use `with` expressions for modifications**
   ```csharp
   var updated = field with { IsRequired = true, Order = 2 };
   ```

## 🐛 Troubleshooting

### Orphaned Fields

If a field references a non-existent parent, it becomes a root field:

```
warn: FormHierarchyService[0]
      Field 'child1' references non-existent parent 'missing'. Making it a root field.
```

**Solution:** Ensure ParentId references an existing field, or use `FixHierarchyIssues()`.

### Circular References

```
Error: Circular reference detected involving field 'field1'
```

**Solution:** Use `FixHierarchyIssues()` or manually correct parent references.

### Missing Validation Rules

```
warn: FormValidationService[0]
      Validation rule 'custom_rule' not found for field 'field1'
```

**Solution:** Register the rule: `validationService.RegisterRule("custom_rule", new CustomRule());`

## 📄 License

[Your License Here]

## 🤝 Contributing

[Your Contributing Guidelines Here]

---

**Version:** 2.0.0
**Last Updated:** November 2025
