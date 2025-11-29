using DynamicForms.Core.V2.Runtime;

namespace DynamicForms.Renderer.Models;

/// <summary>
/// Maintains runtime state for form rendering.
/// Tracks field visibility, enabled state, and provides access to form hierarchy and user data.
/// </summary>
public class RenderContext
{
    /// <summary>
    /// Gets the form module runtime hierarchy.
    /// Contains the complete tree structure of form fields.
    /// </summary>
    public FormModuleRuntime Runtime { get; }

    /// <summary>
    /// Gets the form data containing user input values.
    /// </summary>
    public FormData FormData { get; }

    /// <summary>
    /// Tracks the visibility state of each field.
    /// Key: Field ID, Value: true if visible, false if hidden.
    /// </summary>
    private readonly Dictionary<string, bool> _visibilityState;

    /// <summary>
    /// Tracks the enabled state of each field.
    /// Key: Field ID, Value: true if enabled, false if disabled.
    /// </summary>
    private readonly Dictionary<string, bool> _enabledState;

    /// <summary>
    /// Initializes a new instance of the RenderContext class.
    /// </summary>
    /// <param name="runtime">The form module runtime hierarchy</param>
    /// <param name="formData">The form data (optional, creates new instance if null)</param>
    public RenderContext(FormModuleRuntime runtime, FormData? formData = null)
    {
        Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        FormData = formData ?? new FormData();
        _visibilityState = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        _enabledState = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // Initialize all fields as visible and enabled by default
        InitializeFieldStates();
    }

    /// <summary>
    /// Checks if a field should be visible.
    /// Returns true if no visibility state is set (default is visible).
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>True if the field should be visible; otherwise false</returns>
    public bool IsFieldVisible(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return false;

        // Default to visible if not explicitly set
        return !_visibilityState.TryGetValue(fieldId, out var isVisible) || isVisible;
    }

    /// <summary>
    /// Checks if a field should be enabled.
    /// Returns true if no enabled state is set (default is enabled).
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>True if the field should be enabled; otherwise false</returns>
    public bool IsFieldEnabled(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return false;

        // Default to enabled if not explicitly set
        return !_enabledState.TryGetValue(fieldId, out var isEnabled) || isEnabled;
    }

    /// <summary>
    /// Sets the visibility state of a field.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <param name="visible">True to make the field visible; false to hide it</param>
    public void SetFieldVisibility(string fieldId, bool visible)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return;

        _visibilityState[fieldId] = visible;
    }

    /// <summary>
    /// Sets the enabled state of a field.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <param name="enabled">True to enable the field; false to disable it</param>
    public void SetFieldEnabled(string fieldId, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return;

        _enabledState[fieldId] = enabled;
    }

    /// <summary>
    /// Updates visibility for multiple fields at once.
    /// Useful after evaluating conditional logic.
    /// </summary>
    /// <param name="visibilityStates">Dictionary of field IDs and their visibility states</param>
    public void UpdateVisibilityStates(Dictionary<string, bool> visibilityStates)
    {
        if (visibilityStates == null)
            return;

        foreach (var kvp in visibilityStates)
        {
            SetFieldVisibility(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Updates enabled state for multiple fields at once.
    /// Useful after evaluating conditional logic.
    /// </summary>
    /// <param name="enabledStates">Dictionary of field IDs and their enabled states</param>
    public void UpdateEnabledStates(Dictionary<string, bool> enabledStates)
    {
        if (enabledStates == null)
            return;

        foreach (var kvp in enabledStates)
        {
            SetFieldEnabled(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Gets all visibility states.
    /// Returns a copy to prevent external modification.
    /// </summary>
    /// <returns>Dictionary of field IDs and their visibility states</returns>
    public Dictionary<string, bool> GetAllVisibilityStates()
    {
        return new Dictionary<string, bool>(_visibilityState, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all enabled states.
    /// Returns a copy to prevent external modification.
    /// </summary>
    /// <returns>Dictionary of field IDs and their enabled states</returns>
    public Dictionary<string, bool> GetAllEnabledStates()
    {
        return new Dictionary<string, bool>(_enabledState, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resets all visibility states to default (all visible).
    /// </summary>
    public void ResetVisibilityStates()
    {
        _visibilityState.Clear();
    }

    /// <summary>
    /// Resets all enabled states to default (all enabled).
    /// </summary>
    public void ResetEnabledStates()
    {
        _enabledState.Clear();
    }

    /// <summary>
    /// Gets a field node by its ID from the runtime hierarchy.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>The field node if found; otherwise null</returns>
    public FormFieldNode? GetFieldNode(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return null;

        return FindFieldNodeRecursive(Runtime.RootFields, fieldId);
    }

    /// <summary>
    /// Checks if a field exists in the form hierarchy.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>True if the field exists; otherwise false</returns>
    public bool FieldExists(string fieldId)
    {
        return GetFieldNode(fieldId) != null;
    }

    /// <summary>
    /// Gets the number of visible fields.
    /// </summary>
    /// <returns>Count of fields that are currently visible</returns>
    public int GetVisibleFieldCount()
    {
        return CountFieldsRecursive(Runtime.RootFields, node => IsFieldVisible(node.Schema.Id));
    }

    /// <summary>
    /// Gets the number of enabled fields.
    /// </summary>
    /// <returns>Count of fields that are currently enabled</returns>
    public int GetEnabledFieldCount()
    {
        return CountFieldsRecursive(Runtime.RootFields, node => IsFieldEnabled(node.Schema.Id));
    }

    /// <summary>
    /// Initializes visibility and enabled states for all fields.
    /// Sets all fields to visible and enabled by default.
    /// </summary>
    private void InitializeFieldStates()
    {
        InitializeFieldStatesRecursive(Runtime.RootFields);
    }

    /// <summary>
    /// Recursively initializes field states for all nodes in the hierarchy.
    /// </summary>
    private void InitializeFieldStatesRecursive(List<FormFieldNode> nodes)
    {
        if (nodes == null)
            return;

        foreach (var node in nodes)
        {
            // Initialize as visible and enabled
            _visibilityState[node.Schema.Id] = true;
            _enabledState[node.Schema.Id] = true;

            // Process children recursively
            if (node.Children?.Count > 0)
            {
                InitializeFieldStatesRecursive(node.Children);
            }
        }
    }

    /// <summary>
    /// Recursively searches for a field node by ID.
    /// </summary>
    private static FormFieldNode? FindFieldNodeRecursive(List<FormFieldNode> nodes, string fieldId)
    {
        if (nodes == null)
            return null;

        foreach (var node in nodes)
        {
            if (string.Equals(node.Schema.Id, fieldId, StringComparison.OrdinalIgnoreCase))
                return node;

            if (node.Children?.Count > 0)
            {
                var found = FindFieldNodeRecursive(node.Children, fieldId);
                if (found != null)
                    return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Recursively counts fields matching a predicate.
    /// </summary>
    private static int CountFieldsRecursive(List<FormFieldNode> nodes, Func<FormFieldNode, bool> predicate)
    {
        if (nodes == null)
            return 0;

        var count = 0;
        foreach (var node in nodes)
        {
            if (predicate(node))
                count++;

            if (node.Children?.Count > 0)
            {
                count += CountFieldsRecursive(node.Children, predicate);
            }
        }

        return count;
    }
}
