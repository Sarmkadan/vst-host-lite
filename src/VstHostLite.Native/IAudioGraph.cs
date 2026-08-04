namespace VstHostLite.Native;

/// <summary>
/// Interface for audio graph operations.
/// </summary>
public interface IAudioGraph
{
    IReadOnlyList<GraphNode> Nodes { get; }
    GraphNode AddNode(string name, nint component);
    void Connect(GraphNode from, GraphNode to);
    void AddNode(GraphNode node);
    IReadOnlyList<GraphNode> GetProcessingOrder();
    IReadOnlyList<int> GetProcessingOrderIds();
    void Merge(AudioGraph other, string idPrefix);
    void ProcessBlock(AudioProcessingOptions options, float[] input, float[] output, int sampleFrames);
}