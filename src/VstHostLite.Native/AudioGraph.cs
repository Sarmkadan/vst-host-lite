using System;
using System.Collections.Generic;

namespace VstHostLite.Native;

/// <summary>
/// Intended to route audio buffers between the input device, a chain of plugin
/// nodes and the output device. This never got working - see the notes below
/// and the README. Enumerating factories and instantiating a component works,
/// but wiring IAudioProcessor::process with the right ProcessData/AudioBusBuffers
/// marshalling always crashed or produced silence.
/// </summary>
public sealed class AudioGraph : IAudioGraph
{
    private readonly List<GraphNode> _nodes = new();
    private readonly List<GraphNode> _processingOrder = new();
    private readonly Dictionary<int, GraphNode> _idToNode = new();
    private readonly Dictionary<GraphNode, int> _nodeToId = new();
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
        _idToNode[_nodes.Count - 1] = node;
        _nodeToId[node] = _nodes.Count - 1;
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

    public IReadOnlyList<int> GetProcessingOrderIds()
    {
        var order = GetProcessingOrder();
        var ids = new List<int>(order.Count);
        foreach (var n in order)
        {
            ids.Add(_nodeToId[n]);
        }
        return ids.AsReadOnly();
    }

    private void ComputeTopologicalOrder()
    {
        _processingOrder.Clear();

        // Kahn's algorithm for topological sorting with cycle detection
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
            GraphNode nodeToProcess = null;
            int minIdx = int.MaxValue;
            foreach (var candidate in availableNodes)
            {
                int idx = _nodeToId[candidate];
                if (idx < minIdx)
                {
                    minIdx = idx;
                    nodeToProcess = candidate;
                }
            }

            if (nodeToProcess == null)
                break; // should not happen

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

        // Check for cycles
        if (_processingOrder.Count < _nodes.Count)
        {
            // Find the cycle path
            var cyclePath = FindCyclePath();
            throw new InvalidOperationException(
                $"Audio graph contains a cycle and cannot be topologically sorted. Cycle path: {FormatCyclePath(cyclePath)}");
        }
    }

    private List<GraphNode> FindCyclePath()
    {
        // Use DFS to find a cycle
        var visited = new HashSet<GraphNode>();
        var recursionStack = new HashSet<GraphNode>();
        var parentMap = new Dictionary<GraphNode, GraphNode>();
        GraphNode? cycleStart = null;
        GraphNode? cycleEnd = null;

        bool DFS(GraphNode node)
        {
            if (recursionStack.Contains(node))
            {
                cycleStart = node;
                cycleEnd = node;
                return true;
            }

            if (visited.Contains(node))
            {
                return false;
            }

            visited.Add(node);
            recursionStack.Add(node);

            if (node.Next != null)
            {
                parentMap[node.Next] = node;
                if (DFS(node.Next))
                {
                    return true;
                }
            }

            recursionStack.Remove(node);
            return false;
        }

        // Try to find a cycle starting from each node
        foreach (var node in _nodes)
        {
            if (DFS(node))
            {
                break;
            }
        }

        // Reconstruct the cycle path
        if (cycleStart != null && cycleEnd != null)
        {
            var cyclePath = new List<GraphNode>();
            var current = cycleEnd;
            cyclePath.Add(current);

            // Walk back through parent pointers until we reach cycleStart
            while (current != cycleStart && parentMap.TryGetValue(current, out var parent))
            {
                current = parent;
                cyclePath.Add(current);
            }

            // Reverse to get the cycle in forward direction
            cyclePath.Reverse();
            return cyclePath;
        }

        // If we couldn't find a cycle with DFS, return nodes that weren't processed
        var unprocessed = new List<GraphNode>();
        foreach (var n in _nodes)
        {
            if (!_processingOrder.Contains(n))
                unprocessed.Add(n);
        }
        return unprocessed;
    }

    private string FormatCyclePath(List<GraphNode> cyclePath)
    {
        if (cyclePath.Count == 0)
        {
            return "unknown cycle";
        }

        var nodeIds = new List<int>(cyclePath.Count);
        foreach (var n in cyclePath)
        {
            nodeIds.Add(_nodeToId[n]);
        }
        return $"[{string.Join(" → ", nodeIds)}]";
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
        var existingNames = new HashSet<string>();
        foreach (var n in _nodes)
        {
            existingNames.Add(n.Name);
        }

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
    public void ProcessBlock(AudioProcessingOptions options, float[] input, float[] output, int sampleFrames)
    {
        throw new NotImplementedException(
            "audio graph routing not working yet - ProcessData marshalling to " +
            "IAudioProcessor::process is unsolved (see remarks / README)");
    }

    public bool Equals(AudioGraph? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Nodes.Count != other.Nodes.Count) return false;

        for (int i = 0; i < Nodes.Count; i++)
        {
            var thisNode = Nodes[i];
            var otherNode = other.Nodes[i];
            if (thisNode.Name != otherNode.Name || thisNode.Component != otherNode.Component)
                return false;

            var thisPrev = thisNode.Prev;
            var otherPrev = otherNode.Prev;
            if (thisPrev == null ^ otherPrev == null) return false;
            if (thisPrev != null && otherPrev != null)
            {
                if (thisPrev.Name != otherPrev.Name || thisPrev.Component != otherPrev.Component)
                    return false;
            }

            var thisNext = thisNode.Next;
            var otherNext = otherNode.Next;
            if (thisNext == null ^ otherNext == null) return false;
            if (thisNext != null && otherNext != null)
            {
                if (thisNext.Name != otherNext.Name || thisNext.Component != otherNext.Component)
                    return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AudioGraph);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var node in Nodes)
        {
            hash.Add(node.Name);
            hash.Add(node.Component);
            if (node.Prev != null)
            {
                hash.Add(node.Prev.Name);
                hash.Add(node.Prev.Component);
            }
            else
            {
                hash.Add((string?)null);
                hash.Add((nint?)null);
            }
            if (node.Next != null)
            {
                hash.Add(node.Next.Name);
                hash.Add(node.Next.Component);
            }
            else
            {
                hash.Add((string?)null);
                hash.Add((nint?)null);
            }
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(AudioGraph? left, AudioGraph? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(AudioGraph? left, AudioGraph? right)
    {
        return !(Equals(left, right));
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
