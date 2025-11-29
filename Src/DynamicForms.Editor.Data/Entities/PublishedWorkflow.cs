using System;

namespace DynamicForms.Editor.Data.Entities;

/// <summary>
/// Represents a published workflow (production-ready version).
/// Production applications read ONLY from this table.
/// </summary>
public class PublishedWorkflow
{
    /// <summary>
    /// Primary key (auto-generated)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Business identifier for the workflow (same across all versions)
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
    /// Full FormWorkflowSchema serialized as JSON
    /// </summary>
    public string SchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Version number (increments with each publish)
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// When this version was published
    /// </summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Who published this version (future: username when auth added)
    /// </summary>
    public string? PublishedBy { get; set; }

    /// <summary>
    /// Whether this is the currently active version.
    /// Only one version per WorkflowId should have IsActive = true.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
