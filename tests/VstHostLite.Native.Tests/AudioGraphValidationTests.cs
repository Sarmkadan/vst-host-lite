using System;
using System.Reflection;
using Xunit;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Unit tests covering <c>AudioGraph.Validate</c>, <c>AudioGraph.IsValid</c>, and
/// <c>AudioGraph.EnsureValid</c>, including null-name/null-component detection,
/// self-reference and cycle detection, and disconnected-component detection.
/// </summary>
public class AudioGraphValidationTests
{
    /// <summary>
    /// Verifies that calling <c>Validate</c> on a null <see cref="AudioGraph"/> reference throws
    /// <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void Validate_ThrowsOnNullGraph()
    {
        // Arrange
        AudioGraph? nullGraph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullGraph!.Validate());
    }

    /// <summary>
    /// Verifies that validating an empty graph returns a single problem stating the graph
    /// must contain at least one node.
    /// </summary>
    [Fact]
    public void Validate_EmptyGraph_ReturnsError()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Equal("AudioGraph must contain at least one node.", problems[0]);
    }

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <c>false</c> for an empty graph.
    /// </summary>
    [Fact]
    public void Validate_EmptyGraph_IsValid_ReturnsFalse()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var isValid = graph.IsValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that a single node with a null name produces a problem describing the
    /// invalid name, identifying the node by its default "Node@..." display name.
    /// </summary>
    [Fact]
    public void Validate_SingleNodeWithNullName_ReturnsError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode(null!, nint.Zero);
        graph.AddNode(node);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.StartsWith("Node 'Node@", problems[0]);
        Assert.EndsWith("has an invalid name: must be non-null, non-empty, and not whitespace.", problems[0]);
    }

    /// <summary>
    /// Verifies that a single node whose name is whitespace-only produces a problem
    /// describing the invalid name, identifying the node by its whitespace name.
    /// </summary>
    [Fact]
    public void Validate_SingleNodeWithEmptyName_ReturnsError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode("   ", nint.Zero);
        graph.AddNode(node);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.StartsWith("Node '   '", problems[0]);
        Assert.EndsWith("has an invalid name: must be non-null, non-empty, and not whitespace.", problems[0]);
    }

    /// <summary>
    /// Verifies that a single node with a valid, non-empty name and no component pointer
    /// issue produces no validation problems.
    /// </summary>
    [Fact]
    public void Validate_SingleNodeWithValidName_ReturnsNoErrors()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = graph.AddNode("validNode", nint.Zero);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that a single node with a null component pointer (<see cref="nint.Zero"/>)
    /// produces a problem describing the null component pointer.
    /// </summary>
    [Fact]
    public void Validate_SingleNodeWithNullComponent_ReturnsError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode("testNode", nint.Zero);
        graph.AddNode(node);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.StartsWith("Node 'testNode'", problems[0]);
        Assert.EndsWith("has a null component pointer (nint.Zero).", problems[0]);
    }

    /// <summary>
    /// Verifies that a single node with a non-null component pointer produces no
    /// validation problems.
    /// </summary>
    [Fact]
    public void Validate_SingleNodeWithValidComponent_ReturnsNoErrors()
    {
        // Arrange
        var graph = new AudioGraph();
        var componentPtr = new nint(1);
        var node = new GraphNode("testNode", componentPtr);
        graph.AddNode(node);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that a node whose <c>Prev</c> reference is forced (via reflection) to point
    /// to itself produces a problem describing the self-reference in <c>Prev</c>.
    /// </summary>
    [Fact]
    public void Validate_NodeWithSelfReferenceInPrev_ReturnsError()
    {
        // Arrange - Create a node and use reflection to set Prev to create self-reference
        var graph = new AudioGraph();
        var node = graph.AddNode("selfRefNode", new nint(1));

        // Use reflection to set the internal Prev property
        var prevProperty = typeof(GraphNode).GetProperty("Prev", BindingFlags.Public | BindingFlags.Instance);
        prevProperty?.SetValue(node, node);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.StartsWith("Node 'selfRefNode'", problems[0]);
        Assert.EndsWith("has a self-reference in Prev.", problems[0]);
    }

    /// <summary>
    /// Verifies that a node whose <c>Next</c> reference is forced (via reflection) to point
    /// to itself produces a problem describing the self-reference in <c>Next</c>.
    /// </summary>
    [Fact]
    public void Validate_NodeWithSelfReferenceInNext_ReturnsError()
    {
        // Arrange - Create a node and use reflection to set Next to create self-reference
        var graph = new AudioGraph();
        var node = graph.AddNode("selfRefNode", new nint(1));

        // Use reflection to set the internal Next property
        var nextProperty = typeof(GraphNode).GetProperty("Next", BindingFlags.Public | BindingFlags.Instance);
        nextProperty?.SetValue(node, node);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.StartsWith("Node 'selfRefNode'", problems[0]);
        Assert.EndsWith("has a self-reference in Next.", problems[0]);
    }

    /// <summary>
    /// Verifies that a valid linear chain of three connected nodes produces no validation
    /// problems.
    /// </summary>
    [Fact]
    public void Validate_LinearChain_ReturnsNoErrors()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        var node3 = graph.AddNode("node3", new nint(3));
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that a two-node cycle (node1 -&gt; node2 -&gt; node1) is detected and reported
    /// as a single cycle problem naming the first node involved.
    /// </summary>
    [Fact]
    public void Validate_GraphWithCycle_ReturnsCycleError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        graph.Connect(node1, node2);
        graph.Connect(node2, node1); // Create cycle

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Equal("AudioGraph contains a cycle involving node 'node1'.", problems[0]);
    }

    /// <summary>
    /// Verifies that a three-node cycle (node1 -&gt; node2 -&gt; node3 -&gt; node1) is detected
    /// and reported as a single cycle problem naming the first node involved.
    /// </summary>
    [Fact]
    public void Validate_GraphWithCycleInvolvingMultipleNodes_ReturnsCycleError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        var node3 = graph.AddNode("node3", new nint(3));
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);
        graph.Connect(node3, node1); // Create cycle

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Equal("AudioGraph contains a cycle involving node 'node1'.", problems[0]);
    }

    /// <summary>
    /// Verifies that a node with no connections to an otherwise connected pair of nodes is
    /// reported as belonging to a disconnected component.
    /// </summary>
    [Fact]
    public void Validate_DisconnectedNodes_ReturnsDisconnectedError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        var node3 = graph.AddNode("node3", new nint(3));
        graph.Connect(node1, node2);
        // node3 is disconnected

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.StartsWith("Node '", problems[0]);
        Assert.Contains("node3", problems[0]);
        Assert.EndsWith("is part of a disconnected component.", problems[0]);
    }

    /// <summary>
    /// Verifies that when a graph contains two separate connected chains, both chains other
    /// than the first are reported as disconnected components, producing one problem per
    /// affected node.
    /// </summary>
    [Fact]
    public void Validate_MultipleDisconnectedComponents_ReturnsDisconnectedErrors()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        var node3 = graph.AddNode("node3", new nint(3));
        var node4 = graph.AddNode("node4", new nint(4));
        graph.Connect(node1, node2);
        graph.Connect(node3, node4);
        // Two disconnected chains

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.Contains("node3") && p.Contains("disconnected component"));
        Assert.Contains(problems, p => p.Contains("node4") && p.Contains("disconnected component"));
    }

    /// <summary>
    /// Verifies that a graph consisting solely of isolated nodes with no connections at all
    /// produces no validation problems.
    /// </summary>
    [Fact]
    public void Validate_IsolatedNodeWithNoConnections_ReturnsNoErrors()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        var node3 = graph.AddNode("node3", new nint(3));
        // All nodes are isolated (no connections)

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that a realistic, fully connected linear signal chain (input -&gt; eq -&gt;
    /// compressor -&gt; output) produces no validation problems.
    /// </summary>
    [Fact]
    public void Validate_ComplexValidGraph_ReturnsNoErrors()
    {
        // Arrange - Create a complex but valid graph
        var graph = new AudioGraph();
        var node1 = graph.AddNode("input", new nint(1));
        var node2 = graph.AddNode("eq", new nint(2));
        var node3 = graph.AddNode("compressor", new nint(3));
        var node4 = graph.AddNode("output", new nint(4));

        graph.Connect(node1, node2);
        graph.Connect(node2, node3);
        graph.Connect(node3, node4);

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Empty(problems);
    }

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <c>false</c> for an empty graph.
    /// </summary>
    [Fact]
    public void IsValid_EmptyGraph_ReturnsFalse()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var isValid = graph.IsValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <c>true</c> for a graph with two connected,
    /// properly named and componented nodes.
    /// </summary>
    [Fact]
    public void IsValid_ValidGraph_ReturnsTrue()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        graph.Connect(node1, node2);

        // Act
        var isValid = graph.IsValid();

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Verifies that <c>IsValid</c> returns <c>false</c> when a node has a null component
    /// pointer.
    /// </summary>
    [Fact]
    public void IsValid_InvalidGraph_ReturnsFalse()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero); // Invalid: null component

        // Act
        var isValid = graph.IsValid();

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Verifies that <c>EnsureValid</c> throws <see cref="ArgumentException"/> for an empty
    /// graph, with a message that includes both the summary text and the underlying
    /// "must contain at least one node" problem.
    /// </summary>
    [Fact]
    public void EnsureValid_EmptyGraph_Throws()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => graph.EnsureValid());
        Assert.Contains("AudioGraph is invalid", exception.Message);
        Assert.Contains("AudioGraph must contain at least one node", exception.Message);
    }

    /// <summary>
    /// Verifies that <c>EnsureValid</c> does not throw for a valid graph with two connected
    /// nodes.
    /// </summary>
    [Fact]
    public void EnsureValid_ValidGraph_DoesNotThrow()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        graph.Connect(node1, node2);

        // Act - Should not throw
        graph.EnsureValid();
    }

    /// <summary>
    /// Verifies that calling <c>EnsureValid</c> on a null <see cref="AudioGraph"/> reference
    /// throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [Fact]
    public void EnsureValid_NullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? nullGraph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullGraph!.EnsureValid());
    }

    /// <summary>
    /// Verifies that <c>EnsureValid</c> throws <see cref="ArgumentException"/> when a node has
    /// a null component pointer, with a message that includes both the summary text and the
    /// underlying null-component problem.
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidGraph_ThrowsWithProblems()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero); // Invalid: null component

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => graph.EnsureValid());
        Assert.Contains("AudioGraph is invalid", exception.Message);
        Assert.Contains("has a null component pointer", exception.Message);
    }

    /// <summary>
    /// Verifies that a node combining a null name, a null component pointer, and a
    /// self-reference (forced via reflection) yields all three corresponding validation
    /// problems simultaneously.
    /// </summary>
    [Fact]
    public void Validate_MultipleProblems_ReturnsAllErrors()
    {
        // Arrange - Create graph with multiple validation problems
        var graph = new AudioGraph();
        var node1 = new GraphNode(null!, nint.Zero); // Null name + null component
        var node2 = graph.AddNode("node2", new nint(2));
        graph.AddNode(node1);
        graph.Connect(node1, node2);

        // Use reflection to set self-reference
        var nextProperty = typeof(GraphNode).GetProperty("Next", BindingFlags.Public | BindingFlags.Instance);
        nextProperty?.SetValue(node1, node1);

        // Act
        var problems = graph.Validate();

        // Assert - Should have multiple errors
        Assert.Equal(3, problems.Count);
        Assert.Contains(problems, p => p.Contains("invalid name"));
        Assert.Contains(problems, p => p.Contains("null component pointer"));
        Assert.Contains(problems, p => p.Contains("self-reference"));
    }

    /// <summary>
    /// Verifies that connecting a node to itself via <c>Connect</c> is detected as a cycle.
    /// </summary>
    [Fact]
    public void Validate_SelfConnectionInNext_ReturnsError()
    {
        // Arrange - Create a node and connect it to itself
        var graph = new AudioGraph();
        var node1 = graph.AddNode("selfConnected", new nint(1));
        graph.Connect(node1, node1); // Self-connection

        // Act
        var problems = graph.Validate();

        // Assert - Should detect the cycle
        Assert.Single(problems);
        Assert.Contains("cycle", problems[0]);
    }

    /// <summary>
    /// Verifies that a simple two-node cycle (A -&gt; B -&gt; A) created via <c>Connect</c> is
    /// reported as a cycle problem.
    /// </summary>
    [Fact]
    public void Validate_SimpleCycle_ReturnsCycleError()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(1));
        var node2 = graph.AddNode("node2", new nint(2));
        graph.Connect(node1, node2);
        graph.Connect(node2, node1); // Create cycle A -> B -> A

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("cycle", problems[0]);
    }

    /// <summary>
    /// Verifies that a longer four-node cycle (A -&gt; B -&gt; C -&gt; D -&gt; A) created via
    /// <c>Connect</c> is reported as a cycle problem.
    /// </summary>
    [Fact]
    public void Validate_LongCycle_ReturnsCycleError()
    {
        // Arrange - Create a longer cycle: A -> B -> C -> D -> A
        var graph = new AudioGraph();
        var nodeA = graph.AddNode("A", new nint(1));
        var nodeB = graph.AddNode("B", new nint(2));
        var nodeC = graph.AddNode("C", new nint(3));
        var nodeD = graph.AddNode("D", new nint(4));
        graph.Connect(nodeA, nodeB);
        graph.Connect(nodeB, nodeC);
        graph.Connect(nodeC, nodeD);
        graph.Connect(nodeD, nodeA); // Create cycle

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("cycle", problems[0]);
    }

    /// <summary>
    /// Verifies that a three-node triangle cycle (A -&gt; B -&gt; C -&gt; A) created via
    /// <c>Connect</c> is reported as a cycle problem.
    /// </summary>
    [Fact]
    public void Validate_TriangleCycle_ReturnsCycleError()
    {
        // Arrange - Create a triangle cycle: A -> B -> C -> A
        var graph = new AudioGraph();
        var nodeA = graph.AddNode("A", new nint(1));
        var nodeB = graph.AddNode("B", new nint(2));
        var nodeC = graph.AddNode("C", new nint(3));
        graph.Connect(nodeA, nodeB);
        graph.Connect(nodeB, nodeC);
        graph.Connect(nodeC, nodeA); // Create cycle

        // Act
        var problems = graph.Validate();

        // Assert
        Assert.Single(problems);
        Assert.Contains("cycle", problems[0]);
    }
}
