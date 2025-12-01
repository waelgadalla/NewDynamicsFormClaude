# CodeSet Support in DynamicForms.Core.V2

## Overview

CodeSets provide **centralized, reusable collections of options** for dropdown lists, radio buttons, and other selection-based form fields. Instead of defining options inline in each field, you can reference a shared CodeSet that can be maintained independently.

## Key Benefits

? **Reusability** - Define options once, use across multiple forms  
? **Maintainability** - Update options in one place, affects all references  
? **Consistency** - Same terminology across your application  
? **Flexibility** - No database required - use in-memory, JSON files, database, or API  
? **Multilingual** - Built-in English/French support  

---

## Quick Start

### 1. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using DynamicForms.Core.V2.Extensions;

// Option A: Use default in-memory provider
builder.Services.AddDynamicFormsV2();

// Option B: Use custom CodeSet provider (e.g., database)
builder.Services.AddDynamicFormsV2WithCodeSetProvider<MyDatabaseCodeSetProvider>();

// Option C: No CodeSet support
builder.Services.AddDynamicFormsV2WithoutCodeSets();
```

### 2. Register Sample CodeSets

If using the in-memory provider, register your CodeSets at startup:

```csharp
using DynamicForms.Core.V2.Services;

// Get the in-memory provider
var codeSetProvider = app.Services.GetRequiredService<ICodeSetProvider>() 
    as InMemoryCodeSetProvider;

// Register sample CodeSets
if (codeSetProvider != null)
{
    codeSetProvider.RegisterCodeSets(SampleCodeSets.GetSampleCodeSets());
}
```

### 3. Use CodeSets in Your Forms

**Option A: Reference a CodeSet by ID**

```json
{
  "Id": "province",
  "FieldType": "DropDown",
  "LabelEn": "Province",
  "CodeSetId": 1,
  "IsRequired": true
}
```

**Option B: Use inline options (no CodeSet)**

```json
{
  "Id": "org_type",
  "FieldType": "DropDown",
  "LabelEn": "Organization Type",
  "Options": [
  { "Value": "individual", "LabelEn": "Individual" },
    { "Value": "business", "LabelEn": "Business" }
  ]
}
```

### 4. Build Hierarchy (CodeSets Resolved Automatically)

```csharp
var hierarchyService = serviceProvider.GetRequiredService<IFormHierarchyService>();
var runtime = await hierarchyService.BuildHierarchyAsync(schema);

// CodeSets are automatically resolved during hierarchy building
var fieldNode = runtime.GetField("province");
var options = fieldNode.GetEffectiveOptions(); // Returns resolved CodeSet options
```

---

## Architecture

### Schema Layer (Immutable)

```
CodeSetSchema (record)
  ??? Id, Code, NameEn, NameFr
  ??? Category, Tags, IsActive
  ??? Items: CodeSetItem[]
       ??? Value, TextEn, TextFr
       ??? Order, IsDefault, IsActive
       ??? CssClass, Metadata
```

### Service Layer (Abstraction)

```
ICodeSetProvider (interface)
  ??? GetCodeSetAsync(int codeSetId)
  ??? GetCodeSetByCodeAsync(string code)
  ??? GetAllCodeSetsAsync()
  ??? GetCodeSetAsFieldOptionsAsync(int codeSetId)
```

### Runtime Layer

```
FormFieldNode
  ??? Schema (FormFieldSchema)
  ??? ResolvedOptions (from CodeSet)
  ??? GetEffectiveOptions() ? FieldOption[]
```

---

## Available Implementations

### InMemoryCodeSetProvider (Default)

**Use when:**
- You don't want database dependency yet
- You're testing or prototyping
- Your CodeSets are loaded at startup

**Features:**
- Fast dictionary-based lookup
- Register/unregister CodeSets at runtime
- No I/O overhead

**Example:**

```csharp
var provider = new InMemoryCodeSetProvider(logger);

// Register sample CodeSets
provider.RegisterCodeSets(SampleCodeSets.GetSampleCodeSets());

// Or register custom CodeSet
var customCodeSet = CodeSetSchema.Create(
    id: 100,
    code: "COUNTRIES",
    nameEn: "Countries",
    items: new[]
    {
        new CodeSetItem { Value = "CA", TextEn = "Canada", Order = 1 },
        new CodeSetItem { Value = "US", TextEn = "United States", Order = 2 }
    });

provider.RegisterCodeSet(customCodeSet);

// Query
var options = await provider.GetCodeSetAsFieldOptionsAsync(100);
```

### Custom Implementation (Database, API, Files)

Implement `ICodeSetProvider` to load from any source:

```csharp
public class DatabaseCodeSetProvider : ICodeSetProvider
{
    private readonly IDbConnection _db;
    
    public async Task<CodeSetSchema?> GetCodeSetAsync(
        int codeSetId, 
        CancellationToken ct = default)
    {
  // Load from database
 var record = await _db.QuerySingleOrDefaultAsync<CodeSetRecord>(
            "SELECT * FROM CodeSets WHERE Id = @Id", 
       new { Id = codeSetId });

        if (record == null) return null;
        
     var items = await _db.QueryAsync<CodeSetItemRecord>(
  "SELECT * FROM CodeSetItems WHERE CodeSetId = @CodeSetId ORDER BY [Order]",
        new { CodeSetId = codeSetId });
          
      return MapToSchema(record, items);
    }
    
    // Implement other methods...
}
```

Then register it:

```csharp
builder.Services.AddDynamicFormsV2WithCodeSetProvider<DatabaseCodeSetProvider>();
```

---

## Sample CodeSets Included

### 1. CanadianProvinces (ID: 1)
```
Code: PROVINCES_CA
Items: AB, BC, MB, NB, NL, NS, ON, PE, QC, SK, NT, NU, YT
Category: Geography
```

### 2. OrganizationTypes (ID: 99)
```
Code: ORG_TYPES
Items: Individual, Non-Profit, Business, Government
Category: Organizations
```

### 3. YesNoOptions (ID: 2)
```
Code: YES_NO
Items: Yes, No
Category: Common
```

### 4. ProjectStatuses (ID: 10)
```
Code: PROJECT_STATUS
Items: Draft, Submitted, Under Review, Approved, Rejected, Completed
Category: Workflow
```

### 5. FundingRanges (ID: 20)
```
Code: FUNDING_RANGES
Items: Up to $5K, $5K-$25K, $25K-$50K, $50K-$100K, Over $100K
Category: Funding
```

---

## Schema Examples

### Example 1: Dropdown with CodeSet

```json
{
  "Id": "org_type",
  "ParentId": "sec_contact",
  "FieldType": "DropDown",
  "LabelEn": "Organization Type",
  "LabelFr": "Type d'organisation",
  "CodeSetId": 99,
  "IsRequired": true,
  "Order": 12
}
```

### Example 2: Radio Buttons with CodeSet

```json
{
  "Id": "has_funding",
  "FieldType": "RadioButtonList",
  "LabelEn": "Do you have other funding?",
  "CodeSetId": 2,
  "Order": 20
}
```

### Example 3: Checkbox List with Inline Options (No CodeSet)

```json
{
  "Id": "project_types",
  "FieldType": "CheckBoxList",
  "LabelEn": "Project Types",
  "Options": [
    { "Value": "research", "LabelEn": "Research", "Order": 1 },
    { "Value": "community", "LabelEn": "Community", "Order": 2 },
    { "Value": "education", "LabelEn": "Education", "Order": 3 }
  ]
}
```

---

## Factory Methods

### Create Fields with CodeSets

```csharp
// Using CodeSet
var provinceField = FormFieldSchema.CreateDropDownFromCodeSet(
    id: "province",
    labelEn: "Province/Territory",
    codeSetId: 1,
    labelFr: "Province/Territoire",
    isRequired: true,
    order: 5);

// Using inline options
var statusField = FormFieldSchema.CreateDropDown(
    id: "status",
    labelEn: "Status",
 options: new[]
    {
        new FieldOption("draft", "Draft", "Brouillon", false, 1),
    new FieldOption("active", "Active", "Actif", true, 2)
    });
```

### Create Custom CodeSets

```csharp
var countriesCodeSet = CodeSetSchema.Create(
    id: 50,
    code: "COUNTRIES",
nameEn: "Countries",
    items: new[]
    {
        new CodeSetItem
        {
   Value = "CA",
    TextEn = "Canada",
      TextFr = "Canada",
            IsDefault = true,
       Order = 1
},
        new CodeSetItem
        {
    Value = "US",
            TextEn = "United States",
   TextFr = "États-Unis",
            Order = 2
   }
 })
{
    NameFr = "Pays",
    Category = "Geography",
    Tags = new[] { "geography", "countries" }
};
```

---

## Working with Resolved Options

### In Blazor Components

```razor
@inject IFormHierarchyService HierarchyService

<select @bind="selectedValue">
    @if (fieldNode?.GetEffectiveOptions() is { } options)
    {
        foreach (var option in options)
  {
   <option value="@option.Value">
    @(language == "FR" ? option.LabelFr ?? option.LabelEn : option.LabelEn)
            </option>
        }
    }
</select>

@code {
    [Parameter] public FormFieldNode? fieldNode { get; set; }
    [Parameter] public string language { get; set; } = "EN";
    private string? selectedValue;
}
```

### In Validation

```csharp
public class DropDownValidationRule : IValidationRule
{
    public Task<ValidationResult> ValidateAsync(
    FormFieldNode field,
        object? value,
     Dictionary<string, object?> formData,
        CancellationToken ct = default)
    {
  var options = field.GetEffectiveOptions();
        if (options == null) 
            return Task.FromResult(ValidationResult.Success());

        var stringValue = value?.ToString();
        var isValid = options.Any(o => o.Value == stringValue);

    return isValid
          ? Task.FromResult(ValidationResult.Success())
       : Task.FromResult(new ValidationResult(
    false,
         new[] { new ValidationError(field.Schema.Id, "Invalid selection") }));
    }
}
```

---

## Migration from V1

If you're migrating from DynamicForms.Core (V1):

### V1 Schema (Old)
```json
{
  "Id": "province",
  "CodeSetId": 42,
  "Options": []  // Ignored if CodeSetId is set
}
```

### V2 Schema (New)
```json
{
  "Id": "province",
  "FieldType": "DropDown",
  "LabelEn": "Province",
  "CodeSetId": 42
  // No need to include Options array
}
```

### Key Differences

| Feature | V1 (Core) | V2 (Core.V2) |
|---------|-----------|--------------|
| Schema Type | Mutable class | Immutable record |
| Resolution | Manual service calls | Automatic during hierarchy build |
| Options Storage | Mixed (CodeSet + inline) | Runtime: `ResolvedOptions` property |
| Provider Pattern | No abstraction | `ICodeSetProvider` interface |
| Default Storage | Not provided | `InMemoryCodeSetProvider` |

---

## Best Practices

### ? DO

- Use CodeSets for:
  - Standard lists (provinces, countries, statuses)
  - Lists shared across multiple forms
  - Lists that change frequently
  
- Use inline options for:
  - Simple yes/no choices
  - Field-specific options
  - One-off selections

- Cache CodeSets at application startup for performance

- Use meaningful CodeSet codes (`PROVINCES_CA`, not `CS_001`)

### ? DON'T

- Mix CodeSetId and Options in the same field (CodeSetId takes priority)
- Store user-generated data in CodeSets (use proper data tables)
- Create CodeSets with only 2-3 items (use inline options instead)

---

## Advanced Scenarios

### Hierarchical CodeSets

```csharp
var hierarchicalCodeSet = new[]
{
  new CodeSetItem 
    { 
        Value = "mammals", 
        TextEn = "Mammals",
        Order = 1 
    },
    new CodeSetItem 
    { 
    Value = "dog", 
      TextEn = "Dog",
ParentValue = "mammals",  // Child of "mammals"
   Order = 2 
    },
    new CodeSetItem 
 { 
   Value = "cat", 
        TextEn = "Cat",
        ParentValue = "mammals",
  Order = 3 
    }
};
```

### CodeSets with Metadata

```csharp
new CodeSetItem
{
    Value = "premium",
    TextEn = "Premium Membership",
    Metadata = new Dictionary<string, object>
    {
        ["Price"] = 99.99,
        ["Features"] = new[] { "Feature1", "Feature2" },
        ["PopularityScore"] = 85
    }
}
```

### Dynamic CodeSet Loading

```csharp
// Load CodeSets from JSON files
public class JsonFileCodeSetProvider : ICodeSetProvider
{
    private readonly string _basePath;
    
    public async Task<CodeSetSchema?> GetCodeSetAsync(
        int codeSetId, 
        CancellationToken ct)
    {
 var path = Path.Combine(_basePath, $"{codeSetId}.json");
        if (!File.Exists(path)) return null;
        
        var json = await File.ReadAllTextAsync(path, ct);
        return JsonSerializer.Deserialize<CodeSetSchema>(json);
    }
}
```

---

## Troubleshooting

### Problem: CodeSet options not showing

**Solution:** Ensure:
1. `ICodeSetProvider` is registered in DI
2. CodeSets are registered before building hierarchy
3. Field has `CodeSetId` set (not just empty `Options` array)

```csharp
// Check if CodeSet exists
var provider = services.GetRequiredService<ICodeSetProvider>();
var exists = await provider.CodeSetExistsAsync(99);
if (!exists)
{
    logger.LogWarning("CodeSet 99 not found!");
}
```

### Problem: Wrong options appearing

**Solution:** Check CodeSet resolution priority:

```csharp
// Priority: ResolvedOptions > Schema.Options
var effectiveOptions = fieldNode.GetEffectiveOptions();

// To debug:
Console.WriteLine($"CodeSetId: {fieldNode.Schema.CodeSetId}");
Console.WriteLine($"Resolved: {fieldNode.ResolvedOptions?.Length ?? 0}");
Console.WriteLine($"Inline: {fieldNode.Schema.Options?.Length ?? 0}");
```

### Problem: CodeSets not loading from database

**Solution:** Check your custom provider logs:

```csharp
public class DatabaseCodeSetProvider : ICodeSetProvider
{
    private readonly ILogger _logger;
    
    public async Task<CodeSetSchema?> GetCodeSetAsync(int id, CancellationToken ct)
    {
 _logger.LogDebug("Loading CodeSet {Id}", id);
        try
        {
            // Your code here
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "Failed to load CodeSet {Id}", id);
          return null;
  }
    }
}
```

---

## Future Enhancements

Planned features:

- [ ] CodeSet versioning
- [ ] Cascading dropdown support (parent-child relationships)
- [ ] CodeSet permissions/access control
- [ ] Caching strategies (distributed cache, Redis)
- [ ] CodeSet import/export utilities
- [ ] Admin UI for managing CodeSets
- [ ] Audit trail for CodeSet changes

---

## Summary

CodeSets in V2 provide a **flexible, database-agnostic** way to manage reusable form options:

1. **Simple to use** - Reference by ID or use inline options
2. **Flexible storage** - In-memory, database, files, or API
3. **Automatic resolution** - Happens during hierarchy building
4. **Type-safe** - Immutable records with compile-time safety
5. **Production-ready** - Logging, error handling, and extensibility built-in

Start with `InMemoryCodeSetProvider` for development, then migrate to database storage when ready!
