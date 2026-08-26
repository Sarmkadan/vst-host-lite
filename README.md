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
