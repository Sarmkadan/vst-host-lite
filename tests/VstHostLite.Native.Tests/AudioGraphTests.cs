using System;
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

 [Fact]
 public void AddNode_AddsNodeToGraph()
 {
  // Arrange
  var graph = new AudioGraph();

  // Act
  var node = graph.AddNode("testNode", nint.Zero);

  // Assert
  Assert.Single(graph.Nodes);
  Assert.Equal("testNode", node.Name);
  Assert.Equal(nint.Zero, node.Component);
  Assert.Null(node.Prev);
  Assert.Null(node.Next);
 }

 [Fact]
 public void AddNode_AddsMultipleNodes()
 {
  // Arrange
  var graph = new AudioGraph();

  // Act
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = graph.AddNode("node2", nint.Zero);
  var node3 = graph.AddNode("node3", nint.Zero);

  // Assert
  Assert.Equal(3, graph.Nodes.Count);
  Assert.Equal("node1", graph.Nodes[0].Name);
  Assert.Equal("node2", graph.Nodes[1].Name);
  Assert.Equal("node3", graph.Nodes[2].Name);
 }

 [Fact]
 public void AddNode_WithRawMethod()
 {
  // Arrange
  var graph = new AudioGraph();
  var componentPtr = new nint(1);
  var node = new GraphNode("rawNode", componentPtr);

  // Act
  graph.AddNode(node);

  // Assert
  Assert.Single(graph.Nodes);
  Assert.Equal("rawNode", node.Name);
  Assert.Equal(componentPtr, node.Component);
 }

 [Fact]
 public void AddNode_AllowsDuplicateNames()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("duplicate", nint.Zero);

  // Act
  var componentPtr = new nint(1);
  var node2 = new GraphNode("duplicate", componentPtr);
  graph.AddNode(node2);

  // Assert - AudioGraph allows duplicate names
  Assert.Equal(2, graph.Nodes.Count);
  Assert.Equal("duplicate", node1.Name);
  Assert.Equal("duplicate", node2.Name);
 }

 [Fact]
 public void Connect_ConnectsTwoNodes()
 {
  // Arrange
  var graph = new AudioGraph();
  var from = graph.AddNode("fromNode", nint.Zero);
  var to = graph.AddNode("toNode", nint.Zero);

  // Act
  graph.Connect(from, to);

  // Assert
  Assert.Equal(to, from.Next);
  Assert.Equal(from, to.Prev);
 }

 [Fact]
 public void Connect_MultipleConnections()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = graph.AddNode("node2", nint.Zero);
  var node3 = graph.AddNode("node3", nint.Zero);

  // Act
  graph.Connect(node1, node2);
  graph.Connect(node2, node3);

  // Assert
  Assert.Equal(node2, node1.Next);
  Assert.Equal(node1, node2.Prev);
  Assert.Equal(node3, node2.Next);
  Assert.Equal(node2, node3.Prev);
 }

 [Fact]
 public void Connect_OverwritesExistingConnection()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = graph.AddNode("node2", nint.Zero);
  var node3 = graph.AddNode("node3", nint.Zero);
  graph.Connect(node1, node2);

  // Act - connect node1 to node3 instead
  graph.Connect(node1, node3);

  // Assert - Connect doesn't clear old connections, just overwrites Next/Pprev
  Assert.Equal(node3, node1.Next);
  Assert.Equal(node1, node3.Prev);
  // Old connections remain but are no longer part of the chain
  Assert.Equal(node2, node1.Next); // This is the new connection
 }

 [Fact]
 public void RemoveNode_RemovesNodeFromGraph()
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
  // After removing node2, node1 should connect to node3
  Assert.Equal(node3, node1.Next);
  Assert.Equal(node1, node3.Prev);
  // The removed node's connections should be cleared
  Assert.Null(node2.Prev);
  Assert.Null(node2.Next);
 }

 [Fact]
 public void RemoveNode_WithPrevAndNext_CleansConnections()
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

  // Assert - node1 should now connect to node3
  Assert.Equal(node3, node1.Next);
  Assert.Equal(node1, node3.Prev);
  Assert.Null(node2.Prev);
  Assert.Null(node2.Next);
 }

 [Fact]
 public void RemoveNode_WithOnlyPrev_CleansConnections()
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
  Assert.Null(node1.Next); // node1 has no next since node2 was removed
  Assert.Null(node2.Prev);
  Assert.Null(node2.Next);
  Assert.Null(node1.Prev); // node1 should have no prev
 }

 [Fact]
 public void RemoveNode_WithOnlyNext_CleansConnections()
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
  Assert.Null(node2.Prev); // node2 has no prev since node1 was removed
  Assert.Null(node1.Prev);
  Assert.Null(node1.Next);
 }


 [Fact]
 public void RemoveNode_ThrowsOnNullNode()
 {
  // Arrange
  var graph = new AudioGraph();

  // Act & Assert
  Assert.Throws<ArgumentNullException>(() => graph.RemoveNode(null!));
 }

 [Fact]
 public void RemoveNode_ThrowsOnNodeNotInGraph()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = new GraphNode("node2", nint.Zero);

  // Act & Assert
  var exception = Assert.Throws<ArgumentException>(() => graph.RemoveNode(node2));
  Assert.Contains("not part of the graph", exception.Message);
 }

 [Fact]
 public void GetProcessingOrder_ReturnsNodesInTopologicalOrder()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = graph.AddNode("node2", nint.Zero);
  var node3 = graph.AddNode("node3", nint.Zero);
  graph.Connect(node1, node2);
  graph.Connect(node2, node3);

  // Act
  var processingOrder = graph.GetProcessingOrder();

  // Assert
  Assert.Equal(3, processingOrder.Count);
  Assert.Equal(node1, processingOrder[0]);
  Assert.Equal(node2, processingOrder[1]);
  Assert.Equal(node3, processingOrder[2]);
 }

 [Fact]
 public void GetProcessingOrder_ReturnsSingleNode()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);

  // Act
  var processingOrder = graph.GetProcessingOrder();

  // Assert
  Assert.Single(processingOrder);
  Assert.Equal(node1, processingOrder[0]);
 }

 [Fact]
 public void GetProcessingOrder_EmptyGraph()
 {
  // Arrange
  var graph = new AudioGraph();

  // Act
  var processingOrder = graph.GetProcessingOrder();

  // Assert
  Assert.Empty(processingOrder);
 }

 [Fact]
 public void GetProcessingOrderIds_ReturnsCorrectIndices()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = graph.AddNode("node2", nint.Zero);
  var node3 = graph.AddNode("node3", nint.Zero);
  graph.Connect(node1, node2);
  graph.Connect(node2, node3);

  // Act
  var orderIds = graph.GetProcessingOrderIds();

  // Assert
  Assert.Equal(3, orderIds.Count);
  Assert.Equal(0, orderIds[0]);
  Assert.Equal(1, orderIds[1]);
  Assert.Equal(2, orderIds[2]);
 }

 [Fact]
 public void GetProcessingOrder_HandlesCycleDetection()
 {
  // Arrange
  var graph = new AudioGraph();
  var node1 = graph.AddNode("node1", nint.Zero);
  var node2 = graph.AddNode("node2", nint.Zero);
  graph.Connect(node1, node2);
  graph.Connect(node2, node1); // Create cycle

  // Act & Assert
  var exception = Assert.Throws<InvalidOperationException>(() => graph.GetProcessingOrder());
  Assert.Contains("cycle", exception.Message);
 }
}
