# Entity Framework Core Migrations Guide

## Overview

This guide explains how to use Entity Framework Core migrations to manage the DynamicForms Editor database schema.

## Prerequisites

- .NET 9.0 SDK installed
- SQL Server, SQL Server Express, or LocalDB available
- `DynamicForms.Editor.Data` project built successfully

## Quick Start

### Apply Existing Migrations

If migrations already exist and you just need to create/update the database:

```bash
# From the solution root
dotnet ef database update --project Src/DynamicForms.Editor.Data

# Or from the DynamicForms.Editor.Data directory
cd Src/DynamicForms.Editor.Data
dotnet ef database update
```

This will:
1. Create the database if it doesn't exist
2. Apply all pending migrations
3. Create all tables, indexes, and constraints

### Verify Database Creation

After running the migration, verify the database:

```bash
# List all migrations
dotnet ef migrations list --project Src/DynamicForms.Editor.Data

# Check database connection
dotnet ef database update --connection "Server=(localdb)\mssqllocaldb;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;" --project Src/DynamicForms.Editor.Data
```

---

## Migration Commands

### Create a New Migration

When you modify entity classes or DbContext configuration:

```bash
# Create migration with descriptive name
dotnet ef migrations add AddUserTrackingFields --project Src/DynamicForms.Editor.Data

# Create migration with output directory (optional)
dotnet ef migrations add AddUserTrackingFields \
  --project Src/DynamicForms.Editor.Data \
  --output-dir Migrations
```

**Migration Naming Conventions:**
- Use PascalCase: `AddNewTable`, `UpdateIndexes`
- Be descriptive: `AddEmailNotificationSettings` not `Update1`
- Prefix with action: `Add`, `Update`, `Remove`, `Rename`

### Apply Migrations

```bash
# Apply all pending migrations
dotnet ef database update --project Src/DynamicForms.Editor.Data

# Apply to specific migration
dotnet ef database update InitialEditorDatabase --project Src/DynamicForms.Editor.Data

# Rollback all migrations
dotnet ef database update 0 --project Src/DynamicForms.Editor.Data
```

### Remove Last Migration

If you haven't applied a migration yet, you can remove it:

```bash
# Remove the most recent migration (must not be applied to database)
dotnet ef migrations remove --project Src/DynamicForms.Editor.Data

# Force remove (use with caution)
dotnet ef migrations remove --force --project Src/DynamicForms.Editor.Data
```

### List All Migrations

```bash
# List all migrations and their status
dotnet ef migrations list --project Src/DynamicForms.Editor.Data
```

### Generate SQL Script

Generate SQL script from migrations (useful for production deployments):

```bash
# Generate script for all migrations
dotnet ef migrations script \
  --project Src/DynamicForms.Editor.Data \
  --output Database/migrations.sql

# Generate script from specific migration to latest
dotnet ef migrations script InitialEditorDatabase \
  --project Src/DynamicForms.Editor.Data \
  --output Database/update.sql

# Generate script between two migrations
dotnet ef migrations script Migration1 Migration2 \
  --project Src/DynamicForms.Editor.Data \
  --output Database/partial.sql

# Generate idempotent script (safe to run multiple times)
dotnet ef migrations script \
  --project Src/DynamicForms.Editor.Data \
  --idempotent \
  --output Database/idempotent.sql
```

---

## Connection String Configuration

### Default Connection String

The `DesignTimeDbContextFactory` uses LocalDB by default:

```
Server=(localdb)\mssqllocaldb;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

### Override Connection String

You can override the connection string using an environment variable:

#### Windows (PowerShell)
```powershell
$env:EDITOR_DB_CONNECTION_STRING="Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet ef database update --project Src/DynamicForms.Editor.Data
```

#### Windows (CMD)
```cmd
set EDITOR_DB_CONNECTION_STRING=Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
dotnet ef database update --project Src/DynamicForms.Editor.Data
```

#### Linux/Mac
```bash
export EDITOR_DB_CONNECTION_STRING="Server=localhost;Database=DynamicFormsEditor;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
dotnet ef database update --project Src/DynamicForms.Editor.Data
```

### Use Inline Connection String

```bash
dotnet ef database update \
  --connection "Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;" \
  --project Src/DynamicForms.Editor.Data
```

### Common Connection Strings

**LocalDB (Development):**
```
Server=(localdb)\mssqllocaldb;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server Express:**
```
Server=localhost\SQLEXPRESS;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server (Windows Auth):**
```
Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

**SQL Server (SQL Auth):**
```
Server=localhost;Database=DynamicFormsEditor;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

**Azure SQL Database:**
```
Server=tcp:yourserver.database.windows.net,1433;Database=DynamicFormsEditor;User Id=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;
```

---

## Migration Workflow

### Development Workflow

1. **Modify Entity or DbContext**
   ```csharp
   // Example: Add new property to EditorFormModule
   public string? Tags { get; set; }
   ```

2. **Create Migration**
   ```bash
   dotnet ef migrations add AddTagsToFormModule --project Src/DynamicForms.Editor.Data
   ```

3. **Review Migration**
   - Open `Migrations/XXXXXX_AddTagsToFormModule.cs`
   - Verify `Up()` method creates correct schema changes
   - Verify `Down()` method properly rolls back changes

4. **Apply Migration**
   ```bash
   dotnet ef database update --project Src/DynamicForms.Editor.Data
   ```

5. **Test Changes**
   - Verify database schema updated correctly
   - Run application and test new functionality

6. **Commit to Source Control**
   ```bash
   git add Src/DynamicForms.Editor.Data/Migrations/
   git commit -m "Add Tags property to FormModule"
   ```

### Production Deployment Workflow

1. **Generate Idempotent SQL Script**
   ```bash
   dotnet ef migrations script \
     --project Src/DynamicForms.Editor.Data \
     --idempotent \
     --output Database/production-update.sql
   ```

2. **Review SQL Script**
   - Check for breaking changes
   - Ensure backward compatibility if needed
   - Plan for rollback strategy

3. **Backup Production Database**
   ```sql
   BACKUP DATABASE DynamicFormsEditor
   TO DISK = 'C:\Backups\DynamicFormsEditor_BeforeMigration.bak';
   ```

4. **Apply Script to Production**
   - Run during maintenance window
   - Test thoroughly after deployment
   - Monitor for errors

5. **Rollback if Needed**
   ```bash
   # Generate rollback script
   dotnet ef migrations script CurrentMigration PreviousMigration \
     --project Src/DynamicForms.Editor.Data \
     --output Database/rollback.sql
   ```

---

## Troubleshooting

### Issue: "Build failed" when running migrations

**Solution:** Build the project first
```bash
dotnet build Src/DynamicForms.Editor.Data
dotnet ef migrations add YourMigration --project Src/DynamicForms.Editor.Data --no-build
```

### Issue: "Unable to create an object of type 'ApplicationDbContext'"

**Cause:** Missing design-time factory or incorrect configuration

**Solution:** Ensure `DesignTimeDbContextFactory.cs` exists and is properly configured

### Issue: "A connection was successfully established... but then an error occurred"

**Cause:** Connection string incorrect or SQL Server not running

**Solution:**
1. Verify SQL Server is running: `sqlcmd -S (localdb)\mssqllocaldb -Q "SELECT @@VERSION"`
2. Check connection string is correct
3. Try connecting with SSMS first to verify credentials

### Issue: "The migration 'XXXX' has already been applied to the database"

**Cause:** Migration already applied, trying to re-add it

**Solution:**
- To modify: Remove the migration first (`dotnet ef migrations remove`)
- To reapply: Rollback first (`dotnet ef database update PreviousMigration`)

### Issue: "There is already an object named 'XXX' in the database"

**Cause:** Schema out of sync with migrations

**Solution:**
```bash
# Option 1: Drop database and recreate
dotnet ef database drop --project Src/DynamicForms.Editor.Data
dotnet ef database update --project Src/DynamicForms.Editor.Data

# Option 2: Create new migration to fix discrepancies
dotnet ef migrations add FixSchemaDiscrepancies --project Src/DynamicForms.Editor.Data
```

### Issue: Migration takes too long or hangs

**Cause:** Exclusive lock acquired by another process

**Solution:**
1. Close all connections to the database
2. Check for running queries in SSMS
3. Restart SQL Server service if needed

---

## Best Practices

### 1. Always Review Generated Migrations

```bash
# After creating migration
dotnet ef migrations add AddNewFeature --project Src/DynamicForms.Editor.Data

# Review the generated file
code Src/DynamicForms.Editor.Data/Migrations/XXXXXX_AddNewFeature.cs
```

Check for:
- Correct column types
- Proper indexes
- Data migration needs (if modifying existing columns)
- Breaking changes

### 2. Test Migrations Locally First

```bash
# Create test database
dotnet ef database update --connection "Server=(localdb)\mssqllocaldb;Database=DynamicFormsEditor_Test;..."

# Run migration
dotnet ef database update --project Src/DynamicForms.Editor.Data

# Test rollback
dotnet ef database update PreviousMigration --project Src/DynamicForms.Editor.Data
```

### 3. Use Descriptive Migration Names

```bash
# Good names
dotnet ef migrations add AddEmailNotificationSettings
dotnet ef migrations add UpdateFormModuleIndexes
dotnet ef migrations add RemoveDeprecatedFields

# Bad names
dotnet ef migrations add Update1
dotnet ef migrations add Changes
dotnet ef migrations add Fix
```

### 4. Never Modify Applied Migrations

Once a migration is applied (especially in production), **never modify it**. Instead:
- Create a new migration to fix issues
- Use data migrations for complex changes

### 5. Keep Migrations Small and Focused

Create separate migrations for:
- Schema changes
- Data migrations
- Index optimizations

### 6. Version Control All Migrations

```bash
# Always commit migration files
git add Src/DynamicForms.Editor.Data/Migrations/
git commit -m "Migration: Add user notification preferences"
```

### 7. Document Complex Migrations

Add XML comments to explain non-obvious changes:

```csharp
/// <summary>
/// Migrates existing FormModule status values from legacy format to new format.
/// Legacy: "0" = Draft, "1" = Published
/// New: "Draft", "Published", "Archived"
/// </summary>
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Migration code...
}
```

---

## Advanced Scenarios

### Custom Migration Code

Sometimes you need custom SQL:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Custom SQL for data migration
    migrationBuilder.Sql(@"
        UPDATE EditorFormModules
        SET Status = 'Published'
        WHERE Status = '1';
    ");
}
```

### Seed Data in Migrations

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create table first
    migrationBuilder.CreateTable(/*...*/);

    // Insert seed data
    migrationBuilder.InsertData(
        table: "EditorConfiguration",
        columns: new[] { "ConfigKey", "ConfigValue", "ConfigType", "Description" },
        values: new object[] { "NewSetting", "DefaultValue", "String", "Description" }
    );
}
```

### Handling Production Data

When modifying columns with existing data:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add new column (nullable)
    migrationBuilder.AddColumn<string>(
        name: "NewColumn",
        table: "EditorFormModules",
        nullable: true);

    // 2. Migrate data
    migrationBuilder.Sql(@"
        UPDATE EditorFormModules
        SET NewColumn = CAST(OldColumn AS NVARCHAR(500));
    ");

    // 3. Make column required
    migrationBuilder.AlterColumn<string>(
        name: "NewColumn",
        table: "EditorFormModules",
        nullable: false);

    // 4. Drop old column
    migrationBuilder.DropColumn(
        name: "OldColumn",
        table: "EditorFormModules");
}
```

---

## Migration Files

### File Structure

After creating a migration, you'll see:

```
Migrations/
├── 20251128153515_InitialEditorDatabase.cs          # Migration logic
├── 20251128153515_InitialEditorDatabase.Designer.cs # Metadata
└── ApplicationDbContextModelSnapshot.cs              # Current model state
```

**Never manually edit:**
- `.Designer.cs` files
- `ApplicationDbContextModelSnapshot.cs`

**Safe to edit:**
- Main migration `.cs` file (but only before applying to production)

---

## Resources

- [EF Core Migrations Documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Migration Command Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [Managing Schemas](https://learn.microsoft.com/en-us/ef/core/managing-schemas/)
- [SQL Server Provider](https://learn.microsoft.com/en-us/ef/core/providers/sql-server/)

---

## Quick Reference

```bash
# Create migration
dotnet ef migrations add <Name> --project Src/DynamicForms.Editor.Data

# Apply migrations
dotnet ef database update --project Src/DynamicForms.Editor.Data

# Remove last migration
dotnet ef migrations remove --project Src/DynamicForms.Editor.Data

# List migrations
dotnet ef migrations list --project Src/DynamicForms.Editor.Data

# Generate SQL script
dotnet ef migrations script --project Src/DynamicForms.Editor.Data --output script.sql

# Drop database
dotnet ef database drop --project Src/DynamicForms.Editor.Data

# Update with custom connection
dotnet ef database update --connection "Your-Connection-String" --project Src/DynamicForms.Editor.Data
```

---

*Last Updated: January 2025*
*EF Core Version: 9.0*
