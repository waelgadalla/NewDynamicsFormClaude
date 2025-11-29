namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Represents a snapshot of the editor state at a specific point in time.
/// Used by the undo/redo system to restore previous states.
/// </summary>
/// <param name="SessionId">The editor session ID when this snapshot was taken</param>
/// <param name="EntityType">The type of entity (Module or Workflow)</param>
/// <param name="SnapshotJson">The serialized schema JSON</param>
/// <param name="ActionDescription">Description of the action that created this snapshot</param>
/// <param name="Timestamp">When this snapshot was created (UTC)</param>
/// <param name="SequenceNumber">Sequential number for ordering snapshots</param>
public record EditorSnapshot(
    Guid SessionId,
    EditorEntityType EntityType,
    string SnapshotJson,
    string ActionDescription,
    DateTime Timestamp,
    int SequenceNumber
);
