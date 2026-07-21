using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="NativeModule"/> instances
/// via the <see cref="NativeModuleExtensions"/> helper methods.
/// </summary>
public static class NativeModuleExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes a <see cref="NativeModule"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The native module instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the native module.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this NativeModule value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new NativeModuleJsonModel(value.Path), options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="NativeModule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="NativeModule"/> instance, or <see langword="null"/> if the JSON represents a null value.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static NativeModule? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var model = JsonSerializer.Deserialize<NativeModuleJsonModel>(json, _jsonOptions);
        return model?.ToNativeModule();
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="NativeModule"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized <see cref="NativeModule"/> instance if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
    public static bool TryFromJson(string json, out NativeModule? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            var model = JsonSerializer.Deserialize<NativeModuleJsonModel>(json, _jsonOptions);
            value = model?.ToNativeModule();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Internal model used for JSON serialization/deserialization of <see cref="NativeModule"/>.
    /// </summary>
    private sealed class NativeModuleJsonModel
    {
        public string? Path { get; set; }

        public NativeModuleJsonModel(string path)
        {
            Path = path;
        }

        public NativeModule ToNativeModule()
        {
            if (Path is null)
            {
                throw new JsonException("Path cannot be null");
            }

            return NativeModule.Load(Path);
        }
    }
}