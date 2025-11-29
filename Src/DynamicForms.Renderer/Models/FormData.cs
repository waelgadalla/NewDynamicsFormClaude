using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicForms.Renderer.Models;

/// <summary>
/// Stores user input data for a dynamic form.
/// Provides type-safe access to field values with automatic type conversion.
/// Supports JSON serialization for persistence and transport.
/// </summary>
public class FormData
{
    private readonly Dictionary<string, object?> _values;

    /// <summary>
    /// Initializes a new instance of the FormData class.
    /// </summary>
    public FormData()
    {
        _values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the FormData class with existing values.
    /// </summary>
    /// <param name="values">Initial field values</param>
    public FormData(Dictionary<string, object?> values)
    {
        _values = new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a field value with type conversion.
    /// Returns the default value of T if the field doesn't exist or conversion fails.
    /// </summary>
    /// <typeparam name="T">The expected type of the value</typeparam>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>The field value as type T, or default(T) if not found or conversion fails</returns>
    public T? GetValue<T>(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return default;

        if (!_values.TryGetValue(fieldId, out var value))
            return default;

        if (value == null)
            return default;

        // If value is already of type T, return it directly
        if (value is T typedValue)
            return typedValue;

        // Handle JsonElement (from deserialization)
        if (value is JsonElement jsonElement)
        {
            return ConvertFromJsonElement<T>(jsonElement);
        }

        // Try to convert the value to type T
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            // Conversion failed, return default
            return default;
        }
    }

    /// <summary>
    /// Gets a field value as an object without type conversion.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>The field value, or null if not found</returns>
    public object? GetValue(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return null;

        _values.TryGetValue(fieldId, out var value);
        return value;
    }

    /// <summary>
    /// Sets a field value.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <param name="value">The value to set</param>
    public void SetValue(string fieldId, object? value)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

        _values[fieldId] = value;
    }

    /// <summary>
    /// Gets all field values as a dictionary.
    /// Returns a copy to prevent external modification.
    /// </summary>
    /// <returns>A dictionary containing all field values</returns>
    public Dictionary<string, object?> GetAllValues()
    {
        return new Dictionary<string, object?>(_values, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a field value exists.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>True if the field has a value; otherwise false</returns>
    public bool ContainsField(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return false;

        return _values.ContainsKey(fieldId);
    }

    /// <summary>
    /// Removes a field value.
    /// </summary>
    /// <param name="fieldId">The field identifier</param>
    /// <returns>True if the field was removed; false if it didn't exist</returns>
    public bool RemoveField(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId))
            return false;

        return _values.Remove(fieldId);
    }

    /// <summary>
    /// Clears all field values.
    /// </summary>
    public void Clear()
    {
        _values.Clear();
    }

    /// <summary>
    /// Gets the number of fields with values.
    /// </summary>
    public int Count => _values.Count;

    /// <summary>
    /// Serializes the form data to a JSON string.
    /// </summary>
    /// <returns>JSON representation of the form data</returns>
    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(_values, options);
    }

    /// <summary>
    /// Deserializes form data from a JSON string.
    /// </summary>
    /// <param name="json">JSON string containing form data</param>
    /// <returns>A new FormData instance with the deserialized values</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid</exception>
    public static FormData FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new FormData();

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);
        return new FormData(values ?? new Dictionary<string, object?>());
    }

    /// <summary>
    /// Merges values from another FormData instance.
    /// Existing values are overwritten.
    /// </summary>
    /// <param name="other">The FormData to merge from</param>
    public void Merge(FormData other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        foreach (var kvp in other._values)
        {
            _values[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Creates a deep copy of this FormData instance.
    /// </summary>
    /// <returns>A new FormData instance with copied values</returns>
    public FormData Clone()
    {
        var json = ToJson();
        return FromJson(json);
    }

    /// <summary>
    /// Converts a JsonElement to the specified type.
    /// </summary>
    private static T? ConvertFromJsonElement<T>(JsonElement jsonElement)
    {
        try
        {
            // Handle specific types
            if (typeof(T) == typeof(string))
                return (T)(object)jsonElement.GetString()!;

            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                return (T)(object)jsonElement.GetInt32();

            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                return (T)(object)jsonElement.GetInt64();

            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                return (T)(object)jsonElement.GetDecimal();

            if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                return (T)(object)jsonElement.GetDouble();

            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
                return (T)(object)jsonElement.GetBoolean();

            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                return (T)(object)jsonElement.GetDateTime();

            if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
                return (T)(object)jsonElement.GetGuid();

            // For other types, deserialize from the JSON element
            return jsonElement.Deserialize<T>();
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Gets a string representation of the form data (JSON format).
    /// </summary>
    public override string ToString()
    {
        return ToJson();
    }
}
