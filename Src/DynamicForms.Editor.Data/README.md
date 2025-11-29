# DynamicForms.Editor.Data

Entity Framework Core data access layer for the Visual Form Editor.

## Overview

This project provides the complete data access layer for the DynamicForms Visual Editor, including:
- Entity classes matching the database schema
- ApplicationDbContext with proper Fluent API configuration
- Support for both draft/working data and published production data

## Project Structure

```
DynamicForms.Editor.Data/
├── Entities/
│   ├── EditorFormModule.cs           - Draft form modules
│   ├── EditorWorkflow.cs             - Draft workflows
│   ├── EditorHistorySnapshot.cs      - Undo/redo snapshots
│   ├── PublishedFormModule.cs        - Published forms (production)
│   ├── PublishedWorkflow.cs          - Published workflows (production)
│   └── EditorConfigurationItem.cs    - Configuration settings
└── ApplicationDbContext.cs            - EF Core DbContext
```

## Entities

### Editor Entities (Draft/Working Data)

#### EditorFormModule
Stores form modules being edited. Each draft can be saved multiple times before publishing.

**Key Properties:**
- `ModuleId` - Business identifier (not unique across drafts)
- `SchemaJson` - Full FormModuleSchema as JSON
- `Status` - Draft, Published, or Archived
- `Version` - Version number (increments on publish)

#### EditorWorkflow
Stores multi-module workflows being edited.

**Similar structure to EditorFormModule**

#### EditorHistorySnapshot
Stores undo/redo history snapshots for editor sessions.

**Key Properties:**
- `EditorSessionId` - Groups snapshots from one editing session
- `SnapshotJson` - Full snapshot of entity state
- `SequenceNumber` - Order within session for undo/redo
- `ActionDescription` - Human-readable action (e.g., "Added field 'Email'")

### Published Entities (Production Data)

#### PublishedFormModule
Stores published form modules that are ready for production use.

**Key Properties:**
- `ModuleId` - Business identifier (same across versions)
- `Version` - Version number (auto-increments on publish)
- `IsActive` - Only one active version per ModuleId
- `SchemaJson` - Full FormModuleSchema as JSON

**Query Pattern:**
```csharp
// Get active version of a module
var activeModule = await context.PublishedFormModules
    .Where(m => m.ModuleId == moduleId && m.IsActive)
    .OrderByDescending(m => m.Version)
    .FirstOrDefaultAsync();
```

#### PublishedWorkflow
Similar to PublishedFormModule but for workflows.

### Configuration Entity

#### EditorConfigurationItem
Stores application configuration settings.

**Key Properties:**
- `ConfigKey` - Unique key (e.g., "AutoSave.IntervalSeconds")
- `ConfigValue` - Value as string
- `ConfigType` - Data type (Int, String, Bool, Decimal)

**Default Configuration:**
- `AutoSave.IntervalSeconds` = 30
- `UndoRedo.MaxActions` = 100
- `History.RetentionDays` = 90

## ApplicationDbContext

The DbContext includes:

### DbSets
- `EditorFormModules` - Draft form modules
- `EditorWorkflows` - Draft workflows
- `EditorHistory` - Undo/redo snapshots
- `PublishedFormModules` - Published forms
- `PublishedWorkflows` - Published workflows
- `EditorConfiguration` - Configuration settings

### Features

#### Automatic Timestamp Updates
The context automatically updates `ModifiedAt` timestamps when entities are modified:

```csharp
// ModifiedAt is updated automatically
var module = await context.EditorFormModules.FindAsync(id);
module.Title = "New Title";
await context.SaveChangesAsync(); // ModifiedAt updated to current UTC time
```

#### Fluent API Configuration
All entities are configured using Fluent API to match the database schema exactly:
- Column types (NVARCHAR lengths)
- Required/optional fields
- Default values
- Indexes (including composite and filtered indexes)
- Unique constraints

#### Indexes
Properly configured indexes for performance:

**EditorFormModules:**
- `IX_EditorFormModules_ModuleId`
- `IX_EditorFormModules_Status`
- `IX_EditorFormModules_ModifiedAt` (descending)

**EditorHistory:**
- `IX_EditorHistory_EditorSessionId` (composite with SequenceNumber DESC)
- `IX_EditorHistory_EntityTypeId` (composite)
- `IX_EditorHistory_CreatedAt`

**PublishedFormModules:**
- `IX_PublishedFormModules_ModuleId_Version` (composite, Version DESC)
- `IX_PublishedFormModules_IsActive` (filtered: WHERE IsActive = 1)

## Usage

### Dependency Injection Setup

```csharp
// In Program.cs or Startup.cs
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### Connection String Examples

**LocalDB (Development):**
```json
"Server=(localdb)\\mssqllocaldb;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;"
```

**SQL Server Express:**
```json
"Server=localhost\\SQLEXPRESS;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;"
```

**SQL Server:**
```json
"Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;"
```

### Basic CRUD Operations

```csharp
public class ExampleService
{
    private readonly ApplicationDbContext _context;

    public ExampleService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Create a new draft module
    public async Task<EditorFormModule> CreateDraftAsync(string title, string schemaJson)
    {
        var module = new EditorFormModule
        {
            ModuleId = GetNextModuleId(),
            Title = title,
            SchemaJson = schemaJson,
            Status = "Draft",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        _context.EditorFormModules.Add(module);
        await _context.SaveChangesAsync();
        return module;
    }

    // Load draft module
    public async Task<EditorFormModule?> GetDraftAsync(int moduleId)
    {
        return await _context.EditorFormModules
            .FirstOrDefaultAsync(m => m.ModuleId == moduleId && m.Status == "Draft");
    }

    // Update draft module
    public async Task UpdateDraftAsync(EditorFormModule module)
    {
        _context.EditorFormModules.Update(module);
        await _context.SaveChangesAsync();
        // ModifiedAt is automatically updated
    }

    // Get active published version
    public async Task<PublishedFormModule?> GetActivePublishedAsync(int moduleId)
    {
        return await _context.PublishedFormModules
            .Where(m => m.ModuleId == moduleId && m.IsActive)
            .OrderByDescending(m => m.Version)
            .FirstOrDefaultAsync();
    }

    // Get configuration value
    public async Task<string?> GetConfigValueAsync(string key)
    {
        var config = await _context.EditorConfiguration
            .FirstOrDefaultAsync(c => c.ConfigKey == key);
        return config?.ConfigValue;
    }
}
```

### Querying with Includes

```csharp
// History snapshots for a session (ordered for undo/redo)
var snapshots = await _context.EditorHistory
    .Where(h => h.EditorSessionId == sessionId)
    .OrderByDescending(h => h.SequenceNumber)
    .ToListAsync();

// All published versions of a module
var versions = await _context.PublishedFormModules
    .Where(m => m.ModuleId == moduleId)
    .OrderByDescending(m => m.Version)
    .ToListAsync();
```

## Migrations

### Create Initial Migration

```bash
# From the solution root
dotnet ef migrations add InitialEditorDatabase \
  --project Src/DynamicForms.Editor.Data \
  --startup-project Src/DynamicForms.Editor  # When editor project exists

# Or from the Editor.Data directory
cd Src/DynamicForms.Editor.Data
dotnet ef migrations add InitialEditorDatabase
```

### Apply Migration

```bash
dotnet ef database update \
  --project Src/DynamicForms.Editor.Data
```

### Generate SQL Script

```bash
dotnet ef migrations script \
  --project Src/DynamicForms.Editor.Data \
  --output Database/migrations.sql
```

## Best Practices

### 1. Always Use UTC Timestamps
```csharp
module.CreatedAt = DateTime.UtcNow;  // Good
module.CreatedAt = DateTime.Now;      // Bad (local time)
```

### 2. Let DbContext Update ModifiedAt
```csharp
// Don't manually set ModifiedAt
module.Title = "New Title";
await context.SaveChangesAsync(); // ModifiedAt updated automatically
```

### 3. Use AsNoTracking for Read-Only Queries
```csharp
var modules = await context.EditorFormModules
    .AsNoTracking()  // Better performance for read-only
    .Where(m => m.Status == "Published")
    .ToListAsync();
```

### 4. Handle Concurrency Conflicts
```csharp
try
{
    await context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Handle conflict (e.g., reload and retry)
}
```

### 5. Dispose Context Properly
When not using DI, always dispose the context:
```csharp
using var context = new ApplicationDbContext(options);
// Use context
// Auto-disposed when leaving scope
```

## Package References

- **Microsoft.EntityFrameworkCore.SqlServer** (9.0.*) - SQL Server provider
- **Microsoft.EntityFrameworkCore.Tools** (9.0.*) - Migration tools

## Target Framework

- .NET 9.0

## Next Steps

1. ✅ Entity classes created
2. ✅ ApplicationDbContext configured
3. ⏭️ Create EF Core migrations (Prompt 1.3)
4. ⏭️ Create repository interfaces (Prompt 1.4)
5. ⏭️ Implement repositories (Prompt 1.5)

## Related Documentation

- [Database Schema](../../Database/README.md) - SQL script and schema documentation
- [Visual Editor Design](../../VISUAL_EDITOR_DESIGN_PROPOSAL.md) - Complete architecture
- [Implementation Guide](../../VISUAL_EDITOR_IMPLEMENTATION_PROMPTS.md) - Step-by-step prompts
