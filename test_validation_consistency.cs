using System;
using System.Text.Json;

// Test script to check validation consistency across three paths:
// 1. AudioGraphExtensionsValidation (for extension methods)
// 2. AudioGraphValidation (for graph validation)
// 3. AudioGraphJsonExtensions (JSON deserialization)

var graph = new VstHostLite.Native.AudioGraph();

Console.WriteLine("=== Testing Empty Graph ===");

// Test 1: Empty graph
Console.WriteLine("\n1. AudioGraphExtensionsValidation.Validate(emptyGraph):");
var extValidation = VstHostLite.Native.AudioGraphExtensionsValidation.Validate(graph);
Console.WriteLine($"   Valid: {VstHostLite.Native.AudioGraphExtensionsValidation.IsValid(graph)}");
Console.WriteLine($"   Problems: {extValidation.Count}");

Console.WriteLine("\n2. AudioGraph.Validate(emptyGraph):");
var graphValidation = graph.Validate();
Console.WriteLine($"   Valid: {graph.Validate().IsValid()}");
Console.WriteLine($"   Problems: {graphValidation.Count}");
foreach (var p in graphValidation) Console.WriteLine($"   - {p}");

Console.WriteLine("\n3. AudioGraphJsonExtensions.FromJson(emptyJson):");
try {
    var emptyJson = "{\"Nodes\":[]}";
    var fromJson = VstHostLite.Native.AudioGraphJsonExtensions.FromJson(emptyJson);
    Console.WriteLine($"   Deserialized successfully");
    Console.WriteLine($"   Nodes count: {fromJson.Nodes.Count}");
} catch (Exception ex) {
    Console.WriteLine($"   Error: {ex.GetType().Name}: {ex.Message}");
}

Console.WriteLine("\n=== Testing Cycle Graph ===");

// Test 2: Cycle graph via API
var cycleGraph = new VstHostLite.Native.AudioGraph();
var n1 = cycleGraph.AddNode("node1", new nint(1));
var n2 = cycleGraph.AddNode("node2", new nint(2));
cycleGraph.Connect(n1, n2);
cycleGraph.Connect(n2, n1); // Create cycle

Console.WriteLine("\n1. AudioGraphExtensionsValidation.Validate(cycleGraph):");
var extValidation2 = VstHostLite.Native.AudioGraphExtensionsValidation.Validate(cycleGraph);
Console.WriteLine($"   Valid: {VstHostLite.Native.AudioGraphExtensionsValidation.IsValid(cycleGraph)}");
Console.WriteLine($"   Problems: {extValidation2.Count}");

Console.WriteLine("\n2. AudioGraph.Validate(cycleGraph):");
try {
    cycleGraph.EnsureValid();
    Console.WriteLine("   No exception thrown - validation passed");
} catch (Exception ex) {
    Console.WriteLine($"   Error: {ex.GetType().Name}: {ex.Message}");
}
var graphValidation2 = cycleGraph.Validate();
Console.WriteLine($"   Problems from Validate(): {graphValidation2.Count}");
foreach (var p in graphValidation2) Console.WriteLine($"   - {p}");

// Test 3: Cycle graph via JSON deserialization
Console.WriteLine("\n3. AudioGraphJsonExtensions.FromJson(cycleJson):");
try {
    var cycleJson = @"{
  \"Nodes\": [
    {\"name\":\"node1\",\"component\":\"0x1\",\"nextIndex\":1},
    {\"name\":\"node2\",\"component\":\"0x2\",\"nextIndex\":0}
  ]
}";
    var fromJson = VstHostLite.Native.AudioGraphJsonExtensions.FromJson(cycleJson);
    Console.WriteLine($"   Deserialized successfully");
    Console.WriteLine($"   Nodes count: {fromJson.Nodes.Count}");

    // Try to validate the deserialized graph
    Console.WriteLine("   Validating deserialized graph:");
    var deserializedProblems = fromJson.Validate();
    Console.WriteLine($"   Problems: {deserializedProblems.Count}");
    foreach (var p in deserializedProblems) Console.WriteLine($"   - {p}");

    // Try to use it in processing order (which should throw)
    Console.WriteLine("   Calling GetProcessingOrder():");
    try {
        var order = fromJson.GetProcessingOrder();
        Console.WriteLine($"   Processing order succeeded (unexpected!): {order.Count} nodes");
    } catch (Exception ex) {
        Console.WriteLine($"   Error: {ex.GetType().Name}: {ex.Message}");
    }
} catch (Exception ex) {
    Console.WriteLine($"   Error: {ex.GetType().Name}: {ex.Message}");
}

Console.WriteLine("\n=== Testing Disconnected Nodes ===");

// Test 4: Disconnected nodes via API
var disconnectedGraph = new VstHostLite.Native.AudioGraph();
var dn1 = disconnectedGraph.AddNode("node1", new nint(1));
var dn2 = disconnectedGraph.AddNode("node2", new nint(2));
var dn3 = disconnectedGraph.AddNode("node3", new nint(3));
// Only connect node1 -> node2, node3 is disconnected

disconnectedGraph.Connect(dn1, dn2);

Console.WriteLine("\n1. AudioGraphExtensionsValidation.Validate(disconnectedGraph):");
var extValidation3 = VstHostLite.Native.AudioGraphExtensionsValidation.Validate(disconnectedGraph);
Console.WriteLine($"   Valid: {VstHostLite.Native.AudioGraphExtensionsValidation.IsValid(disconnectedGraph)}");
Console.WriteLine($"   Problems: {extValidation3.Count}");

Console.WriteLine("\n2. AudioGraph.Validate(disconnectedGraph):");
var graphValidation3 = disconnectedGraph.Validate();
Console.WriteLine($"   Problems: {graphValidation3.Count}");
foreach (var p in graphValidation3) Console.WriteLine($"   - {p}");

// Test 5: Disconnected nodes via JSON deserialization
Console.WriteLine("\n3. AudioGraphJsonExtensions.FromJson(disconnectedJson):");
try {
    var disconnectedJson = @"{
  \"Nodes\": [
    {\"name\":\"node1\",\"component\":\"0x1\",\"nextIndex\":1},
    {\"name\":\"node2\",\"component\":\"0x2\",\"nextIndex\":-1},
    {\"name\":\"node3\",\"component\":\"0x3\",\"nextIndex\":-1}
  ]
}";
    var fromJson = VstHostLite.Native.AudioGraphJsonExtensions.FromJson(disconnectedJson);
    Console.WriteLine($"   Deserialized successfully");
    Console.WriteLine($"   Nodes count: {fromJson.Nodes.Count}");

    // Validate the deserialized graph
    Console.WriteLine("   Validating deserialized graph:");
    var deserializedProblems = fromJson.Validate();
    Console.WriteLine($"   Problems: {deserializedProblems.Count}");
    foreach (var p in deserializedProblems) Console.WriteLine($"   - {p}");
} catch (Exception ex) {
    Console.WriteLine($"   Error: {ex.GetType().Name}: {ex.Message}");
}

Console.WriteLine("\n=== Summary ===");
Console.WriteLine("The issue is that AudioGraphExtensionsValidation only checks for null graph,");
Console.WriteLine("while AudioGraphValidation enforces stricter rules (at least one node, no cycles, etc.).");
Console.WriteLine("JSON deserialization doesn't validate at all during deserialization, only during Validate() calls.");