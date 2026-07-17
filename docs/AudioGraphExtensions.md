# AudioGraphExtensions

Extension methods for working with `AudioGraph` instances, providing common operations for node management, traversal, and querying.

## API

### `RemoveNode`

Removes the specified node from the graph. If the node is not part of the graph, the operation has no effect.

- **Parameters**
  - `graph`: The `AudioGraph` instance.
  - `node`: The `GraphNode` to remove.
- **Return value**: None.
- **Exceptions**: None.

### `Clear`

Removes all nodes from the graph, leaving it empty.

- **Parameters**
  - `graph`: The `AudioGraph` instance.
- **Return value**: None.
- **Exceptions**: None.

### `GetNodesInOrder`

Returns an enumerable of nodes in the order they would be processed during audio rendering, based on their connections.

- **Parameters**
  - `graph`: The `AudioGraph` instance.
- **Return value**: An `IEnumerable<GraphNode>` containing nodes in processing order.
- **Exceptions**: None.

### `FindNodeByComponent`

Locates a node containing the specified audio component.

- **Parameters**
  - `graph`: The `AudioGraph` instance.
  - `component`: The audio component to search for.
- **Return value**: The `GraphNode` containing the component, or `null` if not found.
- **Exceptions**: None.

### `GetFirstNode`

Gets the first node in the graph, which is the node with no incoming connections (an entry point).

- **Parameters**
  - `graph`: The `AudioGraph` instance.
- **Return value**: The first `GraphNode`, or `null` if the graph is empty.
- **Exceptions**: None.

### `GetLastNode`

Gets the last node in the graph, which is the node with no outgoing connections (an exit point).

- **Parameters**
  - `graph`: The `AudioGraph` instance.
- **Return value**: The last `GraphNode`, or `null` if the graph is empty.
- **Exceptions**: None.

## Usage

```csharp
// Example 1: Removing a node and clearing the graph
var graph = new AudioGraph();
var node1 = graph.AddNode(new SomeAudioComponent());
var node2 = graph.AddNode(new AnotherAudioComponent());

graph.Connect(node1, node2);

// Remove node2 and clear remaining nodes
graph.RemoveNode(node2);
graph.Clear();

// Example 2: Traversing nodes in processing order
var graph = new AudioGraph();
var nodeA = graph.AddNode(new OscillatorComponent());
var nodeB = graph.AddNode(new GainComponent());
var nodeC = graph.AddNode(new OutputComponent());

graph.Connect(nodeA, nodeB);
graph.Connect(nodeB, nodeC);

foreach (var node in graph.GetNodesInOrder())
{
    Console.WriteLine($"Processing node: {node.Component.Name}");
}
```

## Notes

- **Thread safety**: These methods are not thread-safe. Concurrent modifications to the graph while traversing or querying may lead to undefined behavior.
- **Node removal**: Removing a node invalidates any references to it in the graph's internal structures. Subsequent calls to traversal methods will exclude it.
- **Empty graph**: Methods returning `null` or empty enumerables (`GetFirstNode`, `GetLastNode`, `GetNodesInOrder`) handle empty graphs gracefully without throwing.
- **Component uniqueness**: `FindNodeByComponent` assumes each component exists in at most one node. If a component is shared across nodes, the method returns the first match encountered.
