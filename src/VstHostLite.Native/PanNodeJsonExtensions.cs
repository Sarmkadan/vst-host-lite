using System.Text.Json;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="PanNode"/> objects.
/// </summary>
public static class PanNodeJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a <see cref="PanNode"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The pan node to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the pan node</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static string ToJson(this PanNode value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="PanNode"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A deserialized pan node instance, or null if the JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized</exception>
    public static PanNode? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<PanNodeJsonModel>(json, _jsonOptions)?.ToPanNode();
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="PanNode"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized pan node if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out PanNode? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Internal model used for JSON serialization/deserialization.
    /// </summary>
    private sealed class PanNodeJsonModel
    {
        public string? Name { get; set; }
        public float Pan { get; set; }
        public int Frames { get; set; }

        public PanNode ToPanNode()
        {
            if (Name is null)
            {
                throw new JsonException("Name cannot be null");
            }

            return new PanNode(Name, Frames)
            {
                Pan = Pan,
            };
        }
    }
}