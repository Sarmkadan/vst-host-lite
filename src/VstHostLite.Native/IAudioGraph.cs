namespace VstHostLite.Native;

/// <summary>
/// Interface for audio graph operations.
/// </summary>
public interface IAudioGraph
{
    /// <summary>
    /// Gets the nodes in the graph in the order they were added.
    /// </summary>
    IReadOnlyList<GraphNode> Nodes { get; }

    /// <summary>
    /// Creates and adds a graph node for a native component.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="component">The native component handle represented by the node.</param>
    /// <returns>The newly created node.</returns>
    GraphNode AddNode(string name, nint component);

    /// <summary>
    /// Connects a source node to a destination node.
    /// </summary>
    /// <param name="from">The source node.</param>
    /// <param name="to">The destination node.</param>
    /// <exception cref="InvalidOperationException">
    /// The nodes cannot be connected without creating an invalid graph topology.
    /// </exception>
    void Connect(GraphNode from, GraphNode to);

    /// <summary>
    /// Adds an existing node to the graph.
    /// </summary>
    /// <param name="node">The node to add.</param>
    void AddNode(GraphNode node);

    /// <summary>
    /// Gets the nodes in topological processing order.
    /// </summary>
    /// <returns>The nodes in the order in which they should be processed.</returns>
    /// <exception cref="InvalidOperationException">
    /// The graph contains a cycle and cannot be topologically sorted.
    /// </exception>
    IReadOnlyList<GraphNode> GetProcessingOrder();

    /// <summary>
    /// Gets the identifiers of the nodes in topological processing order.
    /// </summary>
    /// <returns>
    /// The node identifiers, where each identifier is the node's position in <see cref="Nodes"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The graph contains a cycle and cannot be topologically sorted.
    /// </exception>
    IReadOnlyList<int> GetProcessingOrderIds();

    /// <summary>
    /// Imports the nodes and connections from another graph.
    /// </summary>
    /// <param name="other">The graph to import.</param>
    /// <param name="idPrefix">The prefix to apply to imported node names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="idPrefix"/> is empty, or a prefixed node name conflicts with an existing node name.
    /// </exception>
    void Merge(AudioGraph other, string idPrefix);

    /// <summary>
    /// Processes one block of audio through the graph.
    /// </summary>
    /// <param name="options">The audio processing configuration.</param>
    /// <param name="input">The input sample buffer.</param>
    /// <param name="output">The output sample buffer.</param>
    /// <param name="sampleFrames">The number of sample frames to process.</param>
    /// <exception cref="NotImplementedException">Audio graph block processing is not implemented.</exception>
    void ProcessBlock(AudioProcessingOptions options, float[] input, float[] output, int sampleFrames);
}
