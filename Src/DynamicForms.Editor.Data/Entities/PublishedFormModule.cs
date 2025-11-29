using System;

namespace DynamicForms.Editor.Data.Entities;

/// <summary>
/// Represents a published form module (production-ready version).
/// Production applications read ONLY from this table.
/// </summary>
public class PublishedFormModule
{
    /// <summary>
    /// Primary key (auto-generated)
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Business identifier for the module (same across all versions)
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
    /// Full FormModuleSchema serialized as JSON
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
    /// Only one version per ModuleId should have IsActive = true.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
