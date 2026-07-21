using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides System.Text.Json serialization helpers for <see cref="AudioGraph"/> instances.
/// </summary>
public static class AudioGraphExtensionsJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options configured for camelCase property names.
    /// </summary>
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters = { new NintJsonConverter() }
    };

    /// <summary>
    /// Serializes an <see cref="AudioGraph"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The audio graph instance to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON representation of the audio graph.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this AudioGraph value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        var nodesInOrder = value.GetNodesInOrder().ToList();
        var dto = new AudioGraphExtensionsDto(nodesInOrder.Select(node => new NodeDto
        {
            Name = node.Name,
            Component = node.Component,
            NextIndex = node.Next is { } next ? nodesInOrder.IndexOf(next) : -1
        }).ToList());

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AudioGraph"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="AudioGraph"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or missing required data.</exception>
    public static AudioGraph FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        var dto = JsonSerializer.Deserialize<AudioGraphExtensionsDto>(json, _options)
            ?? throw new JsonException("Deserialized AudioGraphExtensionsDto is null.");

        if (dto.Nodes.Count == 0)
        {
            throw new JsonException("AudioGraphExtensionsDto must contain at least one NodeDto.");
        }

        var graph = new AudioGraph();
        var nodes = new System.Collections.Generic.List<GraphNode>();

        // Create all nodes first
        foreach (var nodeDto in dto.Nodes)
        {
            var node = graph.AddNode(nodeDto.Name, nodeDto.Component);
            nodes.Add(node);
        }

        // Reconstruct connections
        for (int i = 0; i < dto.Nodes.Count; i++)
        {
            var nodeDto = dto.Nodes[i];
            if (nodeDto.NextIndex >= 0 && nodeDto.NextIndex < nodes.Count)
            {
                graph.Connect(nodes[i], nodes[nodeDto.NextIndex]);
            }
        }

        return graph;
    }

    /// <summary>
    /// Tries to deserialize a JSON string into an <see cref="AudioGraph"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized audio graph if successful; otherwise <c>null</c>.</param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out AudioGraph? value)
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
    /// Data transfer object used for JSON serialization and deserialization of <see cref="AudioGraphExtensions"/>.
    /// </summary>
    /// <param name="Nodes">The collection of node DTOs representing the audio graph structure.</param>
    private sealed record AudioGraphExtensionsDto(System.Collections.Generic.List<NodeDto> Nodes);

    /// <summary>
    /// Data transfer object for a single graph node.
    /// </summary>
    private sealed record NodeDto
    {
        /// <summary>Gets the name of the audio graph node.</summary>
        public required string Name { get; init; }

        /// <summary>Gets the component pointer associated with this node.</summary>
        public required nint Component { get; init; }

        /// <summary>Gets or sets the index of the next node in the graph, or -1 if this is the last node.</summary>
        public int NextIndex { get; init; } = -1;
    }

    /// <summary>
    /// Custom JSON converter for <see cref="nint"/> (IntPtr) values.
    /// Serializes as a hexadecimal string for JSON compatibility.
    /// </summary>
    private sealed class NintJsonConverter : JsonConverter<nint>
    {
        public override nint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return nint.Zero;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                // Try to read as number first (for backwards compatibility)
                if (reader.TryGetInt64(out long longValue))
                {
                    return (nint)longValue;
                }
            }

            // Read as string and parse as hex
            string? hexString = reader.GetString();
            if (string.IsNullOrEmpty(hexString))
            {
                return nint.Zero;
            }

            if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexString = hexString.Substring(2);
            }

            if (ulong.TryParse(hexString, System.Globalization.NumberStyles.HexNumber, null, out ulong ulongValue))
            {
                return (nint)ulongValue;
            }

            return nint.Zero;
        }

        public override void Write(Utf8JsonWriter writer, nint value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("X"));
        }
    }
}