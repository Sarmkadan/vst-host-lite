## MidiEventQueue

A thread-safe queue that stores MIDI events sorted by sample offset.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        var queue = new MidiEventQueue();
        queue.Enqueue(MidiEvent.NoteOn(60, 100, 0));
        queue.Enqueue(MidiEvent.NoteOff(60, 100, 100));
        var events = queue.DequeueUpTo(100);
        foreach (var e in events)
        {
            Console.WriteLine(e);
        }
    }
}
```

## PluginScanCacheTests

`PluginScanCacheTests` is the xUnit test suite for `PluginScanCache`, exercising the plugin scan cache end to end. Its facts verify that `TryGetFresh` returns false for missing plugins and stale entries, that results saved with `Save` survive a `TryGetFresh` round-trip unchanged, and that `Clear` and `ClearAll` remove the corresponding `.vst3.cache.json` files. Since every fact is a parameterless instance method, they can be invoked individually, and the type also exposes value-style equality via `Equals` and the `==`/`!=` operators.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new PluginScanCacheTests();

        tests.TryGetFresh_ReturnsFalse_WhenPluginDoesNotExist();
        tests.SaveAndTryGetFresh_RoundtripWorks();
        tests.TryGetFresh_ReturnsFalse_WhenCacheIsStale();
        tests.Clear_RemovesCacheFile();
        tests.ClearAll_RemovesAllCacheFiles();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new PluginScanCacheTests());
    }
}
```

## MixerNode

`MixerNode` sums multiple input audio buffers into a single output buffer, applying a per-input gain that can be adjusted at runtime with `SetGain` and read back with `GetGain`. Each node has a read-only `Name` for identification and is constructed with a name, an input count, and a fixed frame count. Call `Process` once per audio cycle with an array of input buffers and an output buffer of matching length to produce the mixed result.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        // Mix 2 inputs of 256 frames each
        var mixer = new MixerNode("master-mixer", inputCount: 2, frames: 256);

        mixer.SetGain(0, 0.8f); // attenuate input 0
        mixer.SetGain(1, 1.0f); // unity gain for input 1

        Console.WriteLine($"{mixer.Name}: input 0 gain = {mixer.GetGain(0)}");

        var inputs = new float[][]
        {
            new float[256], // input 0
            new float[256], // input 1
        };
        var output = new float[256];

        mixer.Process(inputs, output);

        foreach (var sample in output)
        {
            Console.WriteLine(sample);
        }
    }
}
```

## AudioBuffer

`AudioBuffer` holds a fixed-size block of multichannel audio whose shape is described by its read-only `Channels` and `Frames` properties. Samples can be wiped with `Clear`, copied from another buffer with `CopyFrom`, and exported as a single packed `float[]` with `ToFlatArray`. The static `Interleave` and `Deinterleave` factories convert between separate per-channel arrays and packed sample buffers.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        // A stereo buffer with 256 frames per channel.
        var buffer = new AudioBuffer(2, 256);

        Console.WriteLine($"{buffer.Channels} channels x {buffer.Frames} frames");

        // Build two mono channels and pack them into a single buffer.
        var left = new float[buffer.Frames];
        var right = new float[buffer.Frames];
        for (int i = 0; i < buffer.Frames; i++)
        {
            left[i] = MathF.Sin(i * 0.02f);
            right[i] = 0.5f * left[i];
        }

        var interleaved = AudioBuffer.Interleave(new[] { left, right });

        // Export the packed samples, then split them back into channels.
        float[] flat = interleaved.ToFlatArray();
        var restored = AudioBuffer.Deinterleave(flat, interleaved.Channels);

        // Reuse the original buffer: copy the restored samples, then wipe it.
        buffer.CopyFrom(restored);
        buffer.Clear();
    }
}
```

## ClipDetector

The `ClipDetector` static class provides methods to detect clipping in audio buffers. It scans float arrays or `AudioBuffer` instances and returns a `ClipDetectionResult` containing the number of clipped samples, the maximum absolute value found, and the index of the first clipped sample (or -1 if none). The detection threshold is configurable, defaulting to 1.0f.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        // Example 1: Detect clipping in a float array
        float[] samples = { 0.5f, 1.2f, -0.3f, 1.5f };
        var result = ClipDetector.Detect(samples);
        Console.WriteLine($"Clipped: {result.ClippedSampleCount}, Max: {result.MaxAbsoluteValue}, First: {result.FirstClipIndex}");

        // Example 2: Detect clipping in an AudioBuffer
        var buffer = new AudioBuffer(2, 2); // 2 channels, 2 frames
        buffer[0, 0] = 0.5f;
        buffer[1, 0] = 1.1f; // clipped
        buffer[0, 1] = -0.2f;
        buffer[1, 1] = 0.8f;
        var result2 = ClipDetector.Detect(buffer);
        Console.WriteLine(result2.ToString());
    }
}
```

## AudioGraphExtensionsJsonExtensions

`AudioGraphExtensionsJsonExtensions` pairs a lightweight graph descriptor with static JSON helpers for `AudioGraph`. Each instance carries a `Name`, a native `Component` handle, and a mutable `NextIndex` counter, while the static `ToJson`, `FromJson`, and `TryFromJson` members convert audio graphs to and from JSON text. Prefer `TryFromJson` when the input may be malformed, since it reports success through its return value instead of throwing.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        var descriptor = new AudioGraphExtensionsJsonExtensions
        {
            Name = "master-graph",
            Component = 7,
        };

        Console.WriteLine($"{descriptor.Name}: component {descriptor.Component}, next index {descriptor.NextIndex}");

        // Parse a graph from JSON, then serialize it back out.
        var graph = AudioGraphExtensionsJsonExtensions.FromJson("{ \"Name\": \"graph\", \"Component\": 1 }");
        string json = AudioGraphExtensionsJsonExtensions.ToJson(graph);

        if (AudioGraphExtensionsJsonExtensions.TryFromJson(json, out var copy))
        {
            Console.WriteLine("Round-trip succeeded");
        }
    }
}
```

## NativeModuleCache

The `NativeModuleCache` static class provides a thread-safe cache for `NativeModule` instances with reference counting. It ensures that a module loaded from a given path is only loaded once, and multiple callers can acquire the same module. The module is automatically disposed when the last reference is released.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        string modulePath = @"C:\Plugins\MyPlugin.vst3";
        
        // Acquire the module (loads if not cached, increments ref count)
        var module = NativeModuleCache.Acquire(modulePath);
        Console.WriteLine($"Acquired module: {module.Path}");

        // Check the reference count (should be 1)
        int refCount = NativeModuleCache.GetRefCount(modulePath);
        Console.WriteLine($"Reference count: {refCount}");

        // Acquire again to increment the reference count
        var module2 = NativeModuleCache.Acquire(modulePath);
        refCount = NativeModuleCache.GetRefCount(modulePath);
        Console.WriteLine($"Reference count after second acquire: {refCount}");

        // Release one reference
        NativeModuleCache.Release(modulePath, module);
        refCount = NativeModuleCache.GetRefCount(modulePath);
        Console.WriteLine($"Reference count after first release: {refCount}");

        // Release the last reference
        NativeModuleCache.Release(modulePath, module2);
        refCount = NativeModuleCache.GetRefCount(modulePath);
        Console.WriteLine($"Reference count after second release: {refCount}"); // Should be -1 (not cached)

        // Clear the cache (though it's already empty in this example)
        NativeModuleCache.Clear();
    }
}
```

## MixerNodeJsonExtensions

`MixerNodeJsonExtensions` provides JSON serialization and deserialization for `MixerNode` instances. It includes methods to convert a `MixerNode` to a JSON string and to create a `MixerNode` from JSON, with both throwing and non-throwing variants for error handling.

### Example usage:

```csharp
using System;
using VstHostLite.Native;

public class Example
{
    public static void Main()
    {
        // Create a mixer node with 2 inputs and 256 frames
        var mixer = new MixerNode("example-mixer", inputCount: 2, frames: 256);
        mixer.SetGain(0, 0.5f);
        mixer.SetGain(1, 1.5f);

        // Serialize to JSON
        string json = mixer.ToJson();
        Console.WriteLine($"JSON: {json}");

        // Deserialize back
        var deserialized = MixerNodeJsonExtensions.FromJson(json);
        if (deserialized != null)
        {
            Console.WriteLine($"Deserialized name: {deserialized.Name}");
            Console.WriteLine($"Deserialized input 0 gain: {deserialized.GetGain(0)}");
            Console.WriteLine($"Deserialized input 1 gain: {deserialized.GetGain(1)}");
        }

        // Using TryFromJson
        if (MixerNodeJsonExtensions.TryFromJson(json, out var tryResult) && tryResult != null)
        {
            Console.WriteLine("TryFromJson succeeded");
        }
    }
}
```

## AudioGraphTests

`AudioGraphTests` is the xUnit test suite for `AudioGraph`, exercising the core graph operations such as adding nodes, connecting nodes, merging graphs, and removing nodes. Each fact is a parameterless instance method that can be invoked individually to verify specific behavior.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new AudioGraphTests();

        tests.AddNode_AddsNodeToGraph();
        tests.Connect_ConnectsTwoNodes();
        tests.Merge_ImportsNodesWithPrefixedNames();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new AudioGraphTests());
    }
}
```

## AudioGraphValidationTests

`AudioGraphValidationTests` is the xUnit test suite for `AudioGraph.Validate`, `AudioGraph.IsValid`, and `AudioGraph.EnsureValid`. It verifies that the graph validation correctly identifies issues such as null/empty graphs, invalid node names or components, self-references, cycles, and disconnected components. Each test method is a parameterless instance method that can be run individually.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new AudioGraphValidationTests();

        tests.Validate_ThrowsOnNullGraph();
        tests.Validate_EmptyGraph_ReturnsError();
        tests.Validate_SingleNodeWithValidName_ReturnsNoErrors();
        tests.Validate_LinearChain_ReturnsNoErrors();
        tests.Validate_GraphWithCycle_ReturnsCycleError();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new AudioGraphValidationTests());
    }
}
```

## CliArgsTests

`CliArgsTests` is the xUnit test suite for the command-line argument parsing of the VstHostLite application. Each test method verifies the behavior of the CLI for a specific command and argument combination, such as handling missing arguments or unknown commands.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Cli.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual tests directly (each is a parameterless method).
        var tests = new CliArgsTests();

        tests.NoArguments_PrintsUsageAndReturns1();
        tests.UnknownCommand_PrintsUsageAndReturns1();
        tests.InfoCommand_WithPath_ShowsUsageMessage();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new CliArgsTests());
    }
}
```

## Vst3InteropTests

`Vst3InteropTests` is the xUnit test suite for the VST3 interop functionality, covering class counting, class info retrieval, plugin class info handling, and plugin filtering operations. Each test method is a parameterless instance method that can be run individually.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new Vst3InteropTests();

        tests.CountClasses_WithNullFactory_ReturnsZero();
        tests.CountClasses_WithValidFactory_ReturnsNonNegativeCount();
        tests.GetClassInfo_WithNullFactory_ThrowsAccessViolationException();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new Vst3InteropTests());
    }
}
```

## NativeModuleErrorTests

`NativeModuleErrorTests` is the xUnit test suite for error handling and edge cases in the `NativeModule` class. It verifies that loading nonexistent, empty, or whitespace-only paths throws a `FileNotFoundException`, and that the `Dispose` method is safe to call multiple times.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new NativeModuleErrorTests();

        tests.Load_NonexistentPath_ThrowsFileNotFoundException();
        tests.Dispose_MultipleTimes_IsSafe();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new NativeModuleErrorTests());
    }
}
```

## AudioBufferTests

`AudioBufferTests` is the xUnit test suite for `AudioBuffer`, exercising constructor validation, data manipulation, and indexer behavior. Its facts verify correct buffer creation with various parameters, data clearing, copying between buffers, flat array conversion, and indexer access with proper bounds checking. Since every fact is a parameterless instance method, they can be invoked individually.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new AudioBufferTests();

        tests.Constructor_WithZeroChannels_CreatesBufferWithCorrectDimensions();
        tests.Constructor_WithNegativeChannels_ThrowsOverflowException();
        tests.Constructor_WithZeroFrames_CreatesBufferWithCorrectDimensions();
        tests.Constructor_WithNegativeFrames_ThrowsOverflowException();
        tests.Constructor_WithValidParameters_CreatesBufferWithCorrectDimensions();
        tests.Constructor_WithValidParameters_CreatesBufferWithZeroedData();
        tests.Clear_WhenCalled_SetsAllSamplesToZero();
        tests.CopyFrom_WithNullBuffer_ThrowsArgumentNullException();
        tests.CopyFrom_WithDifferentChannels_ThrowsArgumentException();
        tests.CopyFrom_WithDifferentFrames_ThrowsArgumentException();
        tests.CopyFrom_WithSameDimensions_CopiesAllData();
        tests.ToFlatArray_ReturnsCopyOfInternalBuffer();
        tests.Indexer_Get_WithValidIndices_ReturnsCorrectValue();
        tests.Indexer_Get_WithNegativeChannel_ThrowsIndexOutOfRangeException();
        tests.Indexer_Get_WithChannelTooLarge_ThrowsIndexOutOfRangeException();
        tests.Indexer_Get_WithNegativeFrame_ThrowsIndexOutOfRangeException();
        tests.Indexer_Get_WithFrameTooLarge_ThrowsIndexOutOfRangeException();
        tests.Indexer_Set_WithValidIndices_SetsCorrectValue();
        tests.Indexer_Set_WithNegativeChannel_ThrowsIndexOutOfRangeException();
        tests.Indexer_Set_WithChannelTooLarge_ThrowsIndexOutOfRangeException();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new AudioBufferTests());
    }
}
```

## Vst3InteropJsonExtensionsTests

`Vst3InteropJsonExtensionsTests` is the xUnit test suite for the JSON extension methods of `PluginClassInfo`, verifying correct serialization and deserialization behavior, including handling of null, empty, whitespace, and invalid JSON, as well as round-trip preservation of properties. Each test method is a parameterless instance method that can be run individually.

### Example usage:

```csharp
using System;
using VstHostLite.Native;
using VstHostLite.Native.Tests;

public class Example
{
    public static void Main()
    {
        // Run the individual facts directly (each is a parameterless method).
        var tests = new Vst3InteropJsonExtensionsTests();

        tests.ToJson_WithValidPluginClassInfo_ReturnsJsonString();
        tests.FromJson_WithValidJson_ReturnsPluginClassInfo();
        tests.Roundtrip_SerializationDeserialization_PreservesAllProperties();

        // The type also provides value-style equality.
        Console.WriteLine(tests == new Vst3InteropJsonExtensionsTests());
    }
}
```