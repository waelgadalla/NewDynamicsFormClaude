-- ============================================================================
-- Database Verification Script
-- ============================================================================
-- Run this script to verify the database was created correctly
-- ============================================================================

USE DynamicFormsEditor;
GO

PRINT '========================================';
PRINT 'DATABASE VERIFICATION';
PRINT '========================================';
PRINT '';

-- Check all tables exist
PRINT 'Tables:';
SELECT TABLE_NAME,
       (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = t.TABLE_NAME) AS ColumnCount
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Indexes:';
SELECT
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique
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

PRINT '';
PRINT 'Configuration Data:';
SELECT ConfigKey, ConfigValue, ConfigType, Description
FROM EditorConfiguration
ORDER BY ConfigKey;

PRINT '';
PRINT 'Row Counts:';
SELECT 'EditorFormModules' AS TableName, COUNT(*) AS RowCount FROM EditorFormModules
UNION ALL
SELECT 'EditorWorkflows', COUNT(*) FROM EditorWorkflows
UNION ALL
SELECT 'EditorHistory', COUNT(*) FROM EditorHistory
UNION ALL
SELECT 'PublishedFormModules', COUNT(*) FROM PublishedFormModules
UNION ALL
SELECT 'PublishedWorkflows', COUNT(*) FROM PublishedWorkflows
UNION ALL
SELECT 'EditorConfiguration', COUNT(*) FROM EditorConfiguration;

PRINT '';
PRINT '========================================';
PRINT 'VERIFICATION COMPLETE';
PRINT '========================================';
GO
