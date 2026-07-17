namespace VstHostLite.Native;

/// <summary>
/// Provides extension methods for <see cref="AudioGraph"/> to simplify common graph operations.
/// </summary>
public static class AudioGraphExtensions
{
    /// <summary>
    /// Removes the specified node from the graph and updates all connections.
    /// </summary>
    /// <param name="graph">The audio graph instance.</param>
    /// <param name="node">The node to remove.</param>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> or <paramref name="node"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="node"/> is not in the graph.</exception>
    public static void RemoveNode(this AudioGraph graph, GraphNode node)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(node);

        // Remove from the nodes collection
        var nodes = graph.Nodes;
        if (!nodes.Contains(node))
        {
            throw new ArgumentException("The specified node is not part of the graph.", nameof(node));
        }

        var nodesList = (List<GraphNode>)nodes;
        nodesList.Remove(node);

        // Update connections for adjacent nodes
        if (node.Prev is { } prev)
        {
            prev.Next = node.Next;
        }

        if (node.Next is { } next)
        {
            next.Prev = node.Prev;
        }

        // Clear the removed node's connections
        node.Prev = null;
        node.Next = null;
    }

    /// <summary>
    /// Removes all nodes from the graph and clears all connections.
    /// </summary>
    /// <param name="graph">The audio graph instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    public static void Clear(this AudioGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var nodes = graph.Nodes;
        foreach (var node in nodes)
        {
            node.Prev = null;
            node.Next = null;
        }

        ((List<GraphNode>)nodes).Clear();
    }

    /// <summary>
    /// Gets all nodes in the graph in sequential order starting from the first node.
    /// </summary>
    /// <param name="graph">The audio graph instance.</param>
    /// <returns>An enumerable of nodes in sequential order, or empty if graph has no nodes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    public static IEnumerable<GraphNode> GetNodesInOrder(this AudioGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (graph.Nodes.Count == 0)
        {
            return [];
        }

        var startNode = graph.Nodes[0];
        while (startNode.Prev is { } prev)
        {
            startNode = prev;
        }

        var result = new List<GraphNode>();
        var current = startNode;
        while (current is { } curr)
        {
            result.Add(curr);
            current = curr.Next;
        }

        return result;
    }

    /// <summary>
    /// Finds a node by its component pointer.
    /// </summary>
    /// <param name="graph">The audio graph instance.</param>
    /// <param name="component">The component pointer to search for.</param>
    /// <returns>The found node, or <see langword="null"/> if not found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    public static GraphNode? FindNodeByComponent(this AudioGraph graph, nint component)
    {
        ArgumentNullException.ThrowIfNull(graph);

        return graph.Nodes.FirstOrDefault(node => node.Component == component);
    }

    /// <summary>
    /// Gets the first node in the graph (the one with no previous node).
    /// </summary>
    /// <param name="graph">The audio graph instance.</param>
    /// <returns>The first node, or <see langword="null"/> if the graph is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    public static GraphNode? GetFirstNode(this AudioGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (graph.Nodes.Count == 0)
        {
            return null;
        }

        var node = graph.Nodes[0];
        while (node.Prev is { } prev)
        {
            node = prev;
        }

        return node;
    }

    /// <summary>
    /// Gets the last node in the graph (the one with no next node).
    /// </summary>
    /// <param name="graph">The audio graph instance.</param>
    /// <returns>The last node, or <see langword="null"/> if the graph is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="graph"/> is <see langword="null"/>.</exception>
    public static GraphNode? GetLastNode(this AudioGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        if (graph.Nodes.Count == 0)
        {
            return null;
        }

        var node = graph.Nodes[0];
        while (node.Next is { } next)
        {
            node = next;
        }

        return node;
    }
}