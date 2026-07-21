using System;
using System.Text.Json;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ClipDetectionResult"/> objects.
/// </summary>
public static class ClipDetectorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a <see cref="ClipDetectionResult"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The clip detection result to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the clip detection result</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static string ToJson(this ClipDetectionResult value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ClipDetectionResult"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A deserialized clip detection result instance, or null if the JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized</exception>
    public static ClipDetectionResult? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ClipDetectionResult>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ClipDetectionResult"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized clip detection result if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out ClipDetectionResult? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = FromJson(json);
            return value != null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}