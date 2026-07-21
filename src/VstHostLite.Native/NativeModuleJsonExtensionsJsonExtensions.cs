using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="NativeModuleJsonExtensions"/>.
/// </summary>
public static class NativeModuleJsonExtensionsJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options configured for camelCase property names.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes metadata about the <see cref="NativeModuleJsonExtensions"/> type to a JSON string.
    /// </summary>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON string containing type information.</returns>
    public static string ToJson(bool indented = false)
    {
        var data = new { Type = nameof(NativeModuleJsonExtensions) };
        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;
        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Deserializes a JSON string to verify it represents the <see cref="NativeModuleJsonExtensions"/> type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A marker object if the JSON represents the correct type; otherwise <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        var data = JsonSerializer.Deserialize<TypeMarker>(json.Trim(), _options);
        return data?.Type == nameof(NativeModuleJsonExtensions) ? new object() : null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to verify it represents the <see cref="NativeModuleJsonExtensions"/> type.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives a marker object if successful.</param>
    /// <returns><c>true</c> if deserialization succeeded and represents the correct type; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c>, empty, or whitespace.</exception>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var data = JsonSerializer.Deserialize<TypeMarker>(json.Trim(), _options);
            value = data?.Type == nameof(NativeModuleJsonExtensions) ? new object() : null;
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
