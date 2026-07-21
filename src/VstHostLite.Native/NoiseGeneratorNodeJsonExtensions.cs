using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="NoiseGeneratorNode"/>.
/// </summary>
public static class NoiseGeneratorNodeJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    /// <summary>
    /// Serializes a <see cref="NoiseGeneratorNode"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The noise generator node to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the noise generator node</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static string ToJson(this NoiseGeneratorNode value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="NoiseGeneratorNode"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A deserialized <see cref="NoiseGeneratorNode"/> instance, or null if the JSON is empty</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null</exception>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized</exception>
    public static NoiseGeneratorNode? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<NoiseGeneratorNode>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="NoiseGeneratorNode"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized <see cref="NoiseGeneratorNode"/> instance, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeds; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out NoiseGeneratorNode? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<NoiseGeneratorNode>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}