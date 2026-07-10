namespace VstHostLite.Native;

/// <summary>
/// Intended to route audio buffers between the input device, a chain of plugin
/// nodes and the output device. This never got working - see the notes below
/// and the README. Enumerating factories and instantiating a component works,
/// but wiring IAudioProcessor::process with the right ProcessData/AudioBusBuffers
/// marshalling always crashed or produced silence.
/// </summary>
public sealed class AudioGraph
{
    private readonly List<GraphNode> _nodes = new();

    public IReadOnlyList<GraphNode> Nodes => _nodes;

    public GraphNode AddNode(string name, nint component)
    {
        var node = new GraphNode(name, component);
        _nodes.Add(node);
        return node;
    }

    public void Connect(GraphNode from, GraphNode to)
    {
        // Topology bookkeeping is fine; it is the actual buffer handoff that
        // is unsolved.
        from.Next = to;
        to.Prev = from;
    }

    /// <summary>
    /// Pull one block through the graph.
    /// </summary>
    /// <remarks>
    /// BLOCKED: could not get the ProcessData struct to marshal correctly.
    /// AudioBusBuffers is a union (channelBuffers32 / channelBuffers64) of
    /// double-indirection pointers and the ThisCall into IAudioProcessor::process
    /// either returns kResultFalse or access-violates. Suspect the vtable slot
    /// index for process() is wrong on the components we tested, or the
    /// setupProcessing() call needs a valid ProcessSetup first. Shelved here.
    /// </remarks>
    public void ProcessBlock(float[] input, float[] output, int sampleFrames)
    {
        throw new NotImplementedException(
            "audio graph routing not working yet - ProcessData marshalling to " +
            "IAudioProcessor::process is unsolved (see remarks / README)");
    }
}

public sealed class GraphNode
{
    public GraphNode(string name, nint component)
    {
        Name = name;
        Component = component;
    }

    public string Name { get; }
    public nint Component { get; }
    public GraphNode? Prev { get; internal set; }
    public GraphNode? Next { get; internal set; }
}
