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
