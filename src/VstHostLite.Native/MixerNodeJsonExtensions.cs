using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="MixerNode"/>.
/// </summary>
public static class MixerNodeJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a <see cref="MixerNode"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The mixer node to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the mixer node</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static string ToJson(this MixerNode value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new MixerNodeJsonModel(value), options);
    }

    /// <summary>
    /// Deserializes a <see cref="MixerNode"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A deserialized mixer node instance, or null if the JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown if the JSON is invalid or cannot be deserialized</exception>
    public static MixerNode? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        var model = JsonSerializer.Deserialize<MixerNodeJsonModel>(json, _jsonOptions);
        return model?.ToMixerNode();
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="MixerNode"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized mixer node, or null if deserialization fails</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    public static bool TryFromJson(string json, out MixerNode? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            var model = JsonSerializer.Deserialize<MixerNodeJsonModel>(json, _jsonOptions);
            if (model != null)
            {
                value = model.ToMixerNode();
                return true;
            }

            return false;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Internal model used for JSON serialization/deserialization.
    /// </summary>
    private sealed class MixerNodeJsonModel
    {
        public string Name { get; set; } = string.Empty;
        public int InputCount { get; set; }
        public int Frames { get; set; }
        public float[] Gains { get; set; } = Array.Empty<float>();

        public MixerNodeJsonModel()
        {
        }

        public MixerNodeJsonModel(MixerNode node)
        {
            Name = node.Name;
            InputCount = node.InputCount;
            Frames = node.Frames;
            Gains = new float[node.InputCount];
            for (int i = 0; i < node.InputCount; i++)
            {
                Gains[i] = node.GetGain(i);
            }
        }

        public MixerNode ToMixerNode()
        {
            var node = new MixerNode(Name, InputCount, Frames);

            for (int i = 0; i < Math.Min(Gains.Length, InputCount); i++)
            {
                node.SetGain(i, Gains[i]);
            }

            return node;
        }
    }
}