using Xunit;

namespace VstHostLite.Native.Tests;

public class AudioGraphTests
{
    [Fact]
    public void Merge_ThrowsOnNullOtherGraph()
    {
        // Arrange
        var graph = new AudioGraph();
        var node = graph.AddNode("node1", nint.Zero);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.Merge(null!, "prefix_"));
    }

    [Fact]
    public void Merge_ThrowsOnEmptyPrefix()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var graph2 = new AudioGraph();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph1.Merge(graph2, ""));
    }

    [Fact]
    public void Merge_ThrowsOnNullPrefix()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var graph2 = new AudioGraph();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph1.Merge(graph2, null!));
    }

    [Fact]
    public void Merge_ImportsNodesWithPrefixedNames()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var node1 = graph1.AddNode("node1", nint.Zero);
        var node2 = graph1.AddNode("node2", nint.Zero);

        var graph2 = new AudioGraph();
        var node3 = graph2.AddNode("node3", nint.Zero);
        var node4 = graph2.AddNode("node4", nint.Zero);

        // Act
        graph1.Merge(graph2, "prefix_");

        // Assert
        Assert.Equal(4, graph1.Nodes.Count);
        Assert.Contains(graph1.Nodes, n => n.Name == "node1");
        Assert.Contains(graph1.Nodes, n => n.Name == "node2");
        Assert.Contains(graph1.Nodes, n => n.Name == "prefix_node3");
        Assert.Contains(graph1.Nodes, n => n.Name == "prefix_node4");
    }

    [Fact]
    public void Merge_ImportsConnections()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var node1 = graph1.AddNode("node1", nint.Zero);
        var node2 = graph1.AddNode("node2", nint.Zero);
        graph1.Connect(node1, node2);

        var graph2 = new AudioGraph();
        var node3 = graph2.AddNode("node3", nint.Zero);
        var node4 = graph2.AddNode("node4", nint.Zero);
        graph2.Connect(node3, node4);

        // Act
        graph1.Merge(graph2, "prefix_");

        // Assert - connections should be preserved
        var mergedNode1 = graph1.Nodes.First(n => n.Name == "node1");
        var mergedNode2 = graph1.Nodes.First(n => n.Name == "node2");
        var mergedNode3 = graph1.Nodes.First(n => n.Name == "prefix_node3");
        var mergedNode4 = graph1.Nodes.First(n => n.Name == "prefix_node4");

        Assert.Equal(mergedNode2, mergedNode1.Next);
        Assert.Equal(mergedNode4, mergedNode3.Next);
    }

    [Fact]
    public void Merge_ThrowsOnNameCollision()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var node1 = graph1.AddNode("node1", nint.Zero);
        var node2 = graph1.AddNode("prefix_node2", nint.Zero);

        var graph2 = new AudioGraph();
        var node3 = graph2.AddNode("node2", nint.Zero);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => graph1.Merge(graph2, "prefix_"));
        Assert.Contains("Node name collision after prefixing", exception.Message);
        Assert.Contains("prefix_node2", exception.Message);
    }

    [Fact]
    public void Merge_EmptyGraph()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var node1 = graph1.AddNode("node1", nint.Zero);

        var graph2 = new AudioGraph();

        // Act
        graph1.Merge(graph2, "prefix_");

        // Assert
        Assert.Single(graph1.Nodes);
        Assert.Equal("node1", graph1.Nodes[0].Name);
    }

    [Fact]
    public void Merge_ToEmptyGraph()
    {
        // Arrange
        var graph1 = new AudioGraph();

        var graph2 = new AudioGraph();
        var node1 = graph2.AddNode("node1", nint.Zero);
        var node2 = graph2.AddNode("node2", nint.Zero);
        graph2.Connect(node1, node2);

        // Act
        graph1.Merge(graph2, "prefix_");

        // Assert
        Assert.Equal(2, graph1.Nodes.Count);
        Assert.Equal("prefix_node1", graph1.Nodes[0].Name);
        Assert.Equal("prefix_node2", graph1.Nodes[1].Name);
        Assert.Equal(graph1.Nodes[1], graph1.Nodes[0].Next);
    }

    [Fact]
    public void Merge_MultipleNodesWithConnections()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var node1 = graph1.AddNode("node1", nint.Zero);
        var node2 = graph1.AddNode("node2", nint.Zero);
        graph1.Connect(node1, node2);

        var graph2 = new AudioGraph();
        var node3 = graph2.AddNode("node3", nint.Zero);
        var node4 = graph2.AddNode("node4", nint.Zero);
        var node5 = graph2.AddNode("node5", nint.Zero);
        graph2.Connect(node3, node4);
        graph2.Connect(node4, node5);

        // Act
        graph1.Merge(graph2, "chain_");

        // Assert
        Assert.Equal(5, graph1.Nodes.Count);

        var mergedNode1 = graph1.Nodes.First(n => n.Name == "node1");
        var mergedNode2 = graph1.Nodes.First(n => n.Name == "node2");
        var mergedChainNode3 = graph1.Nodes.First(n => n.Name == "chain_node3");
        var mergedChainNode4 = graph1.Nodes.First(n => n.Name == "chain_node4");
        var mergedChainNode5 = graph1.Nodes.First(n => n.Name == "chain_node5");

        // Check graph1 connections
        Assert.Equal(mergedNode2, mergedNode1.Next);

        // Check graph2 connections are preserved
        Assert.Equal(mergedChainNode4, mergedChainNode3.Next);
        Assert.Equal(mergedChainNode5, mergedChainNode4.Next);

        // Check no connections between original and merged graphs
        Assert.Null(mergedNode2.Next);
        Assert.Null(mergedChainNode5.Next);
    }

    [Fact]
    public void Merge_PreservesProcessingOrder()
    {
        // Arrange
        var graph1 = new AudioGraph();
        var node1 = graph1.AddNode("node1", nint.Zero);
        var node2 = graph1.AddNode("node2", nint.Zero);
        graph1.Connect(node1, node2);

        var graph2 = new AudioGraph();
        var node3 = graph2.AddNode("node3", nint.Zero);
        var node4 = graph2.AddNode("node4", nint.Zero);
        graph2.Connect(node3, node4);

        // Act
        graph1.Merge(graph2, "prefix_");

        // Assert
        var processingOrder = graph1.GetProcessingOrder();
        Assert.Equal(4, processingOrder.Count);
        Assert.Equal("node1", processingOrder[0].Name);
        Assert.Equal("node2", processingOrder[1].Name);
        Assert.Equal("prefix_node3", processingOrder[2].Name);
        Assert.Equal("prefix_node4", processingOrder[3].Name);
    }
}
