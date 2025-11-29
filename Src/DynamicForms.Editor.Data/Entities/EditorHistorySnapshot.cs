using System;

namespace DynamicForms.Editor.Data.Entities;

/// <summary>
/// Represents a snapshot of editor state for undo/redo functionality.
/// Each edit operation creates a new snapshot that can be restored.
/// </summary>
public class EditorHistorySnapshot
{
    /// <summary>
    /// Primary key (auto-generated, BIGINT for potentially large history)
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Groups all snapshots from a single editing session
    /// </summary>
    public Guid EditorSessionId { get; set; }

    /// <summary>
    /// Type of entity: "Module" or "Workflow"
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the module or workflow being edited
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// Full snapshot of entity state as JSON
    /// </summary>
    public string SnapshotJson { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the action that created this snapshot
    /// Example: "Added field 'Email'", "Deleted section 'Contact Info'"
    /// </summary>
    public string? ActionDescription { get; set; }

    /// <summary>
    /// When this snapshot was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Order within the editing session (1, 2, 3...)
    /// Used to maintain chronological order for undo/redo
    /// </summary>
    public int SequenceNumber { get; set; }
}
