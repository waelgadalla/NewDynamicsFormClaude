-- ============================================================================
-- DynamicForms Visual Editor Database Schema
-- ============================================================================
-- Purpose: Creates the complete database schema for the DynamicForms Visual
--          Form Editor, including editor tables for drafts/working versions
--          and published tables for production-ready forms.
--
-- Version: 1.0
-- Created: January 2025
-- Target: SQL Server 2019+
--
-- This script is IDEMPOTENT - safe to run multiple times.
-- It will:
--   - Create the database if it doesn't exist
--   - Create tables only if they don't exist
--   - Insert configuration data only if not already present
--
-- Usage:
--   Run this script in SQL Server Management Studio or via sqlcmd:
--   sqlcmd -S localhost -i CreateEditorDatabase.sql
-- ============================================================================

-- ============================================================================
-- SECTION 1: DATABASE CREATION
-- ============================================================================

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DynamicFormsEditor')
BEGIN
    CREATE DATABASE DynamicFormsEditor;
    PRINT 'Database [DynamicFormsEditor] created successfully.';
END
ELSE
BEGIN
    PRINT 'Database [DynamicFormsEditor] already exists.';
END
GO

-- Switch to the database
USE DynamicFormsEditor;
GO

PRINT '========================================';
PRINT 'Using database: DynamicFormsEditor';
PRINT '========================================';
GO

-- ============================================================================
-- SECTION 2: EDITOR TABLES (Working/Draft Data)
-- ============================================================================
-- These tables store forms and workflows that are currently being edited.
-- They represent "work in progress" and may not be ready for production.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- TABLE: EditorFormModules
-- ----------------------------------------------------------------------------
-- Purpose: Stores form modules being edited (draft versions)
--
-- Key Features:
--   - One row per module being edited
--   - Full FormModuleSchema stored as JSON
--   - Tracks status (Draft, Published, Archived)
--   - Supports versioning
--   - Tracks creation and modification timestamps
--
-- Usage: Editor UI loads/saves draft modules from this table
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EditorFormModules]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[EditorFormModules] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ModuleId] INT NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [TitleFr] NVARCHAR(500) NULL,
        [Description] NVARCHAR(MAX) NULL,
        [DescriptionFr] NVARCHAR(MAX) NULL,
        [SchemaJson] NVARCHAR(MAX) NOT NULL,         -- Full FormModuleSchema serialized as JSON
        [Version] INT NOT NULL DEFAULT 1,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',  -- Draft, Published, Archived
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedBy] NVARCHAR(256) NULL              -- Future: username when auth added
    );

    -- Indexes for performance
    CREATE NONCLUSTERED INDEX IX_EditorFormModules_ModuleId
        ON [dbo].[EditorFormModules]([ModuleId]);

    CREATE NONCLUSTERED INDEX IX_EditorFormModules_Status
        ON [dbo].[EditorFormModules]([Status]);

    CREATE NONCLUSTERED INDEX IX_EditorFormModules_ModifiedAt
        ON [dbo].[EditorFormModules]([ModifiedAt] DESC);

    PRINT 'Table [EditorFormModules] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [EditorFormModules] already exists.';
END
GO

-- ----------------------------------------------------------------------------
-- TABLE: EditorWorkflows
-- ----------------------------------------------------------------------------
-- Purpose: Stores multi-module workflows being edited
--
-- Key Features:
--   - Combines multiple form modules into workflows
--   - Supports conditional branching between modules
--   - Full FormWorkflowSchema stored as JSON
--   - Tracks workflow status and versions
--
-- Usage: Workflow editor loads/saves from this table
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EditorWorkflows]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[EditorWorkflows] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [WorkflowId] INT NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [TitleFr] NVARCHAR(500) NULL,
        [Description] NVARCHAR(MAX) NULL,
        [SchemaJson] NVARCHAR(MAX) NOT NULL,         -- Full FormWorkflowSchema as JSON
        [Version] INT NOT NULL DEFAULT 1,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [ModifiedBy] NVARCHAR(256) NULL
    );

    -- Indexes for performance
    CREATE NONCLUSTERED INDEX IX_EditorWorkflows_WorkflowId
        ON [dbo].[EditorWorkflows]([WorkflowId]);

    CREATE NONCLUSTERED INDEX IX_EditorWorkflows_Status
        ON [dbo].[EditorWorkflows]([Status]);

    PRINT 'Table [EditorWorkflows] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [EditorWorkflows] already exists.';
END
GO

-- ----------------------------------------------------------------------------
-- TABLE: EditorHistory
-- ----------------------------------------------------------------------------
-- Purpose: Stores undo/redo history snapshots for editor sessions
--
-- Key Features:
--   - Captures full state snapshots for undo/redo functionality
--   - Groups snapshots by EditorSessionId (one editing session)
--   - Sequence numbers track order of changes
--   - Stores descriptive action text for UI display
--   - Includes cleanup mechanism (old snapshots can be purged)
--
-- Usage: UndoRedoService pushes/pops snapshots from this table
-- Cleanup: Rows older than configured retention period can be deleted
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EditorHistory]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[EditorHistory] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,       -- Use BIGINT for potentially large history
        [EditorSessionId] UNIQUEIDENTIFIER NOT NULL, -- Groups all edits in one editing session
        [EntityType] NVARCHAR(50) NOT NULL,          -- 'Module' or 'Workflow'
        [EntityId] INT NOT NULL,                     -- ModuleId or WorkflowId
        [SnapshotJson] NVARCHAR(MAX) NOT NULL,       -- Full snapshot of entity state
        [ActionDescription] NVARCHAR(500) NULL,      -- e.g., "Added field 'Email'", "Deleted section"
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [SequenceNumber] INT NOT NULL                -- Order within session (1, 2, 3...)
    );

    -- Indexes for efficient undo/redo operations
    CREATE NONCLUSTERED INDEX IX_EditorHistory_EditorSessionId
        ON [dbo].[EditorHistory]([EditorSessionId], [SequenceNumber] DESC);

    CREATE NONCLUSTERED INDEX IX_EditorHistory_EntityTypeId
        ON [dbo].[EditorHistory]([EntityType], [EntityId]);

    CREATE NONCLUSTERED INDEX IX_EditorHistory_CreatedAt
        ON [dbo].[EditorHistory]([CreatedAt]);  -- For cleanup queries

    PRINT 'Table [EditorHistory] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [EditorHistory] already exists.';
END
GO

-- ============================================================================
-- SECTION 3: PRODUCTION TABLES (Published Data)
-- ============================================================================
-- These tables store forms and workflows that have been published and are
-- ready for use in production applications. They are READ-ONLY for
-- production apps.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- TABLE: PublishedFormModules
-- ----------------------------------------------------------------------------
-- Purpose: Stores published form modules (production-ready versions)
--
-- Key Features:
--   - Only published/approved forms are in this table
--   - Supports multiple versions of same module
--   - IsActive flag indicates current active version
--   - Version numbers auto-increment on each publish
--   - Tracks who published and when
--
-- Usage: Production apps read ONLY from this table (never write)
-- Query: SELECT TOP 1 * FROM PublishedFormModules
--        WHERE ModuleId = @id AND IsActive = 1
--        ORDER BY Version DESC
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublishedFormModules]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[PublishedFormModules] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ModuleId] INT NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [TitleFr] NVARCHAR(500) NULL,
        [SchemaJson] NVARCHAR(MAX) NOT NULL,         -- Full FormModuleSchema as JSON
        [Version] INT NOT NULL,
        [PublishedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [PublishedBy] NVARCHAR(256) NULL,            -- Username who published
        [IsActive] BIT NOT NULL DEFAULT 1            -- Only one active version per ModuleId
    );

    -- Composite index for efficient lookups (ModuleId + Version descending)
    CREATE NONCLUSTERED INDEX IX_PublishedFormModules_ModuleId_Version
        ON [dbo].[PublishedFormModules]([ModuleId], [Version] DESC);

    CREATE NONCLUSTERED INDEX IX_PublishedFormModules_IsActive
        ON [dbo].[PublishedFormModules]([IsActive])
        WHERE [IsActive] = 1;  -- Filtered index for active records only

    PRINT 'Table [PublishedFormModules] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [PublishedFormModules] already exists.';
END
GO

-- ----------------------------------------------------------------------------
-- TABLE: PublishedWorkflows
-- ----------------------------------------------------------------------------
-- Purpose: Stores published multi-module workflows (production-ready)
--
-- Key Features:
--   - Published workflows combining multiple modules
--   - Version management same as PublishedFormModules
--   - IsActive flag for current production version
--
-- Usage: Production apps read workflow definitions from this table
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublishedWorkflows]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[PublishedWorkflows] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [WorkflowId] INT NOT NULL,
        [Title] NVARCHAR(500) NOT NULL,
        [TitleFr] NVARCHAR(500) NULL,
        [SchemaJson] NVARCHAR(MAX) NOT NULL,         -- Full FormWorkflowSchema as JSON
        [Version] INT NOT NULL,
        [PublishedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [PublishedBy] NVARCHAR(256) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1
    );

    -- Composite index for efficient lookups
    CREATE NONCLUSTERED INDEX IX_PublishedWorkflows_WorkflowId_Version
        ON [dbo].[PublishedWorkflows]([WorkflowId], [Version] DESC);

    CREATE NONCLUSTERED INDEX IX_PublishedWorkflows_IsActive
        ON [dbo].[PublishedWorkflows]([IsActive])
        WHERE [IsActive] = 1;

    PRINT 'Table [PublishedWorkflows] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [PublishedWorkflows] already exists.';
END
GO

-- ============================================================================
-- SECTION 4: CONFIGURATION TABLE
-- ============================================================================
-- Stores application configuration settings that can be modified at runtime
-- without code changes.
-- ============================================================================

-- ----------------------------------------------------------------------------
-- TABLE: EditorConfiguration
-- ----------------------------------------------------------------------------
-- Purpose: Stores editor configuration settings (auto-save interval, etc.)
--
-- Key Features:
--   - Key-value pairs for configuration
--   - Type information for type-safe retrieval
--   - Unique constraint on ConfigKey
--   - Tracks when configuration was last modified
--
-- Usage: Application reads configuration on startup or periodically
-- Examples:
--   - AutoSave.IntervalSeconds = 30
--   - UndoRedo.MaxActions = 100
--   - History.RetentionDays = 90
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EditorConfiguration]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[EditorConfiguration] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ConfigKey] NVARCHAR(100) NOT NULL,
        [ConfigValue] NVARCHAR(500) NOT NULL,
        [ConfigType] NVARCHAR(50) NOT NULL,          -- 'Int', 'String', 'Bool', 'Decimal'
        [Description] NVARCHAR(500) NULL,
        [ModifiedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT UQ_EditorConfiguration_ConfigKey UNIQUE ([ConfigKey])
    );

    -- Index on ConfigKey for fast lookups
    CREATE NONCLUSTERED INDEX IX_EditorConfiguration_ConfigKey
        ON [dbo].[EditorConfiguration]([ConfigKey]);

    PRINT 'Table [EditorConfiguration] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [EditorConfiguration] already exists.';
END
GO

-- ============================================================================
-- SECTION 5: DEFAULT CONFIGURATION DATA
-- ============================================================================
-- Inserts default configuration values if they don't already exist.
-- These can be modified later via the application or directly in the database.
-- ============================================================================

PRINT '========================================';
PRINT 'Inserting default configuration...';
PRINT '========================================';

-- Auto-Save Interval (in seconds)
IF NOT EXISTS (SELECT 1 FROM [dbo].[EditorConfiguration] WHERE [ConfigKey] = 'AutoSave.IntervalSeconds')
BEGIN
    INSERT INTO [dbo].[EditorConfiguration] ([ConfigKey], [ConfigValue], [ConfigType], [Description])
    VALUES ('AutoSave.IntervalSeconds', '30', 'Int', 'Auto-save interval in seconds. Set to 0 to disable auto-save.');
    PRINT 'Inserted: AutoSave.IntervalSeconds = 30';
END
ELSE
BEGIN
    PRINT 'Configuration already exists: AutoSave.IntervalSeconds';
END

-- Undo/Redo Maximum Actions
IF NOT EXISTS (SELECT 1 FROM [dbo].[EditorConfiguration] WHERE [ConfigKey] = 'UndoRedo.MaxActions')
BEGIN
    INSERT INTO [dbo].[EditorConfiguration] ([ConfigKey], [ConfigValue], [ConfigType], [Description])
    VALUES ('UndoRedo.MaxActions', '100', 'Int', 'Maximum number of undo/redo actions to store per session.');
    PRINT 'Inserted: UndoRedo.MaxActions = 100';
END
ELSE
BEGIN
    PRINT 'Configuration already exists: UndoRedo.MaxActions';
END

-- History Retention Days
IF NOT EXISTS (SELECT 1 FROM [dbo].[EditorConfiguration] WHERE [ConfigKey] = 'History.RetentionDays')
BEGIN
    INSERT INTO [dbo].[EditorConfiguration] ([ConfigKey], [ConfigValue], [ConfigType], [Description])
    VALUES ('History.RetentionDays', '90', 'Int', 'Number of days to keep history snapshots before automatic cleanup.');
    PRINT 'Inserted: History.RetentionDays = 90';
END
ELSE
BEGIN
    PRINT 'Configuration already exists: History.RetentionDays';
END
GO

-- ============================================================================
-- SECTION 6: VERIFICATION & SUMMARY
-- ============================================================================

PRINT '';
PRINT '========================================';
PRINT 'DATABASE SCHEMA CREATION COMPLETE';
PRINT '========================================';
PRINT '';

-- Display table counts
PRINT 'Tables created:';
SELECT
    TABLE_NAME,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
    AND TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME IN (
        'EditorFormModules',
        'EditorWorkflows',
        'EditorHistory',
        'PublishedFormModules',
        'PublishedWorkflows',
        'EditorConfiguration'
    )
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Indexes created:';
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
    AND i.name IS NOT NULL  -- Exclude heap
ORDER BY TableName, IndexName;

PRINT '';
PRINT 'Configuration entries:';
SELECT
    ConfigKey,
    ConfigValue,
    ConfigType,
    Description
FROM [dbo].[EditorConfiguration]
ORDER BY ConfigKey;

PRINT '';
PRINT '========================================';
PRINT 'NEXT STEPS:';
PRINT '1. Verify tables created successfully above';
PRINT '2. Run Entity Framework migrations (see README.md)';
PRINT '3. Configure connection string in appsettings.json';
PRINT '4. Start implementing repositories (Prompt 1.4)';
PRINT '========================================';
GO

-- ============================================================================
-- END OF SCRIPT
-- ============================================================================
