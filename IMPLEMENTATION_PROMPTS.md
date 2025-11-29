# Implementation Prompts for Alternative Design

This document contains precise, well-formed prompts to guide the implementation of the alternative DynamicForms.Core design. Each prompt is designed to be copy-pasted directly to Claude with minimal omissions or errors.

---

## How to Use These Prompts

1. **Execute prompts in order** - They build upon each other
2. **Copy entire prompt** - Include context, requirements, and acceptance criteria
3. **Verify output** - Check acceptance criteria before moving to next prompt
4. **Reference files** - Point Claude to ALTERNATIVE_DESIGN_PROPOSAL.md for details

---

## Prerequisites

Before starting, ensure Claude has access to:
- `ALTERNATIVE_DESIGN_PROPOSAL.md`
- `ALTERNATIVE_DESIGN_EXAMPLES.md`
- `DESIGN_COMPARISON.md`
- Current codebase in `Src/DynamicForms.Core/`

---

## Phase 1: Project Structure & Core Schemas

### Prompt 1.1: Create Project Structure

```
I need you to create a new .NET 9.0 class library project for the alternative DynamicForms.Core design.

PROJECT REQUIREMENTS:
- Project name: DynamicForms.Core.V2
- Target framework: net9.0
- Location: Src/DynamicForms.Core.V2/
- NuGet packages needed:
  - System.Text.Json (version 9.0.0)
  - Microsoft.Extensions.Logging.Abstractions (version 9.0.0)
  - Microsoft.Extensions.DependencyInjection.Abstractions (version 9.0.0)

FOLDER STRUCTURE:
Src/DynamicForms.Core.V2/
├── Schemas/              (Schema classes - serializable)
├── Runtime/              (Runtime state classes - not serialized)
├── Services/             (Service interfaces and implementations)
├── Validation/           (Validation rules and interfaces)
├── Enums/               (Enumerations)
└── Extensions/          (Extension methods)

ACCEPTANCE CRITERIA:
1. ✓ Project builds without errors
2. ✓ All folders created
3. ✓ NuGet packages installed
4. ✓ Project added to solution

Create the project structure now. Show me the .csproj file content and folder structure.
```

### Prompt 1.2: Create Core Enums

```
Create the core enumeration types for the alternative design.

REFERENCE: See ALTERNATIVE_DESIGN_PROPOSAL.md section "New Schema Design"

CREATE FILE: Src/DynamicForms.Core.V2/Enums/RelationshipType.cs

REQUIREMENTS:
1. Create RelationshipType enum with these values:
   - Container (for Section, Group, Panel)
   - Conditional (for conditional show/hide)
   - Cascade (for cascading dropdowns)
   - Validation (for validation dependencies)
2. Add XML documentation for each value
3. Use proper namespace: DynamicForms.Core.V2.Enums

EXAMPLE STRUCTURE:
namespace DynamicForms.Core.V2.Enums;

/// <summary>
/// Defines the type of relationship between parent and child fields
/// </summary>
public enum RelationshipType
{
    /// <summary>Structural container (Section, Group, Panel)</summary>
    Container = 0,

    // ... etc
}

ACCEPTANCE CRITERIA:
1. ✓ File compiles without warnings
2. ✓ All 4 relationship types defined
3. ✓ XML documentation on enum and all values
4. ✓ Proper namespace

Create this file now.
```

### Prompt 1.3: Create Supporting Record Types

```
Create the supporting record types used by field schemas.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "Supporting Types - Simple Records"

CREATE FILE: Src/DynamicForms.Core.V2/Schemas/FieldSupport.cs

REQUIREMENTS:
1. Create ConditionalRule record with properties:
   - string FieldId
   - string Operator (equals, notEquals, contains, greaterThan, lessThan, etc.)
   - string? Value
   - string Action (show, hide, enable, disable, require)

2. Create FieldOption record with properties:
   - string Value
   - string LabelEn
   - string? LabelFr
   - bool IsDefault
   - int Order

3. All should be public records with primary constructors
4. Add XML documentation for each type and property
5. Use namespace: DynamicForms.Core.V2.Schemas

TEMPLATE:
namespace DynamicForms.Core.V2.Schemas;

/// <summary>
/// Defines a conditional rule for field visibility or behavior
/// </summary>
/// <param name="FieldId">ID of the field that triggers this rule</param>
/// <param name="Operator">Comparison operator (equals, notEquals, contains, etc.)</param>
/// <param name="Value">Value to compare against</param>
/// <param name="Action">Action to take when condition is met (show, hide, enable, disable, require)</param>
public record ConditionalRule(
    string FieldId,
    string Operator,
    string? Value,
    string Action
);

ACCEPTANCE CRITERIA:
1. ✓ Both records compile without errors
2. ✓ All properties properly documented
3. ✓ Records use primary constructors
4. ✓ Nullable annotations correct

Create this file now.
```

### Prompt 1.4: Create Field Type Configuration Classes

```
Create the polymorphic field type configuration classes.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "Type-Safe Field Configuration"

CREATE FILE: Src/DynamicForms.Core.V2/Schemas/FieldTypeConfigs.cs

REQUIREMENTS:
1. Create abstract base record: FieldTypeConfig
   - Use [JsonPolymorphic] attribute with TypeDiscriminatorPropertyName = "$type"

2. Create FileUploadConfig record (derived from FieldTypeConfig):
   - string[] AllowedExtensions
   - long MaxFileSizeBytes (default: 10_485_760)
   - int MaxFiles (default: 1)
   - bool RequireVirusScan (default: true)
   - Use [JsonDerivedType(typeof(FileUploadConfig), "fileUpload")]

3. Create DateRangeConfig record (derived from FieldTypeConfig):
   - DateTime? MinDate (default: null)
   - DateTime? MaxDate (default: null)
   - bool AllowFutureDates (default: true)
   - string DateFormat (default: "yyyy-MM-dd")
   - Use [JsonDerivedType(typeof(DateRangeConfig), "dateRange")]

4. Create ModalTableConfig record (derived from FieldTypeConfig):
   - FormFieldSchema[] ModalFields (we'll create FormFieldSchema next)
   - int? MaxRecords (default: null)
   - bool AllowDuplicates (default: false)
   - Use [JsonDerivedType(typeof(ModalTableConfig), "modalTable")]

IMPORTANT:
- Add all [JsonDerivedType] attributes to the base FieldTypeConfig class
- Use System.Text.Json.Serialization namespace
- Add XML documentation for all types and parameters
- Use namespace: DynamicForms.Core.V2.Schemas

ACCEPTANCE CRITERIA:
1. ✓ Base class has all JsonDerivedType attributes
2. ✓ All three derived configs compile
3. ✓ Default values set correctly
4. ✓ Full XML documentation

Create this file now. Note: FormFieldSchema will be created in next step, so ModalTableConfig will have a forward reference.
```

### Prompt 1.5: Create FormFieldSchema Class

```
Create the core FormFieldSchema class - the heart of the alternative design.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "1. Simplified FormField"

CREATE FILE: Src/DynamicForms.Core.V2/Schemas/FormFieldSchema.cs

REQUIREMENTS:
This is a large class. Implement it with these property groups:

1. CORE IDENTITY (all required or with defaults):
   - required string Id { get; init; }
   - required string FieldType { get; init; }
   - int Order { get; init; } = 1
   - float Version { get; init; } = 1.0f

2. HIERARCHY:
   - string? ParentId { get; init; }
   - RelationshipType Relationship { get; init; } = RelationshipType.Container

3. MULTILINGUAL TEXT (all nullable, use init):
   - string? LabelEn, LabelFr
   - string? DescriptionEn, DescriptionFr
   - string? HelpEn, HelpFr
   - string? PlaceholderEn, PlaceholderFr

4. VALIDATION:
   - bool IsRequired { get; init; }
   - int? MinLength { get; init; }
   - int? MaxLength { get; init; }
   - string? Pattern { get; init; }
   - string[]? ValidationRules { get; init; }

5. CONDITIONAL LOGIC:
   - ConditionalRule[]? ConditionalRules { get; init; }

6. DATA SOURCE:
   - int? CodeSetId { get; init; }
   - FieldOption[]? Options { get; init; }

7. LAYOUT & STYLING:
   - int? WidthClass { get; init; }
   - string? CssClasses { get; init; }
   - bool IsVisible { get; init; } = true
   - bool IsReadOnly { get; init; }

8. DATABASE MAPPING:
   - string? ColumnName { get; init; }
   - string? ColumnType { get; init; }

9. TYPE-SPECIFIC CONFIG:
   - FieldTypeConfig? TypeConfig { get; init; }

10. EXTENSIBILITY:
   - JsonElement? ExtendedProperties { get; init; }

FACTORY METHODS (all static):
- CreateTextField(string id, string labelEn, string? labelFr = null, bool isRequired = false, int order = 1)
- CreateSection(string id, string titleEn, string? titleFr = null, int order = 1)
- CreateDropDown(string id, string labelEn, FieldOption[] options, string? labelFr = null, bool isRequired = false, int order = 1)

IMPORTANT:
- Use init-only setters for immutability
- Add comprehensive XML documentation
- Group properties with region comments as shown in design doc
- Use namespace: DynamicForms.Core.V2.Schemas

ACCEPTANCE CRITERIA:
1. ✓ All 10 property groups implemented
2. ✓ All three factory methods work
3. ✓ Class compiles without errors
4. ✓ XML documentation complete
5. ✓ Can create instance: var field = FormFieldSchema.CreateTextField("test", "Test");

Create this file now. Follow the exact structure from ALTERNATIVE_DESIGN_PROPOSAL.md.
```

### Prompt 1.6: Create FormModuleSchema Class

```
Create the FormModuleSchema class.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "2. Simplified FormModule"

CREATE FILE: Src/DynamicForms.Core.V2/Schemas/FormModuleSchema.cs

REQUIREMENTS:
Implement FormModuleSchema with these property groups:

1. CORE IDENTITY:
   - required int Id { get; init; }
   - int? OpportunityId { get; init; }
   - float Version { get; init; } = 1.0f
   - DateTime DateCreated { get; init; } = DateTime.UtcNow
   - DateTime? DateUpdated { get; init; }
   - string? CreatedBy { get; init; }

2. MULTILINGUAL METADATA:
   - required string TitleEn { get; init; }
   - string? TitleFr { get; init; }
   - string? DescriptionEn, DescriptionFr { get; init; }
   - string? InstructionsEn, InstructionsFr { get; init; }

3. FIELDS:
   - FormFieldSchema[] Fields { get; init; } = Array.Empty<FormFieldSchema>()

4. VALIDATION RULES:
   - string[]? OneOfRequiredGroups { get; init; }
   - string[]? NumericFieldIds { get; init; }
   - string[]? DateFieldIds { get; init; }
   - ModuleValidationRule[]? CustomValidationRules { get; init; }

5. DATABASE CONFIGURATION:
   - string? TableName { get; init; }
   - string? SchemaName { get; init; } = "dbo"

6. EXTENSIBILITY:
   - JsonElement? ExtendedProperties { get; init; }

SUPPORTING RECORD:
Create ModuleValidationRule record with:
- string RuleId
- string RuleType
- JsonElement Configuration

FACTORY METHOD:
- static FormModuleSchema Create(int id, string titleEn, string? titleFr = null, int? opportunityId = null)

IMPORTANT:
- Add region comments for property groups
- Full XML documentation
- Use namespace: DynamicForms.Core.V2.Schemas

ACCEPTANCE CRITERIA:
1. ✓ All property groups implemented
2. ✓ Factory method works
3. ✓ ModuleValidationRule record created
4. ✓ Compiles without errors
5. ✓ Test: var module = FormModuleSchema.Create(1, "Test");

Create this file now.
```

### Prompt 1.7: Create FormWorkflowSchema Class

```
Create the FormWorkflowSchema class for multi-module workflows.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "4. Simplified FormWorkflow"

CREATE FILE: Src/DynamicForms.Core.V2/Schemas/FormWorkflowSchema.cs

REQUIREMENTS:
1. FormWorkflowSchema class with properties:
   - required int Id { get; init; }
   - int? OpportunityId { get; init; }
   - float Version { get; init; } = 1.0f
   - DateTime DateCreated { get; init; } = DateTime.UtcNow
   - required string TitleEn { get; init; }
   - string? TitleFr, DescriptionEn, DescriptionFr { get; init; }
   - int[] ModuleIds { get; init; } = Array.Empty<int>()
   - WorkflowNavigation Navigation { get; init; } = new()
   - WorkflowSettings Settings { get; init; } = new()
   - JsonElement? ExtendedProperties { get; init; }

2. WorkflowNavigation record with:
   - bool AllowStepJumping = false
   - bool ShowProgress = true
   - bool ShowStepNumbers = true

3. WorkflowSettings record with:
   - bool RequireAllModulesComplete = true
   - bool AllowModuleSkipping = false
   - int AutoSaveIntervalSeconds = 300

IMPORTANT:
- Use records with primary constructors and default values
- Add XML documentation
- Use namespace: DynamicForms.Core.V2.Schemas

ACCEPTANCE CRITERIA:
1. ✓ Main class and two records compile
2. ✓ Default values set correctly
3. ✓ Can create: var workflow = new FormWorkflowSchema { Id = 1, TitleEn = "Test" };

Create this file now.
```

---

## Phase 2: Runtime State Classes

### Prompt 2.1: Create Runtime Hierarchy Classes

```
Create the runtime hierarchy state classes - these are NOT serialized, only used for navigation.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "3. Runtime Hierarchy State (Not Serialized)"

CREATE FILE: Src/DynamicForms.Core.V2/Runtime/FormFieldNode.cs

REQUIREMENTS:
1. FormFieldNode class with:
   - required FormFieldSchema Schema { get; init; }
   - FormFieldNode? Parent { get; set; }
   - List<FormFieldNode> Children { get; } = new()
   - int Level => Parent?.Level + 1 ?? 0  (computed property)
   - string Path => Parent != null ? $"{Parent.Path}.{Schema.Id}" : Schema.Id  (computed)

2. Navigation methods:
   - IEnumerable<FormFieldNode> GetAllDescendants()  (recursive, yield return)
   - IEnumerable<FormFieldNode> GetAllAncestors()  (walk up parent chain)

IMPORTANT:
- This class is NOT serialized (no JSON attributes needed)
- Parent and Children are mutable (set/get) for building hierarchy
- Schema is immutable (init)
- Use namespace: DynamicForms.Core.V2.Runtime

ACCEPTANCE CRITERIA:
1. ✓ Class compiles
2. ✓ Computed properties work
3. ✓ Navigation methods implemented with yield return
4. ✓ XML documentation complete

Create this file now.
```

### Prompt 2.2: Create HierarchyMetrics Record

```
Create the HierarchyMetrics record for runtime statistics.

CREATE FILE: Src/DynamicForms.Core.V2/Runtime/HierarchyMetrics.cs

REQUIREMENTS:
Create HierarchyMetrics record with these properties:
- int TotalFields
- int RootFields
- int MaxDepth
- double AverageDepth
- int ConditionalFields
- double ComplexityScore

Use record with primary constructor.
Add XML documentation.
Namespace: DynamicForms.Core.V2.Runtime

ACCEPTANCE CRITERIA:
1. ✓ Record compiles
2. ✓ Can create: var metrics = new HierarchyMetrics(10, 2, 3, 1.5, 2, 25.5);

Create this file now.
```

### Prompt 2.3: Create FormModuleRuntime Class

```
Create the runtime representation of a module with built hierarchy.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "Runtime Hierarchy State"

CREATE FILE: Src/DynamicForms.Core.V2/Runtime/FormModuleRuntime.cs

REQUIREMENTS:
1. FormModuleRuntime class with:
   - required FormModuleSchema Schema { get; init; }
   - Dictionary<string, FormFieldNode> FieldNodes { get; init; } = new()
   - List<FormFieldNode> RootFields { get; init; } = new()
   - HierarchyMetrics Metrics { get; init; } = new()

2. Helper methods:
   - FormFieldNode? GetField(string fieldId)  (lookup in FieldNodes)
   - IEnumerable<FormFieldNode> GetFieldsInOrder()  (depth-first traversal)

IMPORTANT:
- This is the object returned by IFormHierarchyService
- Contains both the schema and the built navigation tree
- Not serialized
- Namespace: DynamicForms.Core.V2.Runtime

ACCEPTANCE CRITERIA:
1. ✓ Class compiles
2. ✓ Both helper methods implemented
3. ✓ GetFieldsInOrder() returns fields in depth-first order

Create this file now.
```

---

## Phase 3: Service Interfaces

### Prompt 3.1: Create Validation Result Types

```
Create the validation result types used by services.

CREATE FILE: Src/DynamicForms.Core.V2/Validation/ValidationResults.cs

REQUIREMENTS:
1. ValidationError record:
   - string FieldId
   - string ErrorCode
   - string Message
   - string? MessageFr = null

2. ValidationResult record:
   - bool IsValid
   - List<ValidationError> Errors
   - Static factory methods:
     * static ValidationResult Success()
     * static ValidationResult Failure(params ValidationError[] errors)

IMPORTANT:
- Use records with primary constructors
- Add XML documentation
- Namespace: DynamicForms.Core.V2.Validation

TEMPLATE:
/// <summary>
/// Represents a validation error for a specific field
/// </summary>
public record ValidationError(
    string FieldId,
    string ErrorCode,
    string Message,
    string? MessageFr = null
);

/// <summary>
/// Result of a validation operation
/// </summary>
public record ValidationResult(
    bool IsValid,
    List<ValidationError> Errors
)
{
    public ValidationResult() : this(true, new()) { }

    public static ValidationResult Success() => new();

    public static ValidationResult Failure(params ValidationError[] errors)
        => new(false, new List<ValidationError>(errors));
}

ACCEPTANCE CRITERIA:
1. ✓ Both records compile
2. ✓ Factory methods work
3. ✓ Test: var result = ValidationResult.Success();

Create this file now.
```

### Prompt 3.2: Create IFormHierarchyService Interface

```
Create the hierarchy service interface.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "Service-Based Architecture"

CREATE FILE: Src/DynamicForms.Core.V2/Services/IFormHierarchyService.cs

REQUIREMENTS:
Create IFormHierarchyService interface with these methods:

1. Task<FormModuleRuntime> BuildHierarchyAsync(
       FormModuleSchema schema,
       CancellationToken cancellationToken = default);

2. HierarchyValidationResult ValidateHierarchy(FormModuleSchema schema);

3. FormModuleSchema FixHierarchyIssues(FormModuleSchema schema);

4. HierarchyMetrics CalculateMetrics(FormModuleSchema schema);

Also create HierarchyValidationResult record:
- List<string> Errors
- List<string> Warnings
- bool IsValid => !Errors.Any()

IMPORTANT:
- Interface in Services namespace
- HierarchyValidationResult in Runtime namespace
- Add comprehensive XML documentation
- All methods well-documented with params and returns

ACCEPTANCE CRITERIA:
1. ✓ Interface compiles
2. ✓ HierarchyValidationResult record created
3. ✓ All method signatures correct

Create this file now.
```

### Prompt 3.3: Create IValidationRule Interface

```
Create the validation rule interface for composable validation.

CREATE FILE: Src/DynamicForms.Core.V2/Validation/IValidationRule.cs

REQUIREMENTS:
Create IValidationRule interface:
- string RuleId { get; }
- Task<ValidationResult> ValidateAsync(
      FormFieldNode field,
      object? value,
      Dictionary<string, object?> formData,
      CancellationToken cancellationToken = default);

Add XML documentation explaining:
- RuleId is unique identifier for the rule
- ValidateAsync performs the validation and returns result
- formData contains all field values for cross-field validation

Namespace: DynamicForms.Core.V2.Validation

ACCEPTANCE CRITERIA:
1. ✓ Interface compiles
2. ✓ Method signature correct
3. ✓ Full XML documentation

Create this file now.
```

### Prompt 3.4: Create IFormValidationService Interface

```
Create the validation service interface.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "IFormValidationService - Clean Validation"

CREATE FILE: Src/DynamicForms.Core.V2/Services/IFormValidationService.cs

REQUIREMENTS:
Create IFormValidationService interface with:

1. Task<ValidationResult> ValidateModuleAsync(
       FormModuleRuntime module,
       Dictionary<string, object?> formData,
       CancellationToken cancellationToken = default);

2. Task<ValidationResult> ValidateFieldAsync(
       FormFieldNode field,
       object? value,
       Dictionary<string, object?> formData,
       CancellationToken cancellationToken = default);

3. void RegisterRule(string ruleId, IValidationRule rule);

Add XML documentation for each method.
Namespace: DynamicForms.Core.V2.Services

ACCEPTANCE CRITERIA:
1. ✓ Interface compiles
2. ✓ All three methods defined
3. ✓ XML documentation complete

Create this file now.
```

### Prompt 3.5: Create Repository Interfaces

```
Create the repository interfaces for persistence.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "IFormModuleRepository - Clean Persistence"

CREATE FILE: Src/DynamicForms.Core.V2/Services/IFormModuleRepository.cs

REQUIREMENTS:
Create IFormModuleRepository interface with these methods:

1. Task<bool> SaveAsync(FormModuleSchema schema, CancellationToken cancellationToken = default);

2. Task<FormModuleSchema?> GetByIdAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);

3. Task<FormModuleSchema[]> GetByIdsAsync(int[] moduleIds, int? opportunityId = null, CancellationToken cancellationToken = default);

4. Task<bool> DeleteAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);

5. Task<bool> ExistsAsync(int moduleId, int? opportunityId = null, CancellationToken cancellationToken = default);

Also create supporting types in same file:
- ModuleVersionInfo record (ModuleId, OpportunityId, Version, DateCreated, CreatedBy, IsCurrent, TotalFields, etc.)
- ModuleSearchCriteria record (search parameters)
- ModuleSearchResult record (search results)

IMPORTANT:
- Simple CRUD operations only
- No business logic in repository
- Clean, focused interface
- Namespace: DynamicForms.Core.V2.Services

ACCEPTANCE CRITERIA:
1. ✓ Interface with all 5 methods compiles
2. ✓ Supporting records created
3. ✓ XML documentation complete

Create this file now.
```

---

## Phase 4: Service Implementations

### Prompt 4.1: Implement FormHierarchyService

```
Implement the hierarchy service - the core of the runtime hierarchy building.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "IFormHierarchyService" implementation example

CREATE FILE: Src/DynamicForms.Core.V2/Services/FormHierarchyService.cs

REQUIREMENTS:
Implement FormHierarchyService : IFormHierarchyService

CRITICAL IMPLEMENTATION DETAILS:

1. BuildHierarchyAsync method:
   Phase 1: Create all FormFieldNode instances from schema.Fields
   Phase 2: Build parent-child relationships
     - For each node, if ParentId is set, find parent and:
       * Add node to parent.Children
       * Set node.Parent = parent
     - If ParentId references non-existent field, log warning and make it a root field
   Phase 3: Sort root fields by Order
   Phase 4: Calculate metrics
   Return new FormModuleRuntime with all properties set

2. ValidateHierarchy method:
   - Check all ParentId references exist
   - Check for self-referencing (field.ParentId == field.Id)
   - Check for duplicate field IDs
   - Return errors and warnings

3. FixHierarchyIssues method:
   - Clear invalid ParentId references
   - Return new schema with fixed fields (use 'with' expression)

4. CalculateMetrics method:
   - Build temp hierarchy
   - Calculate total fields, root fields, max depth, average depth
   - Count conditional fields
   - Calculate complexity score

IMPORTANT:
- Inject ILogger<FormHierarchyService> in constructor
- Use async/await properly
- Log warnings for orphaned fields
- Namespace: DynamicForms.Core.V2.Services

ACCEPTANCE CRITERIA:
1. ✓ Class implements IFormHierarchyService
2. ✓ BuildHierarchyAsync builds correct parent-child relationships
3. ✓ Orphaned fields become root fields (with logged warning)
4. ✓ ValidateHierarchy catches common issues
5. ✓ Compiles without errors

Create this implementation now. Follow the algorithm described in ALTERNATIVE_DESIGN_PROPOSAL.md.
```

### Prompt 4.2: Implement Built-in Validation Rules

```
Implement the built-in validation rules.

CREATE FILE: Src/DynamicForms.Core.V2/Validation/BuiltInRules.cs

REQUIREMENTS:
Implement these validation rule classes, all implementing IValidationRule:

1. RequiredFieldRule:
   - RuleId = "required"
   - Check if value is null or whitespace
   - Return error if required and empty

2. LengthValidationRule:
   - RuleId = "length"
   - Check MinLength and MaxLength from field.Schema
   - Return error if out of range

3. PatternValidationRule:
   - RuleId = "pattern"
   - Use field.Schema.Pattern as Regex
   - Return error if doesn't match

4. EmailValidationRule:
   - RuleId = "email"
   - Use regex for email validation: @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
   - Return error if invalid email format

IMPORTANT:
- All in same file for simplicity
- Each class has proper RuleId
- ValidateAsync returns ValidationResult
- Include both English and French error messages
- Namespace: DynamicForms.Core.V2.Validation

TEMPLATE FOR ONE RULE:
public class RequiredFieldRule : IValidationRule
{
    public string RuleId => "required";

    public Task<ValidationResult> ValidateAsync(
        FormFieldNode field,
        object? value,
        Dictionary<string, object?> formData,
        CancellationToken cancellationToken = default)
    {
        if (!field.Schema.IsRequired)
            return Task.FromResult(ValidationResult.Success());

        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return Task.FromResult(ValidationResult.Failure(
                new ValidationError(
                    field.Schema.Id,
                    "REQUIRED",
                    $"{field.Schema.LabelEn ?? field.Schema.Id} is required",
                    $"{field.Schema.LabelFr ?? field.Schema.LabelEn} est requis"
                )
            ));
        }

        return Task.FromResult(ValidationResult.Success());
    }
}

ACCEPTANCE CRITERIA:
1. ✓ All 4 rules implemented
2. ✓ Each returns proper ValidationResult
3. ✓ Bilingual error messages
4. ✓ Compiles without errors

Create this file with all 4 rules now.
```

### Prompt 4.3: Implement FormValidationService

```
Implement the validation service with rule registration.

REFERENCE: ALTERNATIVE_DESIGN_PROPOSAL.md section "IFormValidationService" implementation

CREATE FILE: Src/DynamicForms.Core.V2/Services/FormValidationService.cs

REQUIREMENTS:
Implement FormValidationService : IFormValidationService

KEY IMPLEMENTATION DETAILS:

1. Constructor:
   - Inject ILogger<FormValidationService>
   - Initialize private Dictionary<string, IValidationRule> _rules
   - Register all built-in rules:
     * RegisterRule("required", new RequiredFieldRule())
     * RegisterRule("length", new LengthValidationRule())
     * RegisterRule("pattern", new PatternValidationRule())
     * RegisterRule("email", new EmailValidationRule())

2. ValidateModuleAsync:
   - Iterate through all fields in runtime.GetFieldsInOrder()
   - For each field, get value from formData
   - Call ValidateFieldAsync
   - Collect all errors
   - Return aggregated ValidationResult

3. ValidateFieldAsync:
   - Check IsRequired → run required rule
   - Check ValidationRules array → run each rule by ID
   - Check MinLength/MaxLength → run length rule
   - Check Pattern → run pattern rule
   - Aggregate all errors
   - Return ValidationResult

4. RegisterRule:
   - Add or update rule in _rules dictionary
   - Log info when rule registered

IMPORTANT:
- Use async/await properly
- Log validation operations
- Handle missing rules gracefully (log warning)
- Namespace: DynamicForms.Core.V2.Services

ACCEPTANCE CRITERIA:
1. ✓ Implements IFormValidationService
2. ✓ All built-in rules registered in constructor
3. ✓ ValidateModuleAsync validates all fields
4. ✓ Can register custom rules
5. ✓ Compiles without errors

Create this implementation now.
```

---

## Phase 5: Dependency Injection Setup

### Prompt 5.1: Create Service Registration Extension

```
Create extension methods for easy dependency injection setup.

CREATE FILE: Src/DynamicForms.Core.V2/Extensions/ServiceCollectionExtensions.cs

REQUIREMENTS:
Create static class ServiceCollectionExtensions with extension method:

public static IServiceCollection AddDynamicFormsV2(
    this IServiceCollection services)
{
    // Register services
    services.AddSingleton<IFormHierarchyService, FormHierarchyService>();
    services.AddSingleton<IFormValidationService, FormValidationService>();

    // Register validation rules as singletons
    services.AddSingleton<IValidationRule, RequiredFieldRule>();
    services.AddSingleton<IValidationRule, LengthValidationRule>();
    services.AddSingleton<IValidationRule, PatternValidationRule>();
    services.AddSingleton<IValidationRule, EmailValidationRule>();

    return services;
}

IMPORTANT:
- Use Microsoft.Extensions.DependencyInjection namespace
- Add XML documentation
- Namespace: DynamicForms.Core.V2.Extensions

ACCEPTANCE CRITERIA:
1. ✓ Extension method compiles
2. ✓ All services registered
3. ✓ Can call: services.AddDynamicFormsV2();

Create this file now.
```

---

## Phase 6: Unit Tests

### Prompt 6.1: Create Test Project

```
Create a unit test project for the alternative design.

PROJECT REQUIREMENTS:
- Project name: DynamicForms.Core.V2.Tests
- Target framework: net9.0
- Location: Tests/DynamicForms.Core.V2.Tests/
- NuGet packages:
  - Microsoft.NET.Test.Sdk (latest)
  - xunit (latest)
  - xunit.runner.visualstudio (latest)
  - FluentAssertions (latest)
  - Moq (latest)
  - Microsoft.Extensions.Logging.Abstractions (9.0.0)

- Project reference to Src/DynamicForms.Core.V2/

FOLDER STRUCTURE:
Tests/DynamicForms.Core.V2.Tests/
├── Schemas/          (Schema tests)
├── Services/         (Service tests)
├── Validation/       (Validation rule tests)
└── Helpers/          (Test helpers)

ACCEPTANCE CRITERIA:
1. ✓ Test project created
2. ✓ All packages installed
3. ✓ Project reference added
4. ✓ Test project builds

Create the test project structure now.
```

### Prompt 6.2: Create Schema Tests

```
Create unit tests for FormFieldSchema and FormModuleSchema.

CREATE FILE: Tests/DynamicForms.Core.V2.Tests/Schemas/FormFieldSchemaTests.cs

REQUIREMENTS:
Create test class FormFieldSchemaTests with these tests:

1. CreateTextField_WithRequiredParameters_CreatesValidField
   - Arrange: id, labelEn
   - Act: FormFieldSchema.CreateTextField(...)
   - Assert: Properties set correctly, FieldType is "TextBox"

2. CreateSection_WithMultilingualTitles_CreatesValidSection
   - Test both EN and FR titles set correctly
   - Assert FieldType is "Section"

3. CreateDropDown_WithOptions_CreatesValidDropdown
   - Create with FieldOption array
   - Assert Options property contains all options

4. FieldWithConditionalRules_SerializesAndDeserializes_Correctly
   - Create field with ConditionalRules
   - Serialize to JSON
   - Deserialize back
   - Assert all properties match

5. ImmutableSchema_CannotModifyAfterCreation_ThrowsError
   - Try to modify init-only property
   - Should not compile (comment this as a note)

Use FluentAssertions for assertions:
- field.Id.Should().Be("expectedId");
- field.FieldType.Should().Be("TextBox");

IMPORTANT:
- Use xUnit [Fact] attribute
- Namespace: DynamicForms.Core.V2.Tests.Schemas
- Test JSON serialization round-trip

ACCEPTANCE CRITERIA:
1. ✓ All 4 executable tests pass
2. ✓ JSON serialization tests work
3. ✓ Tests compile and run

Create this test file now.
```

### Prompt 6.3: Create Hierarchy Service Tests

```
Create unit tests for FormHierarchyService.

CREATE FILE: Tests/DynamicForms.Core.V2.Tests/Services/FormHierarchyServiceTests.cs

REQUIREMENTS:
Create FormHierarchyServiceTests with these tests:

1. BuildHierarchyAsync_WithSimpleModule_BuildsCorrectHierarchy
   - Create schema with 3 fields (1 section, 2 children)
   - Build hierarchy
   - Assert: RootFields.Count == 1
   - Assert: RootFields[0].Children.Count == 2
   - Assert: Children have correct Parent references

2. BuildHierarchyAsync_WithOrphanedField_MakesItRootField
   - Create field with ParentId referencing non-existent field
   - Build hierarchy
   - Assert: Orphaned field is in RootFields
   - Assert: Warning logged (use Moq to verify)

3. ValidateHierarchy_WithCircularReference_ReturnsError
   - Create schema where field references itself
   - Validate
   - Assert: Errors contains circular reference message

4. CalculateMetrics_WithComplexModule_ReturnsCorrectMetrics
   - Create module with 10 fields, 3 levels deep
   - Calculate metrics
   - Assert: TotalFields == 10
   - Assert: MaxDepth == 2 (0-indexed)

SETUP:
In constructor or setup method:
- var logger = new Mock<ILogger<FormHierarchyService>>();
- _service = new FormHierarchyService(logger.Object);

Use FluentAssertions:
- runtime.RootFields.Should().HaveCount(1);
- result.Errors.Should().ContainSingle();

ACCEPTANCE CRITERIA:
1. ✓ All tests pass
2. ✓ Tests verify hierarchy building correctly
3. ✓ Tests verify error cases

Create this test file now.
```

### Prompt 6.4: Create Validation Service Tests

```
Create unit tests for FormValidationService.

CREATE FILE: Tests/DynamicForms.Core.V2.Tests/Services/FormValidationServiceTests.cs

REQUIREMENTS:
Create FormValidationServiceTests with these tests:

1. ValidateFieldAsync_RequiredFieldWithValue_ReturnsSuccess
   - Create required field
   - Validate with non-empty value
   - Assert: result.IsValid == true

2. ValidateFieldAsync_RequiredFieldWithoutValue_ReturnsError
   - Create required field
   - Validate with null value
   - Assert: result.IsValid == false
   - Assert: result.Errors contains REQUIRED error code

3. ValidateFieldAsync_EmailField_WithInvalidEmail_ReturnsError
   - Create field with email validation rule
   - Validate with "notanemail"
   - Assert: validation fails

4. ValidateModuleAsync_WithMultipleErrors_ReturnsAllErrors
   - Create module with 3 required fields
   - Validate with empty form data
   - Assert: result.Errors.Count == 3

5. RegisterCustomRule_ThenValidate_UsesCustomRule
   - Create custom validation rule
   - Register it
   - Validate field using custom rule
   - Assert: custom rule was executed

SETUP:
- var logger = new Mock<ILogger<FormValidationService>>();
- _service = new FormValidationService(logger.Object);

ACCEPTANCE CRITERIA:
1. ✓ All tests pass
2. ✓ Tests cover required, email, and custom rules
3. ✓ Tests verify error messages

Create this test file now.
```

### Prompt 6.5: Create Validation Rule Tests

```
Create unit tests for individual validation rules.

CREATE FILE: Tests/DynamicForms.Core.V2.Tests/Validation/ValidationRuleTests.cs

REQUIREMENTS:
Create ValidationRuleTests with test methods for each rule:

1. RequiredFieldRule Tests:
   - RequiredField_WithValue_ReturnsSuccess
   - RequiredField_WithNullValue_ReturnsError
   - RequiredField_WithWhitespace_ReturnsError
   - NonRequiredField_WithNullValue_ReturnsSuccess

2. LengthValidationRule Tests:
   - FieldWithMinLength_ShortValue_ReturnsError
   - FieldWithMaxLength_LongValue_ReturnsError
   - FieldWithLength_ValidValue_ReturnsSuccess

3. PatternValidationRule Tests:
   - FieldWithPattern_MatchingValue_ReturnsSuccess
   - FieldWithPattern_NonMatchingValue_ReturnsError

4. EmailValidationRule Tests:
   - ValidEmail_ReturnsSuccess
   - InvalidEmail_ReturnsError

Each test should:
- Create a FormFieldNode with appropriate schema
- Call rule.ValidateAsync(...)
- Assert result using FluentAssertions

TEMPLATE FOR ONE TEST:
[Fact]
public async Task RequiredField_WithValue_ReturnsSuccess()
{
    // Arrange
    var field = new FormFieldNode
    {
        Schema = FormFieldSchema.CreateTextField("test", "Test", isRequired: true, order: 1)
    };
    var rule = new RequiredFieldRule();

    // Act
    var result = await rule.ValidateAsync(field, "Some value", new());

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
}

ACCEPTANCE CRITERIA:
1. ✓ All validation rules have test coverage
2. ✓ Both success and error cases tested
3. ✓ All tests pass

Create this test file now with all rule tests.
```

---

## Phase 7: Integration Tests

### Prompt 7.1: Create End-to-End Integration Test

```
Create an integration test that exercises the full pipeline.

CREATE FILE: Tests/DynamicForms.Core.V2.Tests/Integration/EndToEndTests.cs

REQUIREMENTS:
Create EndToEndTests class with this comprehensive test:

Test: CompleteWorkflow_CreateValidateBuildNavigate_WorksCorrectly

This test should:

1. CREATE SCHEMA:
   - Create FormModuleSchema with FormModuleSchema.Create()
   - Add 5-6 fields with different types (section, textbox, dropdown, etc.)
   - Include parent-child relationships
   - Include conditional rules
   - Include validation rules

2. SERIALIZE:
   - Serialize to JSON using System.Text.Json
   - Assert JSON is valid and contains expected properties
   - Verify JSON size is reasonable

3. DESERIALIZE:
   - Deserialize JSON back to FormModuleSchema
   - Assert all properties match original

4. BUILD HIERARCHY:
   - Use FormHierarchyService to build runtime
   - Assert hierarchy built correctly
   - Assert parent-child references correct
   - Assert metrics calculated

5. VALIDATE:
   - Create sample form data (some valid, some invalid)
   - Use FormValidationService to validate
   - Assert validation catches expected errors

6. NAVIGATE:
   - Use runtime.GetFieldsInOrder()
   - Verify fields returned in correct order
   - Verify depth-first traversal

IMPORTANT:
- This is a realistic end-to-end test
- Use real services (not mocked)
- Verify the complete workflow works
- Namespace: DynamicForms.Core.V2.Tests.Integration

ACCEPTANCE CRITERIA:
1. ✓ Test creates realistic form schema
2. ✓ JSON serialization round-trip works
3. ✓ Hierarchy builds correctly
4. ✓ Validation works correctly
5. ✓ Navigation works correctly
6. ✓ Test passes end-to-end

Create this comprehensive integration test now.
```

---

## Phase 8: Documentation and Examples

### Prompt 8.1: Create Usage Examples

```
Create a comprehensive usage examples file.

CREATE FILE: Src/DynamicForms.Core.V2/README.md

REQUIREMENTS:
Create markdown documentation with these sections:

1. OVERVIEW
   - Brief description of the library
   - Key features
   - Design principles

2. INSTALLATION
   - How to reference the project
   - How to register services with DI

3. QUICK START
   - Create a simple field
   - Create a simple module
   - Build hierarchy
   - Validate form data

4. CREATING FORMS
   - Field factory methods examples
   - Building complex hierarchies
   - Adding conditional rules
   - Adding validation rules

5. VALIDATION
   - Using built-in validation rules
   - Creating custom validation rules
   - Registering custom rules

6. JSON SERIALIZATION
   - Serialization example
   - Deserialization example
   - Schema vs Runtime explanation

7. ADVANCED TOPICS
   - Type-safe field configurations
   - Workflow creation
   - Performance considerations

8. MIGRATION FROM V1
   - Key differences
   - Migration strategy
   - Conversion examples

Use code examples from ALTERNATIVE_DESIGN_EXAMPLES.md.

ACCEPTANCE CRITERIA:
1. ✓ README covers all major topics
2. ✓ Code examples are correct and compile
3. ✓ Includes both simple and advanced examples

Create this README.md file now.
```

### Prompt 8.2: Create XML Documentation Build Configuration

```
Configure the project to generate XML documentation.

MODIFY FILE: Src/DynamicForms.Core.V2/DynamicForms.Core.V2.csproj

Add to PropertyGroup:
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <DocumentationFile>bin\$(Configuration)\$(TargetFramework)\DynamicForms.Core.V2.xml</DocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>

ACCEPTANCE CRITERIA:
1. ✓ XML documentation file generated on build
2. ✓ IntelliSense shows documentation in Visual Studio

Make this change now.
```

---

## Phase 9: Verification and Quality Checks

### Prompt 9.1: Run All Tests and Fix Issues

```
I need you to:

1. BUILD THE PROJECT:
   - Run: dotnet build Src/DynamicForms.Core.V2/DynamicForms.Core.V2.csproj
   - Verify no errors or warnings
   - If there are errors, fix them

2. RUN ALL TESTS:
   - Run: dotnet test Tests/DynamicForms.Core.V2.Tests/
   - Verify all tests pass
   - If tests fail, analyze and fix

3. CODE QUALITY CHECKS:
   - Verify all public classes have XML documentation
   - Verify naming conventions followed
   - Verify no compiler warnings

4. REPORT RESULTS:
   - Number of tests run
   - Number passed/failed
   - Any warnings or issues found
   - Summary of implemented features

ACCEPTANCE CRITERIA:
1. ✓ Project builds with zero errors
2. ✓ All tests pass (100% pass rate)
3. ✓ No compiler warnings
4. ✓ All public APIs documented

Run these verifications now and report results.
```

### Prompt 9.2: Create Simple Console Demo

```
Create a simple console application to demonstrate the library.

CREATE NEW PROJECT:
- Name: DynamicForms.Demo
- Type: Console application (.NET 9.0)
- Location: Demos/DynamicForms.Demo/
- Reference: Src/DynamicForms.Core.V2/

PROGRAM.CS REQUIREMENTS:
Create a demo that:

1. Sets up dependency injection
2. Creates a sample grant application form with:
   - Organization Info section
     - Organization name (required text)
     - Organization type (dropdown)
     - Charity number (conditional on type, with pattern validation)
   - Project Info section
     - Project title (required)
     - Project description
     - Budget amount (numeric with validation)
3. Serializes to JSON and displays
4. Deserializes back
5. Builds hierarchy using service
6. Displays hierarchy structure (indented tree)
7. Validates with sample data (some valid, some invalid)
8. Displays validation results

OUTPUT should show:
- JSON schema (formatted)
- Hierarchy tree
- Validation results
- Metrics (field count, depth, complexity)

IMPORTANT:
- Use Console.WriteLine with colors for different sections
- Make output easy to read
- Demonstrate all key features

ACCEPTANCE CRITERIA:
1. ✓ Demo runs without errors
2. ✓ Output is clear and informative
3. ✓ Demonstrates all major features

Create this demo project now.
```

---

## Phase 10: Final Deliverables

### Prompt 10.1: Create Package Metadata

```
Add NuGet package metadata to the project.

MODIFY FILE: Src/DynamicForms.Core.V2/DynamicForms.Core.V2.csproj

Add these properties to PropertyGroup:
<PropertyGroup>
  <!-- Existing properties -->
  <TargetFramework>net9.0</TargetFramework>

  <!-- Package metadata -->
  <PackageId>DynamicForms.Core.V2</PackageId>
  <Version>2.0.0-beta1</Version>
  <Authors>Your Organization</Authors>
  <Company>Your Organization</Company>
  <Description>Simplified, high-performance dynamic forms library for .NET. Enterprise and government-grade with multilingual support, hierarchical fields, and extensive validation capabilities.</Description>
  <PackageTags>forms;dynamic-forms;validation;multilingual;hierarchical;government;enterprise</PackageTags>
  <RepositoryUrl>https://github.com/yourorg/DynamicForms</RepositoryUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageReadmeFile>README.md</PackageReadmeFile>

  <!-- Build settings -->
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>

<ItemGroup>
  <None Include="README.md" Pack="true" PackagePath="\" />
</ItemGroup>

ACCEPTANCE CRITERIA:
1. ✓ Package metadata complete
2. ✓ Can build NuGet package: dotnet pack
3. ✓ README included in package

Make these changes now.
```

### Prompt 10.2: Create Implementation Summary

```
Create a comprehensive summary document of what was implemented.

CREATE FILE: IMPLEMENTATION_SUMMARY.md

This document should include:

1. PROJECT STRUCTURE
   - List all created projects
   - List all namespaces
   - List all major classes

2. SCHEMA CLASSES
   - FormFieldSchema (with property count)
   - FormModuleSchema
   - FormWorkflowSchema
   - Supporting types (records, enums)

3. RUNTIME CLASSES
   - FormFieldNode
   - FormModuleRuntime
   - HierarchyMetrics

4. SERVICE INTERFACES
   - IFormHierarchyService
   - IFormValidationService
   - IFormModuleRepository
   - IValidationRule

5. SERVICE IMPLEMENTATIONS
   - FormHierarchyService
   - FormValidationService
   - Built-in validation rules (list each)

6. TESTS
   - Test projects
   - Test coverage summary
   - Number of tests

7. DOCUMENTATION
   - README.md
   - XML documentation
   - Code examples

8. METRICS
   - Total lines of code
   - Number of classes/interfaces
   - Number of tests
   - Test coverage percentage (if available)

9. FEATURES IMPLEMENTED
   - Checklist of features from design doc
   - Mark each as ✓ complete

10. NEXT STEPS
    - What's not yet implemented
    - Suggestions for phase 2

Use markdown tables and checkboxes for clarity.

ACCEPTANCE CRITERIA:
1. ✓ Complete inventory of implementation
2. ✓ Accurate metrics
3. ✓ Clear summary of what's done and what's next

Create this summary document now.
```

---

## Verification Checklist

After executing all prompts, verify:

### Code Completeness
- [ ] All schema classes created (FormFieldSchema, FormModuleSchema, FormWorkflowSchema)
- [ ] All runtime classes created (FormFieldNode, FormModuleRuntime, HierarchyMetrics)
- [ ] All service interfaces created (IFormHierarchyService, IFormValidationService, IFormModuleRepository)
- [ ] All service implementations created (FormHierarchyService, FormValidationService)
- [ ] All validation rules created (Required, Length, Pattern, Email)
- [ ] DI extension methods created
- [ ] All supporting types created (enums, records)

### Testing
- [ ] Schema tests pass
- [ ] Service tests pass
- [ ] Validation rule tests pass
- [ ] Integration test passes
- [ ] All tests pass with 100% success rate

### Documentation
- [ ] XML documentation on all public APIs
- [ ] README.md with examples
- [ ] Implementation summary created

### Build & Package
- [ ] Project builds with zero errors
- [ ] No compiler warnings
- [ ] NuGet package can be created
- [ ] Demo app runs successfully

### Quality
- [ ] Code follows C# naming conventions
- [ ] All classes in appropriate namespaces
- [ ] Consistent code style
- [ ] No TODO comments left in code

---

## Tips for Success

1. **Execute in Order**: Prompts build on each other - don't skip ahead
2. **Verify Each Step**: Check acceptance criteria before moving on
3. **Reference Design Docs**: Point Claude to ALTERNATIVE_DESIGN_PROPOSAL.md for details
4. **Fix as You Go**: Don't accumulate errors - fix immediately
5. **Test Frequently**: Run tests after each phase
6. **Ask for Clarification**: If a prompt is unclear, ask Claude to explain before implementing

---

## Troubleshooting

### If Tests Fail
```
The tests in [test file] are failing. Here are the errors:
[paste error messages]

Please analyze these failures and fix the implementation. Show me what you changed and why.
```

### If Build Fails
```
The project is not building. Here are the compilation errors:
[paste errors]

Please fix these compilation errors. Ensure all namespaces and using statements are correct.
```

### If Implementation is Incomplete
```
Looking at [class name], I notice it's missing [feature]. According to ALTERNATIVE_DESIGN_PROPOSAL.md section [section], this should include [details].

Please add the missing functionality.
```

---

## Final Prompt - Comprehensive Review

After completing all prompts, use this final verification prompt:

```
I need you to perform a comprehensive review of the DynamicForms.Core.V2 implementation.

REVIEW CHECKLIST:

1. COMPLETENESS
   - Verify all classes from ALTERNATIVE_DESIGN_PROPOSAL.md are implemented
   - Check all required properties exist
   - Verify all required methods exist

2. CORRECTNESS
   - Build the project: dotnet build
   - Run all tests: dotnet test
   - Verify all tests pass

3. CODE QUALITY
   - Check XML documentation coverage
   - Verify naming conventions
   - Check for compiler warnings
   - Review code for best practices

4. FUNCTIONALITY
   - Run the demo application
   - Verify it produces expected output
   - Test JSON serialization round-trip
   - Test hierarchy building
   - Test validation

5. DOCUMENTATION
   - README.md is complete
   - Examples are correct
   - Implementation summary is accurate

Please review each item and report:
- ✓ What's complete and working
- ⚠️ What has issues
- ❌ What's missing

For any issues found, provide fixes.
```

---

## Success Criteria

Implementation is complete when:

✅ All prompts executed successfully
✅ Project builds with zero errors and warnings
✅ All tests pass (100% pass rate)
✅ Demo application runs and demonstrates all features
✅ XML documentation complete on all public APIs
✅ README.md with comprehensive examples
✅ Can create NuGet package
✅ Implementation matches design specification in ALTERNATIVE_DESIGN_PROPOSAL.md

---

*Total Estimated Time: 6-8 hours of focused implementation*
*Recommended Approach: Execute 2-3 phases per session, verify thoroughly between sessions*
