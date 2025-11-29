namespace DynamicForms.Editor.Services.State;

/// <summary>
/// Represents the type of entity being edited in the form editor.
/// </summary>
public enum EditorEntityType
{
    /// <summary>
    /// Editing a form module (form definition with fields).
    /// </summary>
    Module,

    /// <summary>
    /// Editing a workflow (process flow with stages and transitions).
    /// </summary>
    Workflow
}
