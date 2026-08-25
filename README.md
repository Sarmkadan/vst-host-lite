## MidiEventQueue

A thread-safe queue that stores MIDI events sorted by sample offset.

### Example usage:

csharp
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
