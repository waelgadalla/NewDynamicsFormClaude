-- ============================================================================
-- Seed Default Configuration Data
-- ============================================================================
-- This script inserts default configuration values into EditorConfiguration
-- It's safe to run multiple times (uses IF NOT EXISTS checks)
--
-- Run this after applying the InitialEditorDatabase migration
-- ============================================================================

USE DynamicFormsEditor;
GO

-- Auto-Save Interval (in seconds)
IF NOT EXISTS (SELECT 1 FROM EditorConfiguration WHERE ConfigKey = 'AutoSave.IntervalSeconds')
BEGIN
    INSERT INTO EditorConfiguration (ConfigKey, ConfigValue, ConfigType, Description, ModifiedAt)
    VALUES ('AutoSave.IntervalSeconds', '30', 'Int', 'Auto-save interval in seconds. Set to 0 to disable auto-save.', GETUTCDATE());
    PRINT 'Inserted: AutoSave.IntervalSeconds = 30';
END
ELSE
BEGIN
    PRINT 'Configuration already exists: AutoSave.IntervalSeconds';
END

-- Undo/Redo Maximum Actions
IF NOT EXISTS (SELECT 1 FROM EditorConfiguration WHERE ConfigKey = 'UndoRedo.MaxActions')
BEGIN
    INSERT INTO EditorConfiguration (ConfigKey, ConfigValue, ConfigType, Description, ModifiedAt)
    VALUES ('UndoRedo.MaxActions', '100', 'Int', 'Maximum number of undo/redo actions to store per session.', GETUTCDATE());
    PRINT 'Inserted: UndoRedo.MaxActions = 100';
END
ELSE
BEGIN
    PRINT 'Configuration already exists: UndoRedo.MaxActions';
END

-- History Retention Days
IF NOT EXISTS (SELECT 1 FROM EditorConfiguration WHERE ConfigKey = 'History.RetentionDays')
BEGIN
    INSERT INTO EditorConfiguration (ConfigKey, ConfigValue, ConfigType, Description, ModifiedAt)
    VALUES ('History.RetentionDays', '90', 'Int', 'Number of days to keep history snapshots before automatic cleanup.', GETUTCDATE());
    PRINT 'Inserted: History.RetentionDays = 90';
END
ELSE
BEGIN
    PRINT 'Configuration already exists: History.RetentionDays';
END

-- Verify configuration
SELECT
    ConfigKey,
    ConfigValue,
    ConfigType,
    Description
FROM EditorConfiguration
ORDER BY ConfigKey;

PRINT 'Default configuration seed completed.';
GO
