using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization and deserialization helpers for VST3 interop JSON extensions type information.
/// </summary>
public static class Vst3InteropJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the VST3 interop JSON extensions type information as a JSON string.
    /// </summary>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation containing the type information.</returns>
    public static string ToJson(bool indented = false)
    {
        var data = new { Type = nameof(Vst3InteropJsonExtensions) };
        return JsonSerializer.Serialize(data, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a JSON string to verify it represents the VST3 interop JSON extensions type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A marker object indicating the type, or <see langword="null"/> if the JSON is empty or whitespace or doesn't match.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        var data = JsonSerializer.Deserialize<TypeMarker>(json.Trim(), _jsonOptions);
        return data?.Type == nameof(Vst3InteropJsonExtensions) ? new object() : null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to verify it represents the VST3 interop JSON extensions type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives a marker object if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded and represents the correct type; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var data = JsonSerializer.Deserialize<TypeMarker>(json.Trim(), _jsonOptions);
            value = data?.Type == nameof(Vst3InteropJsonExtensions) ? new object() : null;
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    private sealed class TypeMarker
    {
        public string? Type { get; set; }
    }
}