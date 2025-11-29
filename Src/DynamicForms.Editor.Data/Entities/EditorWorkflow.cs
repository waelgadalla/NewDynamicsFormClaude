using System;

namespace DynamicForms.Editor.Data.Entities;

/// <summary>
/// Represents a multi-module workflow being edited (draft version).
/// Combines multiple form modules into a sequential or conditional workflow.
/// </summary>
public class EditorWorkflow
{
    /// <summary>
    /// Primary key (auto-generated)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Business identifier for the workflow (not unique - multiple drafts/versions possible)
    /// </summary>
    public int WorkflowId { get; set; }

    /// <summary>
    /// Workflow title (English)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Workflow title (French)
    /// </summary>
    public string? TitleFr { get; set; }

    /// <summary>
    /// Workflow description (English)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Full FormWorkflowSchema serialized as JSON
    /// </summary>
    public string SchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Version number (increments on each publish)
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Current status: Draft, Published, Archived
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// When this draft was first created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this draft was last modified
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Who last modified this draft (future: username when auth added)
    /// </summary>
    public string? ModifiedBy { get; set; }
}
