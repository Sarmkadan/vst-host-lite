using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Data transfer object used for JSON serialization and deserialization of an <see cref="AudioGraph"/>.
/// </summary>
/// <param name="Nodes">The collection of node DTOs representing the audio graph structure, in processing order.</param>
internal sealed record AudioGraphDto(System.Collections.Generic.List<AudioGraphNodeDto> Nodes);

/// <summary>
/// Data transfer object for a single <see cref="GraphNode"/> within an <see cref="AudioGraph"/>.
/// </summary>
internal sealed record AudioGraphNodeDto
{
    /// <summary>Gets the name of the audio graph node.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the native component pointer associated with this node.</summary>
    public required nint Component { get; init; }

    /// <summary>Gets the index of the next node in the graph, or -1 if this is the last node.</summary>
    public int NextIndex { get; init; } = -1;
}

/// <summary>
/// Single System.Text.Json source-generated serialization context for the VST host's graph model.
/// This is the one consolidated entry point for graph-related JSON (de)serialization; new node or
/// graph DTOs should be registered here rather than growing another ad-hoc <c>XxxJsonExtensions</c> type.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = false,
    Converters = [typeof(NintJsonConverter)])]
[JsonSerializable(typeof(AudioGraphDto))]
[JsonSerializable(typeof(AudioGraphNodeDto))]
internal partial class Vst3HostJsonContext : JsonSerializerContext
{
}
