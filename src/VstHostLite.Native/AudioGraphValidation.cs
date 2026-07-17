namespace VstHostLite.Native;

/// <summary>
/// Provides validation helpers for <see cref="AudioGraph"/> instances.
/// </summary>
public static class AudioGraphValidation
{
    /// <summary>
    /// Validates an <see cref="AudioGraph"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The audio graph to validate.</param>
    /// <returns>An enumerable of validation problems; empty if the graph is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this AudioGraph? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate nodes collection
        if (value.Nodes.Count == 0)
        {
            problems.Add("AudioGraph must contain at least one node.");
        }

        // Validate each node
        foreach (var node in value.Nodes)
        {
            ValidateNode(node, problems);
        }

        // Validate connections
        ValidateConnections(value.Nodes, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="AudioGraph"/> instance is valid.
    /// </summary>
    /// <param name="value">The audio graph to check.</param>
    /// <returns>True if the graph is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this AudioGraph? value)
        => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that an <see cref="AudioGraph"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The audio graph to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the graph contains validation problems.</exception>
    public static void EnsureValid(this AudioGraph? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"AudioGraph is invalid. Problems:\n{string.Join("\n", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a single <see cref="GraphNode"/> and adds any problems to the list.
    /// </summary>
    /// <param name="node">The node to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    private static void ValidateNode(GraphNode node, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(problems);

        // Validate node name
        if (string.IsNullOrWhiteSpace(node.Name))
        {
            problems.Add($"Node '{GetNodeIdentifier(node)}' has an invalid name: must be non-null, non-empty, and not whitespace.");
        }

        // Validate component pointer
        if (node.Component == nint.Zero)
        {
            problems.Add($"Node '{GetNodeIdentifier(node)}' has a null component pointer (nint.Zero).");
        }

        // Validate connection pointers (these are set by Connect calls)
        if (node.Prev == node)
        {
            problems.Add($"Node '{GetNodeIdentifier(node)}' has a self-reference in Prev.");
        }

        if (node.Next == node)
        {
            problems.Add($"Node '{GetNodeIdentifier(node)}' has a self-reference in Next.");
        }
    }

    /// <summary>
    /// Validates the connections between nodes in the audio graph.
    /// </summary>
    /// <param name="nodes">The collection of nodes to validate.</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    private static void ValidateConnections(IReadOnlyList<GraphNode> nodes, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(problems);

        if (nodes.Count == 0)
        {
            return;
        }

        var visited = new HashSet<GraphNode>();
        var hasCycle = false;

        // Check for cycles using DFS
        foreach (var node in nodes)
        {
            if (!visited.Contains(node))
            {
                CheckCycle(node, visited, new HashSet<GraphNode>(), problems, ref hasCycle);
                if (hasCycle)
                {
                    break;
                }
            }
        }

        // Check for disconnected components
        var connectedComponents = new HashSet<GraphNode>();
        foreach (var node in nodes)
        {
            if (node.Prev == null && node.Next == null)
            {
                // Single node with no connections is valid
                continue;
            }

            // Traverse backwards
            for (var current = node; current != null; current = current.Prev)
            {
                connectedComponents.Add(current);
            }

            // Traverse forwards
            for (var current = node; current != null; current = current.Next)
            {
                connectedComponents.Add(current);
            }
        }

        // Report nodes not in any connected component
        foreach (var node in nodes)
        {
            if (!connectedComponents.Contains(node) && node.Prev is not null || node.Next is not null)
            {
                problems.Add($"Node '{GetNodeIdentifier(node)}' is part of a disconnected component.");
            }
        }
    }

    /// <summary>
    /// Checks for cycles in the audio graph using depth-first search.
    /// </summary>
    /// <param name="node">The current node being visited.</param>
    /// <param name="visited">Set of nodes that have been fully visited.</param>
    /// <param name="recursionStack">Set of nodes in the current recursion stack (for cycle detection).</param>
    /// <param name="problems">The list to accumulate validation problems.</param>
    /// <param name="hasCycle">Reference to a flag indicating if a cycle has been found.</param>
    private static void CheckCycle(GraphNode node, HashSet<GraphNode> visited, HashSet<GraphNode> recursionStack,
        List<string> problems, ref bool hasCycle)
    {
        if (hasCycle)
        {
            return;
        }

        if (recursionStack.Contains(node))
        {
            hasCycle = true;
            problems.Add($"AudioGraph contains a cycle involving node '{GetNodeIdentifier(node)}'.");
            return;
        }

        if (visited.Contains(node))
        {
            return;
        }

        visited.Add(node);
        recursionStack.Add(node);

        if (node.Prev != null)
        {
            CheckCycle(node.Prev, visited, recursionStack, problems, ref hasCycle);
        }

        if (!hasCycle && node.Next != null)
        {
            CheckCycle(node.Next, visited, recursionStack, problems, ref hasCycle);
        }

        recursionStack.Remove(node);
    }

    /// <summary>
    /// Gets a human-readable identifier for a node, using its name if available or a hash code otherwise.
    /// </summary>
    /// <param name="node">The node to identify.</param>
    /// <returns>A string identifier for the node.</returns>
    private static string GetNodeIdentifier(GraphNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return string.IsNullOrWhiteSpace(node.Name)
            ? $"Node@{node.GetHashCode():X8}"
            : node.Name;
    }
}