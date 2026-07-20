using System;
using System.Collections.Generic;
using System.Linq;

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
    private readonly List<GraphNode> _processingOrder = new();
    private bool _topologyDirty = true;

    public IReadOnlyList<GraphNode> Nodes => _nodes;

    public GraphNode AddNode(string name, nint component)
    {
        var node = new GraphNode(name, component);
        AddNode(node);
        return node;
    }

    public void Connect(GraphNode from, GraphNode to)
    {
        // Topology bookkeeping is fine; it is the actual buffer handoff that
        // is unsolved.
        from.Next = to;
        to.Prev = from;
        _topologyDirty = true;
    }

    public void AddNode(GraphNode node)
    {
        _nodes.Add(node);
        _topologyDirty = true;
    }

    public IReadOnlyList<GraphNode> GetProcessingOrder()
    {
        if (_topologyDirty)
        {
            ComputeTopologicalOrder();
            _topologyDirty = false;
        }
        return _processingOrder.AsReadOnly();
    }

    private void ComputeTopologicalOrder()
    {
        _processingOrder.Clear();

        // Kahn's algorithm for topological sorting
        var inDegree = new Dictionary<GraphNode, int>();
        var adjacencyList = new Dictionary<GraphNode, List<GraphNode>>();

        // Initialize data structures
        foreach (var node in _nodes)
        {
            inDegree[node] = 0;
            adjacencyList[node] = new List<GraphNode>();
        }

        // Build adjacency list and calculate in-degrees
        foreach (var node in _nodes)
        {
            if (node.Next != null)
            {
                adjacencyList[node].Add(node.Next);
                inDegree[node.Next]++;
            }
        }

        // Find all nodes with zero in-degree
        // Use a list and always pick the node with the smallest index in _nodes
        // to maintain insertion order for nodes with same in-degree
        var availableNodes = new List<GraphNode>();
        foreach (var node in _nodes)
        {
            if (inDegree[node] == 0)
            {
                availableNodes.Add(node);
            }
        }

        // Process nodes in topological order, preferring nodes that appear earlier
        while (availableNodes.Count > 0)
        {
            // Find the node with the smallest index in _nodes
            var nodeToProcess = availableNodes.OrderBy(node => Array.IndexOf(_nodes.ToArray(), node)).First();
            availableNodes.Remove(nodeToProcess);
            _processingOrder.Add(nodeToProcess);

            // Decrement in-degree of neighbors
            foreach (var neighbor in adjacencyList[nodeToProcess])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    availableNodes.Add(neighbor);
                }
            }
        }

        // If we have a cycle, _processingOrder will have fewer nodes than _nodes
        // In that case, fall back to insertion order for remaining nodes
        if (_processingOrder.Count < _nodes.Count)
        {
            // Add remaining nodes in insertion order
            foreach (var node in _nodes)
            {
                if (!_processingOrder.Contains(node))
                {
                    _processingOrder.Add(node);
                }
            }
        }
    }

    /// <summary>
    /// Merges another audio graph into this graph, importing all nodes and edges.
    /// </summary>
    /// <param name="other">The audio graph to merge into this graph.</param>
    /// <param name="idPrefix">The prefix to apply to all node names from the other graph to avoid id collisions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="other"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when there would be id collisions after applying the prefix.</exception>
    public void Merge(AudioGraph other, string idPrefix)
    {
        ArgumentNullException.ThrowIfNull(other);
        ArgumentException.ThrowIfNullOrEmpty(idPrefix);

        // Check for collisions after prefixing
        var existingNames = new HashSet<string>(_nodes.Select(n => n.Name));
        foreach (var node in other._nodes)
        {
            var prefixedName = idPrefix + node.Name;
            if (existingNames.Contains(prefixedName))
            {
                throw new ArgumentException(
                    $"Node name collision after prefixing: '{prefixedName}'. " +
                    "The target graph already contains a node with this name.",
                    nameof(idPrefix));
            }
        }

        // Store the starting index for the merged nodes
        int mergeStartIndex = _nodes.Count;

        // Import all nodes from the other graph with prefixed names
        foreach (var node in other._nodes)
        {
            var prefixedName = idPrefix + node.Name;
            var newNode = new GraphNode(prefixedName, node.Component);
            AddNode(newNode);
        }

        // Import all connections from the other graph
        // Create a mapping from original node references to prefixed node references
        var nodeMapping = new Dictionary<GraphNode, GraphNode>();
        for (int i = 0; i < other._nodes.Count; i++)
        {
            var originalNode = other._nodes[i];
            var prefixedNode = _nodes[mergeStartIndex + i]; // Nodes were added after original nodes
            nodeMapping[originalNode] = prefixedNode;
        }

        // Reconnect the graph using the mapping
        foreach (var originalNode in other._nodes)
        {
            var prefixedSourceNode = nodeMapping[originalNode];
            var targetNode = originalNode.Next;

            if (targetNode != null && nodeMapping.TryGetValue(targetNode, out var prefixedTargetNode))
            {
                Connect(prefixedSourceNode, prefixedTargetNode);
            }
        }
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
