# vst-host-lite

A minimal VST3 plugin host experiment in C#. The idea was to load a `.vst3`
module, enumerate its plugin classes, instantiate a component and stream audio
through it from the command line - no DAW, no GUI, just the smallest possible
host.

**Status: shelved.** The native-interop plumbing works far enough to inspect a
plugin, but the actual audio routing never came together. Parking it here.

## What works

- Loading a native VST3 module cross-platform (`NativeModule`, via
  `NativeLibrary` + platform-specific module entry/exit points).
- Getting the plugin factory (`GetPluginFactory`) and walking the COM-style
  vtable by hand to call `IPluginFactory::countClasses` / `getClassInfo`.
- Listing classes from the CLI:

  ```
  vsthost info /path/to/SomePlugin.vst3
  ```

  This prints the class count and each class name / category / CID.

## Where it stalled

`audio graph routing not working yet`.

`AudioGraph.ProcessBlock` throws `NotImplementedException` on purpose. The wall
was marshalling the VST3 `ProcessData` / `AudioBusBuffers` structures across the
managed boundary and calling `IAudioProcessor::process` correctly:

- `AudioBusBuffers` is a C++ union of `channelBuffers32` / `channelBuffers64`,
  which are double-indirection pointers (`float**`). Getting the pinning and
  layout right from C# was fiddly and every attempt either returned
  `kResultFalse` or access-violated.
- The `process()` vtable slot index differed between the components I tested, so
  the hand-rolled vtable walk that works for the factory did not transfer.
- I suspect a proper `setupProcessing()` / `ProcessSetup` handshake is required
  first, plus `setActive(true)` on the component, before `process()` is legal.
  Never confirmed.

The honest summary: doing VST3's COM ABI by hand in C# is possible for the
read-only factory calls but turns into a maintenance sink for the real-time
processing path. The right move is probably a small C shim that exposes a flat
C API and P/Invoke that instead - but at that point most of the interesting work
is in C++, which defeated the point of the experiment for me. Shelved.

## Layout

```
VstHostLite.sln
src/
  VstHostLite.Native/   native module loading + VST3 vtable interop + (stub) audio graph
  VstHostLite.Cli/      `vsthost` command-line front end
```

## Build

```
dotnet build
```

Requires the .NET 10 SDK. Only tested on Windows-style single-file `.vst3`
modules; the Linux/macOS bundle layout paths are written but untested.

## AudioGraphJsonExtensions

`AudioGraphJsonExtensions` provides JSON serialization and deserialization utilities for `AudioGraph` instances, allowing you to persist and restore audio processing graphs across application sessions. The extension methods handle conversion to and from JSON format, making it easy to save graph configurations and reload them later.

```csharp
using System;
using VstHostLite.Native;

class Example
{
    static void Main()
    {
        // Create a graph with a node
        var graph = new AudioGraph
        {
            Name = "MyGraph",
            Component = (nint)0x12345678,
            NextIndex = 1
        };

        // Serialize to JSON
        string json = graph.ToJson();
        Console.WriteLine(json);

        // Deserialize from JSON
        var restoredGraph = AudioGraph.FromJson(json);
        Console.WriteLine(restoredGraph.Name);

        // Try to deserialize with error handling
        if (AudioGraph.TryFromJson(json, out var parsedGraph))
        {
            Console.WriteLine($"Successfully parsed graph: {parsedGraph.Name}");
        }
    }
}
```

These methods are defined in `src/VstHostLite.Native/AudioGraphJsonExtensions.cs`.