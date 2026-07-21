using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="AudioGraphValidation"/> results.
/// </summary>
public static class AudioGraphValidationJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options configured for camelCase property names.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an <see cref="AudioGraphValidation"/> result to a JSON string.
    /// </summary>
    /// <param name="value">The validation result to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON representation of the validation result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this IReadOnlyList<string> value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AudioGraphValidation"/> result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A list of validation problems; empty if the JSON represents a valid graph.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static IReadOnlyList<string> FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<List<string>>(json, _options)
            ?? throw new JsonException("Deserialized validation result list is null.");
    }

    /// <summary>
    /// Tries to deserialize a JSON string into an <see cref="AudioGraphValidation"/> result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized validation result if successful; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out IReadOnlyList<string>? value)
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
}