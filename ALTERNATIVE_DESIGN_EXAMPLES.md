# Alternative Design - Practical Examples

This document shows practical examples of how the simplified design works in real-world scenarios.

---

## Example 1: Creating a Simple Form

### Current Design (Verbose & Complex)

```csharp
// Current: Lots of nested initialization
var module = new FormModule
{
    Id = 101,
    OpportunityId = 5,
    Version = 1.0f,
    Text = new FormModule.TextResource
    {
        Title = new TextClass { EN = "Application Form", FR = "Formulaire de demande" },
        Description = new TextClass { EN = "Complete this form", FR = "Remplissez ce formulaire" }
    },
    Database = new ModuleDatabase { TableName = "Applications" },
    Fields = new FormField[]
    {
        new FormField
        {
            Id = "firstName",
            FieldType = new FieldType { Type = "TextBox" },
            Order = 1,
            Text = new FormField.TextResource
            {
                Label = new TextClass { EN = "First Name", FR = "Prénom" },
                Description = new TextClass { EN = "Enter your first name", FR = "Entrez votre prénom" }
            },
            IsRequired = true,
            Database = new FieldDatabase { ColumnName = "FirstName" }
        }
    }
};

// REQUIRED: Manual initialization
module.EnsureInitialized();
module.RebuildFieldHierarchy();
```

### New Design (Clean & Simple)

```csharp
// New: Simple, fluent creation
var module = FormModuleSchema.Create(
    id: 101,
    titleEn: "Application Form",
    titleFr: "Formulaire de demande",
    opportunityId: 5
) with
{
    DescriptionEn = "Complete this form",
    DescriptionFr = "Remplissez ce formulaire",
    TableName = "Applications",
    Fields = new[]
    {
        FormFieldSchema.CreateTextField(
            id: "firstName",
            labelEn: "First Name",
            labelFr: "Prénom",
            isRequired: true,
            order: 1
        ) with
        {
            DescriptionEn = "Enter your first name",
            DescriptionFr = "Entrez votre prénom",
            ColumnName = "FirstName"
        }
    }
};

// No initialization needed! Ready to use.
```

---

## Example 2: Building Hierarchy

### Current Design

```csharp
// Current: Manual hierarchy building required
var module = JsonSerializer.Deserialize<FormModule>(json);
module.EnsureInitialized();  // Required!
module.RebuildFieldHierarchy(); // Required!

// Now you can use hierarchy
var rootFields = module.GetRootFields();
foreach (var field in rootFields)
{
    ProcessField(field);
    foreach (var child in field.ChildFields) // Runtime property
    {
        ProcessField(child);
    }
}
```

### New Design

```csharp
// New: Service builds hierarchy automatically
var schema = JsonSerializer.Deserialize<FormModuleSchema>(json);
// No manual initialization needed!

// Build hierarchy through service
var runtime = await _hierarchyService.BuildHierarchyAsync(schema);

// Clean, predictable navigation
foreach (var field in runtime.RootFields)
{
    ProcessField(field);
    foreach (var child in field.Children)
    {
        ProcessField(child);
    }
}

// Or use helper methods
var allFieldsOrdered = runtime.GetFieldsInOrder();
```

---

## Example 3: JSON Serialization/Deserialization

### Current Design Issues

```csharp
// Current: Circular reference problems
var module = new FormModule { /* ... */ };
module.RebuildFieldHierarchy();

// Parent/Child references create circular refs
// field.Parent.ChildFields contains field
// Must use JsonIgnore to prevent infinite loop

var json = JsonSerializer.Serialize(module);
// Result: Parent and ChildFields are NOT in JSON!

var deserialized = JsonSerializer.Deserialize<FormModule>(json);
// Problem: Parent and ChildFields are null!
// MUST call RebuildFieldHierarchy() or things break
```

### New Design (Clean)

```csharp
// New: Clean serialization, no circular refs
var schema = FormModuleSchema.Create(101, "Form");
var json = JsonSerializer.Serialize(schema);
// Result: Clean JSON with ParentId (no object references)

var deserialized = JsonSerializer.Deserialize<FormModuleSchema>(json);
// Perfect! Schema is complete and valid
// No initialization needed

// Build hierarchy when you need it
var runtime = await _hierarchyService.BuildHierarchyAsync(deserialized);
// Now you have full object graph
```

**JSON Output Comparison:**

**Current Design:**
```json
{
  "id": 101,
  "text": {
    "title": {
      "en": "Form",
      "fr": "Formulaire"
    }
  },
  "fields": [
    {
      "id": "field1",
      "text": {
        "label": {
          "en": "Name"
        }
      },
      "parentId": null
      // Parent and ChildFields are JsonIgnored!
    }
  ]
}
```

**New Design:**
```json
{
  "id": 101,
  "titleEn": "Form",
  "titleFr": "Formulaire",
  "fields": [
    {
      "id": "field1",
      "labelEn": "Name",
      "parentId": null
    }
  ]
}
```

**Size Reduction: ~40% smaller!**

---

## Example 4: Validation

### Current Design (Logic in Entity)

```csharp
// Current: Validation mixed into entity
public class FormModule
{
    public ModuleValidationResult ValidateModuleEnhanced(
        Dictionary<string, object>? formData)
    {
        var result = new ModuleValidationResult();

        // Complex validation logic in entity
        foreach (var field in Fields)
        {
            var fieldValue = formData?.GetValueOrDefault(field.Id);
            var fieldValidation = field.ValidateFieldEnhanced(fieldValue, formData);
            // ...
        }

        return result;
    }
}

// Usage: Validation is tightly coupled to entity
var result = module.ValidateModuleEnhanced(formData);
```

### New Design (Service-Based)

```csharp
// New: Clean service-based validation
public class FormValidationService : IFormValidationService
{
    private readonly Dictionary<string, IValidationRule> _rules = new();

    public FormValidationService()
    {
        // Register built-in rules
        RegisterRule("required", new RequiredFieldRule());
        RegisterRule("email", new EmailValidationRule());
        RegisterRule("phone", new PhoneValidationRule());
        RegisterRule("length", new LengthValidationRule());
    }

    public async Task<ValidationResult> ValidateModuleAsync(
        FormModuleRuntime module,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ValidationError>();

        foreach (var field in module.GetFieldsInOrder())
        {
            var fieldValue = formData.GetValueOrDefault(field.Schema.Id);
            var fieldResult = await ValidateFieldAsync(field, fieldValue, formData);

            if (!fieldResult.IsValid)
            {
                errors.AddRange(fieldResult.Errors);
            }
        }

        return errors.Any()
            ? new ValidationResult(false, errors)
            : ValidationResult.Success();
    }

    public void RegisterRule(string ruleId, IValidationRule rule)
    {
        _rules[ruleId] = rule;
    }
}

// Usage: Clean separation of concerns
var validationService = new FormValidationService();
var result = await validationService.ValidateModuleAsync(runtime, formData);
```

**Benefits:**
- Easy to unit test validation rules in isolation
- Easy to add custom rules without modifying entities
- Composable validation logic
- Clear dependency injection

---

## Example 5: Complex Hierarchical Form

### Creating a Multi-Level Form

```csharp
public async Task<FormModuleSchema> CreateGrantApplicationForm()
{
    var fields = new List<FormFieldSchema>();

    // Section 1: Organization Information
    var orgSection = FormFieldSchema.CreateSection(
        id: "orgInfo",
        titleEn: "Organization Information",
        titleFr: "Informations sur l'organisation",
        order: 1
    );
    fields.Add(orgSection);

    // Fields under Organization section
    fields.Add(FormFieldSchema.CreateTextField(
        id: "orgName",
        labelEn: "Organization Name",
        labelFr: "Nom de l'organisation",
        isRequired: true,
        order: 1
    ) with
    {
        ParentId = "orgInfo",
        MaxLength = 200,
        ColumnName = "OrganizationName"
    });

    fields.Add(FormFieldSchema.CreateDropDown(
        id: "orgType",
        labelEn: "Organization Type",
        options: new[]
        {
            new FieldOption("nonprofit", "Non-profit", "Sans but lucratif"),
            new FieldOption("forprofit", "For-profit", "À but lucratif"),
            new FieldOption("government", "Government", "Gouvernement")
        },
        labelFr: "Type d'organisation",
        isRequired: true,
        order: 2
    ) with
    {
        ParentId = "orgInfo",
        ColumnName = "OrganizationType"
    });

    // Conditional field - only show if organization type is "nonprofit"
    fields.Add(FormFieldSchema.CreateTextField(
        id: "charityNumber",
        labelEn: "Charity Registration Number",
        labelFr: "Numéro d'enregistrement de bienfaisance",
        isRequired: false,
        order: 3
    ) with
    {
        ParentId = "orgInfo",
        ConditionalRules = new[]
        {
            new ConditionalRule(
                FieldId: "orgType",
                Operator: "equals",
                Value: "nonprofit",
                Action: "show"
            )
        },
        Pattern = @"^\d{9}RR\d{4}$", // Canadian charity number format
        ColumnName = "CharityNumber"
    });

    // Section 2: Project Information
    var projectSection = FormFieldSchema.CreateSection(
        id: "projectInfo",
        titleEn: "Project Information",
        titleFr: "Informations sur le projet",
        order: 2
    );
    fields.Add(projectSection);

    fields.Add(FormFieldSchema.CreateTextField(
        id: "projectTitle",
        labelEn: "Project Title",
        labelFr: "Titre du projet",
        isRequired: true,
        order: 1
    ) with
    {
        ParentId = "projectInfo",
        MaxLength = 300,
        ColumnName = "ProjectTitle"
    });

    // Create the module
    var module = FormModuleSchema.Create(
        id: 201,
        titleEn: "Grant Application Form",
        titleFr: "Formulaire de demande de subvention",
        opportunityId: 10
    ) with
    {
        DescriptionEn = "Complete this form to apply for a grant",
        DescriptionFr = "Remplissez ce formulaire pour demander une subvention",
        InstructionsEn = "Please provide accurate information",
        InstructionsFr = "Veuillez fournir des informations exactes",
        Fields = fields.ToArray(),
        TableName = "GrantApplications",
        OneOfRequiredGroups = new[] { "orgName,projectTitle" }
    };

    return module;
}
```

### Using the Form

```csharp
public async Task ProcessApplicationForm()
{
    // Create the schema
    var schema = await CreateGrantApplicationForm();

    // Build runtime hierarchy
    var runtime = await _hierarchyService.BuildHierarchyAsync(schema);

    // Display hierarchy structure
    Console.WriteLine($"Module: {schema.TitleEn}");
    Console.WriteLine($"Total Fields: {runtime.Metrics.TotalFields}");
    Console.WriteLine($"Root Fields: {runtime.Metrics.RootFields}");
    Console.WriteLine($"Max Depth: {runtime.Metrics.MaxDepth}");
    Console.WriteLine();

    // Iterate through hierarchy
    foreach (var rootField in runtime.RootFields)
    {
        DisplayField(rootField, 0);
    }
}

private void DisplayField(FormFieldNode field, int indent)
{
    var padding = new string(' ', indent * 2);
    Console.WriteLine($"{padding}- {field.Schema.LabelEn} ({field.Schema.FieldType})");

    foreach (var child in field.Children.OrderBy(c => c.Schema.Order))
    {
        DisplayField(child, indent + 1);
    }
}
```

**Output:**
```
Module: Grant Application Form
Total Fields: 6
Root Fields: 2
Max Depth: 1

- Organization Information (Section)
  - Organization Name (TextBox)
  - Organization Type (DropDownList)
  - Charity Registration Number (TextBox)
- Project Information (Section)
  - Project Title (TextBox)
```

---

## Example 6: Repository Implementation

### New Clean Repository

```csharp
public class SqlServerFormModuleRepository : IFormModuleRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlServerFormModuleRepository> _logger;

    public SqlServerFormModuleRepository(
        string connectionString,
        ILogger<SqlServerFormModuleRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<bool> SaveAsync(
        FormModuleSchema schema,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE ModuleSchemas
            SET IsCurrent = 0, DateUpdated = GETUTCDATE()
            WHERE ModuleId = @ModuleId
              AND OpportunityId = @OpportunityId
              AND IsCurrent = 1;

            INSERT INTO ModuleSchemas
            (ModuleId, OpportunityId, Version, SchemaJson, DateCreated, CreatedBy, IsCurrent, IsActive)
            VALUES
            (@ModuleId, @OpportunityId, @Version, @SchemaJson, @DateCreated, @CreatedBy, 1, 1);
        ";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Serialize schema to JSON
            var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            await connection.ExecuteAsync(sql, new
            {
                ModuleId = schema.Id,
                OpportunityId = schema.OpportunityId,
                Version = schema.Version,
                SchemaJson = schemaJson,
                DateCreated = schema.DateCreated,
                CreatedBy = schema.CreatedBy
            }, transaction);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Saved module {ModuleId} version {Version}",
                schema.Id,
                schema.Version);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to save module {ModuleId}", schema.Id);
            return false;
        }
    }

    public async Task<FormModuleSchema?> GetByIdAsync(
        int moduleId,
        int? opportunityId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT TOP 1 SchemaJson
            FROM ModuleSchemas
            WHERE ModuleId = @ModuleId
              AND (@OpportunityId IS NULL OR OpportunityId = @OpportunityId)
              AND IsCurrent = 1
              AND IsActive = 1
            ORDER BY Version DESC;
        ";

        using var connection = new SqlConnection(_connectionString);
        var schemaJson = await connection.QuerySingleOrDefaultAsync<string>(
            sql,
            new { ModuleId = moduleId, OpportunityId = opportunityId });

        if (string.IsNullOrEmpty(schemaJson))
            return null;

        // Simple deserialization - no initialization needed!
        var schema = JsonSerializer.Deserialize<FormModuleSchema>(schemaJson);

        return schema;
    }
}
```

**No conversion logic needed! Schema serializes/deserializes cleanly.**

---

## Example 7: Workflow with Multiple Modules

```csharp
public async Task<FormWorkflowSchema> CreateMultiStepWorkflow()
{
    // Create individual module schemas
    var personalInfoModule = FormModuleSchema.Create(
        id: 301,
        titleEn: "Personal Information",
        titleFr: "Informations personnelles"
    );

    var employmentModule = FormModuleSchema.Create(
        id: 302,
        titleEn: "Employment History",
        titleFr: "Historique d'emploi"
    );

    var referencesModule = FormModuleSchema.Create(
        id: 303,
        titleEn: "References",
        titleFr: "Références"
    );

    // Create workflow
    var workflow = new FormWorkflowSchema
    {
        Id = 1,
        TitleEn = "Employment Application",
        TitleFr = "Demande d'emploi",
        DescriptionEn = "Complete all steps to submit your application",
        DescriptionFr = "Complétez toutes les étapes pour soumettre votre demande",
        ModuleIds = new[] { 301, 302, 303 },
        Navigation = new WorkflowNavigation(
            AllowStepJumping: false,
            ShowProgress: true,
            ShowStepNumbers: true
        ),
        Settings = new WorkflowSettings(
            RequireAllModulesComplete: true,
            AutoSaveIntervalSeconds: 300
        )
    };

    return workflow;
}
```

---

## Example 8: Custom Validation Rule

```csharp
/// <summary>
/// Custom validation rule for Canadian postal codes
/// </summary>
public class CanadianPostalCodeRule : IValidationRule
{
    public string RuleId => "canadianPostalCode";

    private static readonly Regex PostalCodeRegex =
        new(@"^[A-Z]\d[A-Z]\s?\d[A-Z]\d$", RegexOptions.Compiled);

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return Task.FromResult(ValidationResult.Success());
        }

        var postalCode = value.ToString()!.Trim().ToUpperInvariant();

        if (!PostalCodeRegex.IsMatch(postalCode))
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(
                    field.Schema.Id,
                    "INVALID_POSTAL_CODE",
                    "Please enter a valid Canadian postal code (e.g., K1A 0B1)",
                    "Veuillez entrer un code postal canadien valide (par ex., K1A 0B1)"
                )
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

// Register custom rule
var validationService = new FormValidationService();
validationService.RegisterRule("canadianPostalCode", new CanadianPostalCodeRule());

// Use in field schema
var postalCodeField = FormFieldSchema.CreateTextField(
    id: "postalCode",
    labelEn: "Postal Code",
    labelFr: "Code postal"
) with
{
    ValidationRules = new[] { "required", "canadianPostalCode" }
};
```

---

## Example 9: Type-Safe Field Configuration

```csharp
// Example: File upload field with type-safe config
var fileUploadField = new FormFieldSchema
{
    Id = "resume",
    FieldType = "FileUpload",
    LabelEn = "Resume / CV",
    LabelFr = "CV / Curriculum vitae",
    IsRequired = true,
    Order = 1,
    TypeConfig = new FileUploadConfig(
        AllowedExtensions: new[] { ".pdf", ".doc", ".docx" },
        MaxFileSizeBytes: 5_242_880, // 5MB
        MaxFiles: 1,
        RequireVirusScan: true
    )
};

// Example: Date range picker with constraints
var projectDateField = new FormFieldSchema
{
    Id = "projectDate",
    FieldType = "DateRangePicker",
    LabelEn = "Project Duration",
    LabelFr = "Durée du projet",
    Order = 2,
    TypeConfig = new DateRangeConfig(
        MinDate: DateTime.Today,
        MaxDate: DateTime.Today.AddYears(2),
        AllowFutureDates: true,
        DateFormat: "yyyy-MM-dd"
    )
};

// Example: Modal table with nested fields
var teamMembersField = new FormFieldSchema
{
    Id = "teamMembers",
    FieldType = "ModalTable",
    LabelEn = "Team Members",
    LabelFr = "Membres de l'équipe",
    Order = 3,
    TypeConfig = new ModalTableConfig(
        ModalFields: new[]
        {
            FormFieldSchema.CreateTextField("memberName", "Name", "Nom", true, 1),
            FormFieldSchema.CreateTextField("memberRole", "Role", "Rôle", true, 2),
            FormFieldSchema.CreateTextField("memberEmail", "Email", "Courriel", true, 3)
        },
        MaxRecords: 10,
        AllowDuplicates: false
    )
};
```

---

## Example 10: Complete Service Usage

```csharp
public class FormProcessingService
{
    private readonly IFormModuleRepository _repository;
    private readonly IFormHierarchyService _hierarchyService;
    private readonly IFormValidationService _validationService;
    private readonly ILogger<FormProcessingService> _logger;

    public FormProcessingService(
        IFormModuleRepository repository,
        IFormHierarchyService hierarchyService,
        IFormValidationService validationService,
        ILogger<FormProcessingService> logger)
    {
        _repository = repository;
        _hierarchyService = hierarchyService;
        _validationService = validationService;
        _logger = logger;
    }

    public async Task<ProcessResult> ProcessApplicationAsync(
        int moduleId,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Load schema from repository
            var schema = await _repository.GetByIdAsync(moduleId, cancellationToken: cancellationToken);
            if (schema == null)
            {
                return ProcessResult.Failure($"Module {moduleId} not found");
            }

            // 2. Build runtime hierarchy
            var runtime = await _hierarchyService.BuildHierarchyAsync(schema, cancellationToken);

            // 3. Validate form data
            var validationResult = await _validationService.ValidateModuleAsync(
                runtime,
                formData,
                cancellationToken);

            if (!validationResult.IsValid)
            {
                return ProcessResult.Failure("Validation failed", validationResult.Errors);
            }

            // 4. Process the data (save to database, send emails, etc.)
            await SaveFormDataAsync(schema, formData, cancellationToken);

            _logger.LogInformation(
                "Successfully processed application for module {ModuleId}",
                moduleId);

            return ProcessResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing application for module {ModuleId}", moduleId);
            return ProcessResult.Failure($"Processing error: {ex.Message}");
        }
    }

    private async Task SaveFormDataAsync(
        FormModuleSchema schema,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken)
    {
        // Implementation for saving data
        // This would use IFormDataRepository
        await Task.CompletedTask;
    }
}

public record ProcessResult(
    bool IsSuccessful,
    string? ErrorMessage = null,
    List<ValidationError>? ValidationErrors = null
)
{
    public static ProcessResult Success() => new(true);
    public static ProcessResult Failure(string message, List<ValidationError>? errors = null)
        => new(false, message, errors);
}
```

---

## Summary

The new design provides:

✅ **Simpler creation** - Factory methods, fluent syntax, no nesting
✅ **Clean JSON** - Direct serialization, 40% smaller payload
✅ **No initialization** - No `EnsureInitialized()` or `RebuildFieldHierarchy()` required
✅ **Service-based logic** - Clear separation of concerns
✅ **Type safety** - Strongly-typed configs instead of dictionaries
✅ **Easier testing** - Services are easily mockable and testable
✅ **Better performance** - Less memory, faster serialization
✅ **Maintainability** - Clear, predictable code structure

All while retaining full enterprise and government capabilities!
