# Database Setup Guide

## Overview

This directory contains the SQL Server database schema for the DynamicForms Visual Editor.

## Database Structure

The database `DynamicFormsEditor` contains 6 tables organized into three logical groups:

### Editor Tables (Draft/Working Data)
- **EditorFormModules** - Form modules being edited (draft versions)
- **EditorWorkflows** - Multi-module workflows being edited
- **EditorHistory** - Undo/redo history snapshots

### Production Tables (Published Data)
- **PublishedFormModules** - Published form modules (production-ready)
- **PublishedWorkflows** - Published workflows (production-ready)

### Configuration
- **EditorConfiguration** - Application configuration settings

## Quick Start

### Method 1: Using SQL Server Management Studio (SSMS)

1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Open `CreateEditorDatabase.sql`
4. Click **Execute** or press F5

### Method 2: Using sqlcmd

```bash
# From the Src/Database directory
sqlcmd -S localhost -i CreateEditorDatabase.sql

# Or with specific server and authentication
sqlcmd -S localhost\SQLEXPRESS -E -i CreateEditorDatabase.sql

# Or with SQL authentication
sqlcmd -S localhost -U sa -P YourPassword -i CreateEditorDatabase.sql
```

### Method 3: Using Azure Data Studio

1. Open Azure Data Studio
2. Connect to your SQL Server instance
3. Open `CreateEditorDatabase.sql`
4. Click **Run** or press F5

## Idempotency

The script is **idempotent** - safe to run multiple times. It will:
- Create the database only if it doesn't exist
- Create tables only if they don't exist
- Insert configuration data only if not already present
- Skip creating indexes if they already exist

This means you can run it repeatedly without errors or data loss.

## Verification

After running the script, verify the setup:

### Check Database Exists
```sql
SELECT name FROM sys.databases WHERE name = 'DynamicFormsEditor';
```

### Check All Tables Exist
```sql
USE DynamicFormsEditor;

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

Expected result: 6 tables
- EditorConfiguration
- EditorFormModules
- EditorHistory
- EditorWorkflows
- PublishedFormModules
- PublishedWorkflows

### Check Configuration Data
```sql
USE DynamicFormsEditor;

SELECT ConfigKey, ConfigValue, ConfigType, Description
FROM EditorConfiguration
ORDER BY ConfigKey;
```

Expected result: 3 configuration entries
- AutoSave.IntervalSeconds = 30
- History.RetentionDays = 90
- UndoRedo.MaxActions = 100

### Check Indexes
```sql
USE DynamicFormsEditor;

SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE OBJECT_NAME(i.object_id) IN (
    'EditorFormModules',
    'EditorWorkflows',
    'EditorHistory',
    'PublishedFormModules',
    'PublishedWorkflows',
    'EditorConfiguration'
)
AND i.name IS NOT NULL
ORDER BY TableName, IndexName;
```

Expected: Multiple indexes per table for performance

## Connection Strings

### For LocalDB (Development)
```
Server=(localdb)\mssqllocaldb;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

### For SQL Server Express (Development)
```
Server=localhost\SQLEXPRESS;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

### For SQL Server (Development with Windows Auth)
```
Server=localhost;Database=DynamicFormsEditor;Trusted_Connection=True;TrustServerCertificate=True;
```

### For SQL Server (SQL Authentication)
```
Server=localhost;Database=DynamicFormsEditor;User Id=your_user;Password=your_password;TrustServerCertificate=True;
```

### For Azure SQL Database
```
Server=tcp:yourserver.database.windows.net,1433;Database=DynamicFormsEditor;User Id=your_user;Password=your_password;Encrypt=True;TrustServerCertificate=False;
```

## Configuration Settings

The database includes default configuration. You can modify these values:

### Auto-Save Interval
```sql
UPDATE EditorConfiguration
SET ConfigValue = '60'  -- Change to 60 seconds
WHERE ConfigKey = 'AutoSave.IntervalSeconds';
```

### Undo/Redo Limit
```sql
UPDATE EditorConfiguration
SET ConfigValue = '200'  -- Change to 200 actions
WHERE ConfigKey = 'UndoRedo.MaxActions';
```

### History Retention
```sql
UPDATE EditorConfiguration
SET ConfigValue = '30'  -- Change to 30 days
WHERE ConfigKey = 'History.RetentionDays';
```

## Maintenance

### Clean Up Old History Snapshots

Run this periodically (e.g., daily scheduled job) to remove old history:

```sql
USE DynamicFormsEditor;

DECLARE @RetentionDays INT;

-- Get retention period from configuration
SELECT @RetentionDays = CAST(ConfigValue AS INT)
FROM EditorConfiguration
WHERE ConfigKey = 'History.RetentionDays';

-- Delete old snapshots
DELETE FROM EditorHistory
WHERE CreatedAt < DATEADD(DAY, -@RetentionDays, GETUTCDATE());

PRINT 'Cleaned up history older than ' + CAST(@RetentionDays AS VARCHAR(10)) + ' days.';
```

### Backup Database

```bash
# Using sqlcmd
sqlcmd -S localhost -Q "BACKUP DATABASE DynamicFormsEditor TO DISK = 'C:\Backups\DynamicFormsEditor.bak'"
```

Or in SQL Server Management Studio:
1. Right-click on `DynamicFormsEditor` database
2. Tasks → Back Up...
3. Choose backup type and destination
4. Click OK

### Restore Database

```bash
# Using sqlcmd
sqlcmd -S localhost -Q "RESTORE DATABASE DynamicFormsEditor FROM DISK = 'C:\Backups\DynamicFormsEditor.bak' WITH REPLACE"
```

## Common Issues

### Issue: "Database already exists" error when re-running

**Solution**: This is normal. The script checks and skips if database exists.

### Issue: "Cannot open database" error

**Solution**: Ensure you have permissions on the SQL Server instance.

### Issue: LocalDB not found

**Solution**: Install SQL Server LocalDB:
```bash
# Download from: https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb
# Or install via Visual Studio Installer → Individual Components → SQL Server Express LocalDB
```

### Issue: Tables not created

**Solution**: Check for error messages in the output. Ensure you have CREATE TABLE permissions.

## Next Steps

After database setup:

1. **Configure Entity Framework** (Prompt 1.2)
   - Create `DynamicForms.Editor.Data` project
   - Add DbContext and entity classes
   - Configure connection string

2. **Create Migrations** (Prompt 1.3)
   - Run `dotnet ef migrations add InitialEditorDatabase`
   - Run `dotnet ef database update`

3. **Implement Repositories** (Prompt 1.4-1.5)
   - Create repository interfaces
   - Implement data access layer

## Resources

- [SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [SQL Server Management Studio](https://aka.ms/ssmsfullsetup)
- [Azure Data Studio](https://aka.ms/azuredatastudio)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

## Database Diagram

```
┌──────────────────────┐
│ EditorFormModules    │
│ (Draft Forms)        │
└───────┬──────────────┘
        │
        ├──> EditorHistory (snapshots for undo/redo)
        │
        └──> PublishedFormModules (published versions)


┌──────────────────────┐
│ EditorWorkflows      │
│ (Draft Workflows)    │
└───────┬──────────────┘
        │
        ├──> EditorHistory (snapshots for undo/redo)
        │
        └──> PublishedWorkflows (published versions)


┌──────────────────────┐
│ EditorConfiguration  │
│ (App Settings)       │
└──────────────────────┘
```

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review error messages in SQL Server logs
3. Verify permissions and connectivity
4. Check SQL Server version compatibility (2019+ recommended)
