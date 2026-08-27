using System;
using System.Linq;
using Xunit;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Contains tests for the <see cref="AudioGraph"/> extension methods.
/// </summary>
public class AudioGraphExtensionsTests
{
    /// <summary>
    /// Tests that RemoveNode removes a node from the graph when the node has both previous and next nodes.
    /// </summary>
    [Fact]
    public void RemoveNode_RemovesNodeFromGraph_WhenNodeHasBothPrevAndNext()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.RemoveNode(node2);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.DoesNotContain(node2, graph.Nodes);
        Assert.Equal(node3, node1.Next);
        Assert.Equal(node1, node3.Prev);
        Assert.Null(node2.Prev);
        Assert.Null(node2.Next);
    }

    /// <summary>
    /// Tests that RemoveNode removes a node from the graph when the node has only a previous node.
    /// </summary>
    [Fact]
    public void RemoveNode_RemovesNodeFromGraph_WhenNodeHasOnlyPrev()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        graph.Connect(node1, node2);

        // Act
        graph.RemoveNode(node2);

        // Assert
        Assert.Single(graph.Nodes);
        Assert.DoesNotContain(node2, graph.Nodes);
        Assert.Null(node1.Next);
        Assert.Null(node2.Prev);
        Assert.Null(node2.Next);
    }

    /// <summary>
    /// Tests that RemoveNode removes a node from the graph when the node has only a next node.
    /// </summary>
    [Fact]
    public void RemoveNode_RemovesNodeFromGraph_WhenNodeHasOnlyNext()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        graph.Connect(node1, node2);

        // Act
        graph.RemoveNode(node1);

        // Assert
        Assert.Single(graph.Nodes);
        Assert.DoesNotContain(node1, graph.Nodes);
        Assert.Null(node2.Prev);
        Assert.Null(node1.Prev);
        Assert.Null(node1.Next);
    }

    /// <summary>
    /// Tests that RemoveNode removes the first node from the graph.
    /// </summary>
    [Fact]
    public void RemoveNode_RemovesFirstNodeFromGraph()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.RemoveNode(node1);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.DoesNotContain(node1, graph.Nodes);
        Assert.Equal(node3, node2.Next);
        Assert.Null(node2.Prev);
        Assert.Null(node1.Prev);
        Assert.Null(node1.Next);
    }

    /// <summary>
    /// Tests that RemoveNode removes the last node from the graph.
    /// </summary>
    [Fact]
    public void RemoveNode_RemovesLastNodeFromGraph()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.RemoveNode(node3);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.DoesNotContain(node3, graph.Nodes);
        Assert.Null(node2.Next);
        Assert.Null(node3.Prev);
        Assert.Null(node3.Next);
    }

    /// <summary>
    /// Tests that RemoveNode throws ArgumentNullException when the graph is null.
    /// </summary>
    [Fact]
    public void RemoveNode_ThrowsArgumentNullException_WhenGraphIsNull()
    {
        // Arrange
        AudioGraph? graph = null;
        var node = new GraphNode("test", nint.Zero);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.RemoveNode(node));
    }

    /// <summary>
    /// Tests that RemoveNode throws ArgumentNullException when the node is null.
    /// </summary>
    [Fact]
    public void RemoveNode_ThrowsArgumentNullException_WhenNodeIsNull()
    {
        // Arrange
        var graph = new AudioGraph();
        GraphNode? node = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.RemoveNode(node!));
    }

    /// <summary>
    /// Tests that RemoveNode throws ArgumentException when the node is not part of the graph.
    /// </summary>
    [Fact]
    public void RemoveNode_ThrowsArgumentException_WhenNodeNotInGraph()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = new GraphNode("node2", nint.Zero);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => graph.RemoveNode(node2));
        Assert.Contains("not part of the graph", exception.Message);
    }

    /// <summary>
    /// Tests that Clear removes all nodes from the graph.
    /// </summary>
    [Fact]
    public void Clear_RemovesAllNodesFromGraph()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.Clear();

        // Assert
        Assert.Empty(graph.Nodes);
        Assert.Null(node1.Prev);
        Assert.Null(node1.Next);
        Assert.Null(node2.Prev);
        Assert.Null(node2.Next);
        Assert.Null(node3.Prev);
        Assert.Null(node3.Next);
    }

    /// <summary>
    /// Tests that Clear handles an empty graph correctly.
    /// </summary>
    [Fact]
    public void Clear_HandlesEmptyGraph()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        graph.Clear();

        // Assert
        Assert.Empty(graph.Nodes);
    }

    /// <summary>
    /// Tests that Clear throws ArgumentNullException when the graph is null.
    /// </summary>
    [Fact]
    public void Clear_ThrowsArgumentNullException_WhenGraphIsNull()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.Clear());
    }

    /// <summary>
    /// Tests that GetNodesInOrder returns nodes in sequential order when the graph has multiple nodes.
    /// </summary>
    [Fact]
    public void GetNodesInOrder_ReturnsNodesInSequentialOrder()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var nodesInOrder = graph.GetNodesInOrder();

        // Assert
        Assert.Equal(3, nodesInOrder.Count());
        Assert.Equal(node1, nodesInOrder.ElementAt(0));
        Assert.Equal(node2, nodesInOrder.ElementAt(1));
        Assert.Equal(node3, nodesInOrder.ElementAt(2));
    }

    /// <summary>
    /// Tests that GetNodesInOrder returns a single node when the graph has only one node.
    /// </summary>
    [Fact]
    public void GetNodesInOrder_ReturnsSingleNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);

        // Act
        var nodesInOrder = graph.GetNodesInOrder();

        // Assert
        Assert.Single(nodesInOrder);
        Assert.Equal(node1, nodesInOrder.ElementAt(0));
    }

    [Fact]
    public void GetNodesInOrder_ReturnsEmpty_WhenGraphIsEmpty()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var nodesInOrder = graph.GetNodesInOrder();

        // Assert
        Assert.Empty(nodesInOrder);
    }

    [Fact]
    public void GetNodesInOrder_ThrowsArgumentNullException_WhenGraphIsNull()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.GetNodesInOrder());
    }

    [Fact]
    public void GetNodesInOrder_ReturnsNodesInCorrectOrder_WhenNodesAddedOutOfOrder()
    {
        // Arrange - Add nodes in reverse order
        var graph = new AudioGraph();
        var node3 = graph.AddNode("node3", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node1 = graph.AddNode("node1", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var nodesInOrder = graph.GetNodesInOrder();

        // Assert - Should return nodes in sequential order regardless of insertion order
        Assert.Equal(3, nodesInOrder.Count());
        Assert.Equal(node1, nodesInOrder.ElementAt(0));
        Assert.Equal(node2, nodesInOrder.ElementAt(1));
        Assert.Equal(node3, nodesInOrder.ElementAt(2));
    }

    [Fact]
    public void FindNodeByComponent_ReturnsNode_WhenComponentExists()
    {
        // Arrange
        var graph = new AudioGraph();
        var component1 = new nint(100);
        var component2 = new nint(200);
        var component3 = new nint(300);
        var node1 = graph.AddNode("node1", component1);
        var node2 = graph.AddNode("node2", component2);
        var node3 = graph.AddNode("node3", component3);

        // Act
        var foundNode = graph.FindNodeByComponent(component2);

        // Assert
        Assert.Equal(node2, foundNode);
    }

    [Fact]
    public void FindNodeByComponent_ReturnsNull_WhenComponentNotFound()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", new nint(100));
        var node2 = graph.AddNode("node2", new nint(200));

        // Act
        var foundNode = graph.FindNodeByComponent(new nint(999));

        // Assert
        Assert.Null(foundNode);
    }

    [Fact]
    public void FindNodeByComponent_ReturnsNull_WhenGraphIsEmpty()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var foundNode = graph.FindNodeByComponent(new nint(100));

        // Assert
        Assert.Null(foundNode);
    }

    [Fact]
    public void FindNodeByComponent_ThrowsArgumentNullException_WhenGraphIsNull()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.FindNodeByComponent(nint.Zero));
    }

    [Fact]
    public void GetFirstNode_ReturnsFirstNode_WhenGraphHasMultipleNodes()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var firstNode = graph.GetFirstNode();

        // Assert
        Assert.Equal(node1, firstNode);
    }

    [Fact]
    public void GetFirstNode_ReturnsNode_WhenGraphHasSingleNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);

        // Act
        var firstNode = graph.GetFirstNode();

        // Assert
        Assert.Equal(node1, firstNode);
    }

    [Fact]
    public void GetFirstNode_ReturnsNull_WhenGraphIsEmpty()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var firstNode = graph.GetFirstNode();

        // Assert
        Assert.Null(firstNode);
    }

    [Fact]
    public void GetFirstNode_ThrowsArgumentNullException_WhenGraphIsNull()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.GetFirstNode());
    }

    [Fact]
    public void GetFirstNode_ReturnsNodeWithNoPrev_WhenNodesAddedOutOfOrder()
    {
        // Arrange - Add nodes in reverse order
        var graph = new AudioGraph();
        var node3 = graph.AddNode("node3", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node1 = graph.AddNode("node1", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var firstNode = graph.GetFirstNode();

        // Assert - Should find the node with no previous node
        Assert.Equal(node1, firstNode);
        Assert.Null(firstNode!.Prev);
    }

    [Fact]
    public void GetLastNode_ReturnsLastNode_WhenGraphHasMultipleNodes()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node3 = graph.AddNode("node3", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var lastNode = graph.GetLastNode();

        // Assert
        Assert.Equal(node3, lastNode);
    }

    [Fact]
    public void GetLastNode_ReturnsNode_WhenGraphHasSingleNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = graph.AddNode("node1", nint.Zero);

        // Act
        var lastNode = graph.GetLastNode();

        // Assert
        Assert.Equal(node1, lastNode);
    }

    [Fact]
    public void GetLastNode_ReturnsNull_WhenGraphIsEmpty()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var lastNode = graph.GetLastNode();

        // Assert
        Assert.Null(lastNode);
    }

    [Fact]
    public void GetLastNode_ThrowsArgumentNullException_WhenGraphIsNull()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.GetLastNode());
    }

    [Fact]
    public void GetLastNode_ReturnsNodeWithNoNext_WhenNodesAddedOutOfOrder()
    {
        // Arrange - Add nodes in reverse order
        var graph = new AudioGraph();
        var node3 = graph.AddNode("node3", nint.Zero);
        var node2 = graph.AddNode("node2", nint.Zero);
        var node1 = graph.AddNode("node1", nint.Zero);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var lastNode = graph.GetLastNode();

        // Assert - Should find the node with no next node
        Assert.Equal(node3, lastNode);
        Assert.Null(lastNode!.Next);
    }
}