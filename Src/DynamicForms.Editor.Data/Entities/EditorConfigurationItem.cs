using System;

namespace DynamicForms.Editor.Data.Entities;

/// <summary>
/// Represents a configuration setting for the editor application.
/// Allows runtime configuration without code changes.
/// </summary>
public class EditorConfigurationItem
{
    /// <summary>
    /// Primary key (auto-generated)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Configuration key (unique identifier)
    /// Example: "AutoSave.IntervalSeconds", "UndoRedo.MaxActions"
    /// </summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>
    /// Configuration value (stored as string, parsed based on ConfigType)
    /// </summary>
    public string ConfigValue { get; set; } = string.Empty;

    /// <summary>
    /// Data type of the value: "Int", "String", "Bool", "Decimal"
    /// Used for type-safe retrieval
    /// </summary>
    public string ConfigType { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of this configuration setting
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When this configuration was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}
