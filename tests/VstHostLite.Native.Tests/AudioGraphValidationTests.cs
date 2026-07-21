using System;
using System.Reflection;
using Xunit;

namespace VstHostLite.Native.Tests;

public class AudioGraphValidationTests
{
    [Fact]
    public void Validate_ThrowsOnNullGraph()
    {
        // Arrange
        AudioGraph? nullGraph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullGraph!.Validate());
    }

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

    [Fact]
    public void EnsureValid_NullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? nullGraph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullGraph!.EnsureValid());
    }

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
}