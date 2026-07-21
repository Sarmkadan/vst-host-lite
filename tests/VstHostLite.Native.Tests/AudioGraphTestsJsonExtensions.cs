using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Provides JSON serialization helpers for <see cref="AudioGraphTests"/> test fixture.
/// </summary>
public static class AudioGraphTestsJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options configured for camelCase property names.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an <see cref="AudioGraphTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The audio graph tests instance to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON representation of the audio graph tests instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this AudioGraphTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AudioGraphTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="AudioGraphTests"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or missing required data.</exception>
    public static AudioGraphTests? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<AudioGraphTests>(json, _options);
    }

    /// <summary>
    /// Tries to deserialize a JSON string into an <see cref="AudioGraphTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized audio graph tests instance if successful; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out AudioGraphTests? value)
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