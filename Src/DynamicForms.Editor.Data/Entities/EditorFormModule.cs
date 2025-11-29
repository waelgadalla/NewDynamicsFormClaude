using System;

namespace DynamicForms.Editor.Data.Entities;

/// <summary>
/// Represents a form module being edited (draft version).
/// Stores the working copy of form schemas that may not yet be ready for production.
/// </summary>
public class EditorFormModule
{
    /// <summary>
    /// Primary key (auto-generated)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Business identifier for the module (not unique - multiple drafts/versions possible)
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// Module title (English)
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Module title (French)
    /// </summary>
    public string? TitleFr { get; set; }

    /// <summary>
    /// Module description (English)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Module description (French)
    /// </summary>
    public string? DescriptionFr { get; set; }

    /// <summary>
    /// Full FormModuleSchema serialized as JSON
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
