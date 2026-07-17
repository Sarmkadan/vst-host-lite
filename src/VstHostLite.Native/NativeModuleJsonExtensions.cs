using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization helpers for <see cref="NativeModule"/>.
/// </summary>
public static class NativeModuleJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options configured for camelCase property names.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="NativeModule"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The module to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON representation of the module.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this NativeModule value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented ? new JsonSerializerOptions(_options) { WriteIndented = true } : _options;
        // Serialize only the Path property; other fields are internal.
        return JsonSerializer.Serialize(new { value.Path }, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a <see cref="NativeModule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="NativeModule"/> instance, or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or missing required data.</exception>
    public static NativeModule? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        var dto = JsonSerializer.Deserialize<ModuleDto>(json, _options)
                  ?? throw new JsonException("Deserialized object is null.");
        return NativeModule.Load(dto.Path);
    }

    /// <summary>
    /// Tries to deserialize a JSON string into a <see cref="NativeModule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized module if successful; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out NativeModule? value)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// DTO used for JSON (de)serialization.
    /// </summary>
    private sealed record ModuleDto(string Path);
}
