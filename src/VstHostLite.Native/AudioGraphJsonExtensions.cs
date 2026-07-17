using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON serialization helpers for <see cref="AudioGraph"/>.
/// </summary>
public static class AudioGraphJsonExtensions
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
    /// Serializes the <see cref="AudioGraph"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The audio graph to serialize.</param>
    /// <param name="indented">Whether the output should be indented.</param>
    /// <returns>A JSON representation of the audio graph.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this AudioGraph value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_options) { WriteIndented = true }
            : _options;

        var dto = new AudioGraphDto(value.GetNodesInOrder().Select(node => new NodeDto
        {
            Name = node.Name,
            Component = node.Component,
            NextIndex = -1
        }).ToList());

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an <see cref="AudioGraph"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An <see cref="AudioGraph"/> instance, or <c>null</c> if deserialization fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or missing required data.</exception>
    public static AudioGraph? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var dto = JsonSerializer.Deserialize<AudioGraphDto>(json, _options)
            ?? throw new JsonException("Deserialized object is null.");

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
    /// DTO used for JSON (de)serialization of AudioGraph.
    /// </summary>
    private sealed record AudioGraphDto(System.Collections.Generic.List<NodeDto> Nodes);

    /// <summary>
    /// DTO for a single graph node.
    /// </summary>
    private sealed record NodeDto
    {
        public required string Name { get; init; }
        public required nint Component { get; init; }
        public int NextIndex { get; init; } = -1;
    }
}