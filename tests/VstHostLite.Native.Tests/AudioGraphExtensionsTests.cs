using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VstHostLite.Native.Tests;

public class AudioGraphExtensionsTests
{
    [Fact]
    public void RemoveNode_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? graph = null;
        var node = new GraphNode("test", 1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.RemoveNode(node));
    }

    [Fact]
    public void RemoveNode_WithNullNode_ThrowsArgumentNullException()
    {
        // Arrange
        var graph = new AudioGraph();
        GraphNode? node = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.RemoveNode(node!));
    }

    [Fact]
    public void RemoveNode_WithNodeNotInGraph_ThrowsArgumentException()
    {
        // Arrange
        var graph = new AudioGraph();
        var nodeInGraph = new GraphNode("node1", 1);
        graph.AddNode(nodeInGraph);
        var nodeNotInGraph = new GraphNode("node2", 2);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => graph.RemoveNode(nodeNotInGraph));
        Assert.Equal("The specified node is not part of the graph. (Parameter 'node')", exception.Message);
    }

    [Fact]
    public void RemoveNode_WithSingleNode_RemovesNodeAndClearsConnections()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode("single", 1);
        graph.AddNode(node);

        // Act
        graph.RemoveNode(node);

        // Assert
        Assert.Empty(graph.Nodes);
        Assert.Null(node.Prev);
        Assert.Null(node.Next);
    }

    [Fact]
    public void RemoveNode_WithFirstNodeInChain_UpdatesNextPointer()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.RemoveNode(node1);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Equal(node2, graph.Nodes[0]);
        Assert.Equal(node3, graph.Nodes[1]);
        Assert.Null(node2.Prev);
        Assert.Equal(node3, node2.Next);
        Assert.Null(node3.Next);
    }

    [Fact]
    public void RemoveNode_WithMiddleNodeInChain_UpdatesPrevAndNextPointers()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.RemoveNode(node2);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Equal(node1, graph.Nodes[0]);
        Assert.Equal(node3, graph.Nodes[1]);
        Assert.Equal(node3, node1.Next);
        Assert.Equal(node1, node3.Prev);
        Assert.Null(node2.Prev);
        Assert.Null(node2.Next);
    }

    [Fact]
    public void RemoveNode_WithLastNodeInChain_UpdatesPrevPointer()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        graph.RemoveNode(node3);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Equal(node1, graph.Nodes[0]);
        Assert.Equal(node2, graph.Nodes[1]);
        Assert.Null(node2.Next);
        Assert.Equal(node2, node1.Next);
        Assert.Null(node3.Prev);
        Assert.Null(node3.Next);
    }

    [Fact]
    public void Clear_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.Clear());
    }

    [Fact]
    public void Clear_WithEmptyGraph_DoesNotThrow()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        graph.Clear();

        // Assert
        Assert.Empty(graph.Nodes);
    }

    [Fact]
    public void Clear_WithMultipleNodes_ClearsAllNodesAndConnections()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
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

    [Fact]
    public void GetNodesInOrder_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.GetNodesInOrder());
    }

    [Fact]
    public void GetNodesInOrder_WithEmptyGraph_ReturnsEmptyEnumerable()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var result = graph.GetNodesInOrder();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetNodesInOrder_WithSingleNode_ReturnsSingleNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode("single", 1);
        graph.AddNode(node);

        // Act
        var result = graph.GetNodesInOrder();

        // Assert
        Assert.Single(result);
        Assert.Equal(node, result.First());
    }

    [Fact]
    public void GetNodesInOrder_WithLinearChain_ReturnsNodesInOrder()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var result = graph.GetNodesInOrder();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Equal(node1, result.ElementAt(0));
        Assert.Equal(node2, result.ElementAt(1));
        Assert.Equal(node3, result.ElementAt(2));
    }

    [Fact]
    public void GetNodesInOrder_WithDisconnectedNodes_ReturnsFirstNodeOnly()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        // No connections - all nodes are disconnected

        // Act
        var result = graph.GetNodesInOrder();

        // Assert
        // GetNodesInOrder starts from Nodes[0] and walks backward (Prev) to find the first node,
        // then walks forward (Next). With disconnected nodes, all have Prev=null and Next=null,
        // so it only returns the starting node (Nodes[0])
        Assert.Single(result);
        Assert.Equal(node1, result.First());
    }

    [Fact]
    public void FindNodeByComponent_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? graph = null;
        var component = new nint(1);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.FindNodeByComponent(component));
    }

    [Fact]
    public void FindNodeByComponent_WithEmptyGraph_ReturnsNull()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var result = graph.FindNodeByComponent(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindNodeByComponent_WithExistingComponent_ReturnsNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);

        // Act
        var result = graph.FindNodeByComponent(2);

        // Assert
        Assert.Equal(node2, result);
    }

    [Fact]
    public void FindNodeByComponent_WithNonExistingComponent_ReturnsNull()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        graph.AddNode(node1);
        graph.AddNode(node2);

        // Act
        var result = graph.FindNodeByComponent(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindNodeByComponent_WithMultipleNodesWithSameComponent_ReturnsFirstMatch()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 1); // Same component
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);

        // Act
        var result = graph.FindNodeByComponent(1);

        // Assert
        Assert.Equal(node1, result); // Should return first match
    }

    [Fact]
    public void GetFirstNode_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.GetFirstNode());
    }

    [Fact]
    public void GetFirstNode_WithEmptyGraph_ReturnsNull()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var result = graph.GetFirstNode();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFirstNode_WithSingleNode_ReturnsThatNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode("single", 1);
        graph.AddNode(node);

        // Act
        var result = graph.GetFirstNode();

        // Assert
        Assert.Equal(node, result);
    }

    [Fact]
    public void GetFirstNode_WithLinearChain_ReturnsFirstNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var result = graph.GetFirstNode();

        // Assert
        Assert.Equal(node1, result);
    }

    [Fact]
    public void GetFirstNode_WithLinearChain_ReturnsFirstNodeInInsertionOrder()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var result = graph.GetFirstNode();

        // Assert
        Assert.Equal(node1, result);
    }

    [Fact]
    public void GetLastNode_WithNullGraph_ThrowsArgumentNullException()
    {
        // Arrange
        AudioGraph? graph = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph!.GetLastNode());
    }

    [Fact]
    public void GetLastNode_WithEmptyGraph_ReturnsNull()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var result = graph.GetLastNode();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetLastNode_WithSingleNode_ReturnsThatNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = new GraphNode("single", 1);
        graph.AddNode(node);

        // Act
        var result = graph.GetLastNode();

        // Assert
        Assert.Equal(node, result);
    }

    [Fact]
    public void GetLastNode_WithLinearChain_ReturnsLastNode()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var result = graph.GetLastNode();

        // Assert
        Assert.Equal(node3, result);
    }

    [Fact]
    public void GetLastNode_WithLinearChain_ReturnsLastNodeInInsertionOrder()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act
        var result = graph.GetLastNode();

        // Assert
        Assert.Equal(node3, result);
    }

    [Fact]
    public void RemoveNode_AfterGetNodesInOrder_StillWorksCorrectly()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Get nodes in order first
        var nodesInOrder = graph.GetNodesInOrder().ToList();
        Assert.Equal(3, nodesInOrder.Count);

        // Act - remove middle node
        graph.RemoveNode(node2);

        // Assert
        Assert.Equal(2, graph.Nodes.Count);
        Assert.Equal(node1, graph.Nodes[0]);
        Assert.Equal(node3, graph.Nodes[1]);
    }

    [Fact]
    public void GetNodesInOrder_AfterRemoveNode_ReturnsCorrectOrder()
    {
        // Arrange
        var graph = new AudioGraph();
        var node1 = new GraphNode("first", 1);
        var node2 = new GraphNode("second", 2);
        var node3 = new GraphNode("third", 3);
        graph.AddNode(node1);
        graph.AddNode(node2);
        graph.AddNode(node3);
        graph.Connect(node1, node2);
        graph.Connect(node2, node3);

        // Act - remove middle node
        graph.RemoveNode(node2);

        // Get nodes in order after removal
        var result = graph.GetNodesInOrder();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal(node1, result.ElementAt(0));
        Assert.Equal(node3, result.ElementAt(1));
    }
}