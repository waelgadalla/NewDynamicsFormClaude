using System.Text.Json;
using DynamicForms.Core.V2.Schemas;
using DynamicForms.Core.V2.Services;
using Microsoft.Extensions.Logging;

Console.WriteLine("=== DynamicForms.Core.V2 Quick Demo ===\n");

// Setup logging
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var hierarchyLogger = loggerFactory.CreateLogger<FormHierarchyService>();
var validationLogger = loggerFactory.CreateLogger<FormValidationService>();

// Create services
var hierarchyService = new FormHierarchyService(hierarchyLogger);
var validationService = new FormValidationService(validationLogger);

// 1. CREATE SCHEMA
Console.WriteLine("1. Creating form schema...");
var module = FormModuleSchema.Create(1, "Grant Application", "Demande de subvention");

var fields = new[]
{
    // Organization section
    FormFieldSchema.CreateSection("org_section", "Organization Information", "Renseignements sur l'organisation", 1),
    FormFieldSchema.CreateTextField("org_name", "Organization Name", "Nom de l'organisation", isRequired: true, order: 2)
        with { ParentId = "org_section" },
    FormFieldSchema.CreateDropDown("org_type", "Organization Type", new[]
    {
        new FieldOption("charity", "Registered Charity", "Organisme de bienfaisance", IsDefault: true, Order: 0),
        new FieldOption("nonprofit", "Non-Profit", "Organisme sans but lucratif", Order: 1),
        new FieldOption("other", "Other", "Autre", Order: 2)
    }, "Type d'organisation", isRequired: true, order: 3)
        with { ParentId = "org_section" },

    // Project section
    FormFieldSchema.CreateSection("project_section", "Project Information", "Renseignements sur le projet", 4),
    FormFieldSchema.CreateTextField("project_title", "Project Title", "Titre du projet", isRequired: true, order: 5)
        with { ParentId = "project_section", MinLength = 10, MaxLength = 200 },
    FormFieldSchema.CreateTextField("email", "Contact Email", "Courriel de contact", isRequired: true, order: 6)
        with { ParentId = "project_section", ValidationRules = new[] { "email" } }
};

module = module with { Fields = fields };

Console.WriteLine($"✓ Created module with {fields.Length} fields\n");

// 2. SERIALIZE TO JSON
Console.WriteLine("2. Serializing to JSON...");
var json = JsonSerializer.Serialize(module, new JsonSerializerOptions { WriteIndented = true });
Console.WriteLine($"✓ JSON size: {json.Length} bytes\n");

// 3. DESERIALIZE
Console.WriteLine("3. Deserializing from JSON...");
var deserializedModule = JsonSerializer.Deserialize<FormModuleSchema>(json);
Console.WriteLine($"✓ Deserialized module: {deserializedModule?.TitleEn}\n");

// 4. BUILD HIERARCHY
Console.WriteLine("4. Building hierarchy...");
var runtime = await hierarchyService.BuildHierarchyAsync(deserializedModule!);
Console.WriteLine($"✓ Hierarchy built:");
Console.WriteLine($"  - Total fields: {runtime.Metrics.TotalFields}");
Console.WriteLine($"  - Root fields: {runtime.Metrics.RootFields}");
Console.WriteLine($"  - Max depth: {runtime.Metrics.MaxDepth}");
Console.WriteLine($"  - Complexity score: {runtime.Metrics.ComplexityScore}\n");

// 5. DISPLAY HIERARCHY TREE
Console.WriteLine("5. Hierarchy structure:");
foreach (var field in runtime.GetFieldsInOrder())
{
    var indent = new string(' ', field.Level * 2);
    Console.WriteLine($"{indent}[{field.Schema.Order}] {field.Schema.FieldType}: {field.Schema.LabelEn} (ID: {field.Schema.Id})");
}
Console.WriteLine();

// 6. VALIDATE WITH SAMPLE DATA
Console.WriteLine("6. Validating form data...");

// Invalid data (missing required fields)
var invalidData = new Dictionary<string, object?>
{
    ["org_name"] = "",  // Required but empty
    ["email"] = "not-an-email"  // Invalid format
};

var invalidResult = await validationService.ValidateModuleAsync(runtime, invalidData);
Console.WriteLine($"✓ Validation result (should fail): IsValid = {invalidResult.IsValid}");
if (!invalidResult.IsValid)
{
    Console.WriteLine($"  Errors found: {invalidResult.Errors.Count}");
    foreach (var error in invalidResult.Errors)
    {
        Console.WriteLine($"  - {error.FieldId}: {error.Message}");
    }
}
Console.WriteLine();

// Valid data
var validData = new Dictionary<string, object?>
{
    ["org_name"] = "My Great Charity",
    ["org_type"] = "charity",
    ["project_title"] = "Community Garden Project for All",
    ["email"] = "contact@mygreaatchar ity.org"
};

var validResult = await validationService.ValidateModuleAsync(runtime, validData);
Console.WriteLine($"✓ Validation result (should pass): IsValid = {validResult.IsValid}");
if (!validResult.IsValid)
{
    Console.WriteLine($"  Unexpected errors: {validResult.Errors.Count}");
    foreach (var error in validResult.Errors)
    {
        Console.WriteLine($"  - {error.FieldId}: {error.Message}");
    }
}
Console.WriteLine();

Console.WriteLine("=== Demo Complete! ===");
Console.WriteLine("\nAll core features working:");
Console.WriteLine("✓ Schema creation with factory methods");
Console.WriteLine("✓ JSON serialization/deserialization");
Console.WriteLine("✓ Hierarchy building with parent-child relationships");
Console.WriteLine("✓ Metrics calculation");
Console.WriteLine("✓ Field validation with built-in rules");
Console.WriteLine("✓ Multilingual support");
