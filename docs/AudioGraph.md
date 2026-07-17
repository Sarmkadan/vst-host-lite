# AudioGraph

A lightweight in-memory graph for routing audio between VST3 plug-in nodes. It tracks plug-in instances (`GraphNode`) and their signal connections, and drives processing in topological order. The graph is mutable at setup time and read-only during processing.

## API

### `GraphNode AddNode(nint component)`

Creates and returns a new node that wraps the supplied VST3 component handle (`component`).
- **Parameters**
  - `component` – non-zero handle returned by the plug-in factory or loader.
- **Return Value**
  - The newly created `GraphNode` instance.
- **Exceptions**
  - Throws `ArgumentException` if `component` is zero or already present in the graph.

---

### `void Connect(GraphNode from, GraphNode to)`

Establishes a directed audio connection from `from` to `to`.
- **Parameters**
  - `from` – source node.
  - `to` – destination node.
- **Exceptions**
  - Throws `ArgumentException` if either node is absent from the graph.
  - Throws `InvalidOperationException` if the connection would create a cycle.

---

### `void ProcessBlock(ReadOnlySpan<float> input, Span<float> output)`

Runs one audio block through the graph.
- **Parameters**
  - `input` – interleaved input samples (must match the host’s configured channel count).
  - `output` – interleaved output buffer (must match the host’s configured channel count).
- **Exceptions**
  - Throws `ArgumentException` if `input` or `output` lengths are incorrect.
  - Throws `InvalidOperationException` if the graph is not fully connected or contains cycles.

---

### `GraphNode public string Name { get; }`

Gets the human-readable name of the node, derived from the plug-in’s `IPluginBase` information.

---

### `public nint Component { get; }`

Gets the underlying VST3 component handle (`IComponent*`) associated with this node.

---
### `public GraphNode? Prev { get; }`

Gets the single preceding node in the signal chain, or `null` if this node is a head.

---
### `public GraphNode? Next { get; }`

Gets the single succeeding node in the signal chain, or `null` if this node is a tail.

## Usage

```csharp
// 1) Build a simple two-node graph
var loader = new Vst3Loader("path/to/plugin.vst3");
var factory = loader.GetFactory();
var component = factory.CreateInstance("MyPlugin");
var graph = new AudioGraph();

var inputNode = graph.AddNode(component);
var outputNode = graph.AddNode(factory.CreateInstance("MyOutput"));
graph.Connect(inputNode, outputNode);

var input = new float[1024];          // interleaved stereo
var output = new float[1024];
graph.ProcessBlock(input, output);
```

```csharp
// 2) Chain three nodes with a bypassable middle node
var synth = graph.AddNode(loader.CreateInstance("Synth"));
var fx = graph.AddNode(loader.CreateInstance("Delay"));
var outNode = graph.AddNode(loader.CreateInstance("Out"));
graph.Connect(synth, fx);
graph.Connect(fx, outNode);

// Later, swap the middle node for a dry/wet bypass
var dryWet = graph.AddNode(loader.CreateInstance("DryWet"));
graph.Disconnect(synth, fx);   // hypothetical helper
graph.Disconnect(fx, outNode);
graph.Connect(synth, dryWet);
graph.Connect(dryWet, outNode);
```

## Notes

- **Thread Safety**: The graph is not thread-safe; all public members must be called from a single thread. Once `ProcessBlock` begins, the graph is considered immutable until the next call to `ProcessBlock` completes.
- **Cycle Detection**: `Connect` performs a quick cycle check; large graphs may incur noticeable overhead.
- **Buffer Sizes**: `ProcessBlock` assumes fixed-size I/O buffers; mismatched sizes throw immediately.
- **Ownership**: The graph does **not** take ownership of the component handles; the caller must release them after the graph is disposed.
