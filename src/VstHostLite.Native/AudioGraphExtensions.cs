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
    public static void RemoveNode(this AudioGraph graph, GraphNode node)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(node);

        // Remove from the nodes collection
        var nodesList = (List<GraphNode>)graph.Nodes;
        nodesList.Remove(node);

        // Update connections for adjacent nodes
        if (node.Prev is not null)
        {
            node.Prev.Next = node.Next;
        }

        if (node.Next is not null)
        {
            node.Next.Prev = node.Prev;
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

        var nodesList = (List<GraphNode>)graph.Nodes;
        foreach (var node in nodesList)
        {
            node.Prev = null;
            node.Next = null;
        }

        nodesList.Clear();
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
            return Enumerable.Empty<GraphNode>();
        }

        var startNode = graph.Nodes[0];
        while (startNode.Prev is not null)
        {
            startNode = startNode.Prev;
        }

        var result = new List<GraphNode>();
        var current = startNode;
        while (current is not null)
        {
            result.Add(current);
            current = current.Next;
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

        foreach (var node in graph.Nodes)
        {
            if (node.Component == component)
            {
                return node;
            }
        }

        return null;
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
        while (node.Prev is not null)
        {
            node = node.Prev;
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
        while (node.Next is not null)
        {
            node = node.Next;
        }

        return node;
    }
}