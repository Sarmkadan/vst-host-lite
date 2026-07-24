using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VstHostLite.Native.Tests
{
    /// <summary>
    /// Extension methods for <see cref="AudioGraph"/> to simplify test setup and common operations.
    /// </summary>
    public static class AudioGraphTestsExtensions
    {
        /// <summary>
        /// Adds multiple nodes with the specified names and default component (IntPtr.Zero) to the graph.
        /// </summary>
        /// <param name="graph">The audio graph to add nodes to.</param>
        /// <param name="names">The names of the nodes to add.</param>
        /// <returns>
        /// A read-only list of the added nodes in the same order as the input names.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> or <paramref name="names"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="names"/> contains null, empty, or whitespace-only strings.</exception>
        public static IReadOnlyList<GraphNode> AddNodes(this AudioGraph graph, IEnumerable<string> names)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(names);

            var nodeList = new List<GraphNode>();
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Node name cannot be null, empty, or whitespace.", nameof(names));
                }

                var node = graph.AddNode(name, nint.Zero);
                nodeList.Add(node);
            }

            return nodeList;
        }

        /// <summary>
        /// Connects a sequence of nodes in a chain (first → second, second → third, etc.).
        /// </summary>
        /// <param name="graph">The audio graph containing the nodes.</param>
        /// <param name="nodes">The nodes to connect in sequence.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> or <paramref name="nodes"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="nodes"/> contains fewer than two nodes.</exception>
        public static void ConnectChain(this AudioGraph graph, IEnumerable<GraphNode> nodes)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(nodes);

            var nodeList = nodes.ToList();
            if (nodeList.Count < 2)
            {
                throw new ArgumentException("At least two nodes are required to form a chain.", nameof(nodes));
            }

            for (int i = 0; i < nodeList.Count - 1; i++)
            {
                graph.Connect(nodeList[i], nodeList[i + 1]);
            }
        }

        /// <summary>
        /// Creates a node with the specified name and component pointer, then adds it to the graph.
        /// </summary>
        /// <param name="graph">The audio graph to add the node to.</param>
        /// <param name="name">The name of the node.</param>
        /// <param name="component">The component pointer for the node.</param>
        /// <returns>The created node.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or whitespace.</exception>
        public static GraphNode AddNodeWithComponent(this AudioGraph graph, string name, nint component)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(name);
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Node name cannot be empty or whitespace.", nameof(name));
            }

            return graph.AddNode(name, component);
        }

        /// <summary>
        /// Finds a node by its name in the graph.
        /// </summary>
        /// <param name="graph">The audio graph to search.</param>
        /// <param name="name">The name of the node to find.</param>
        /// <returns>The node with the specified name, or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> or <paramref name="name"/> is null.</exception>
        public static GraphNode? FindNodeByName(this AudioGraph graph, string name)
        {
            ArgumentNullException.ThrowIfNull(graph);
            ArgumentNullException.ThrowIfNull(name);

            return graph.Nodes.FirstOrDefault(n => n.Name == name);
        }
    }
}