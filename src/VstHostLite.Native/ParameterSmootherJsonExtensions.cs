using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ParameterSmoother"/>.
/// </summary>
public static class ParameterSmootherJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private static readonly JsonSerializerOptions _jsonOptionsIndented = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the <see cref="ParameterSmoother"/> to a JSON string.
    /// </summary>
    /// <param name="value">The parameter smoother to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the parameter smoother.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ParameterSmoother value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? _jsonOptionsIndented : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a <see cref="ParameterSmoother"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized parameter smoother, or null if the JSON is null or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ParameterSmoother? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ParameterSmoother>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ParameterSmoother"/> from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized parameter smoother if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ParameterSmoother? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ParameterSmoother>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}