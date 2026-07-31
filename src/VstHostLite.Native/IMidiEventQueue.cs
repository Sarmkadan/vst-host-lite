using System.Collections.Generic;

namespace VstHostLite.Native;

public interface IMidiEventQueue
{
    int Capacity { get; }
    int Count { get; }
    void Enqueue(MidiEvent e);
    void EnqueueRange(IEnumerable<MidiEvent> events);
    MidiEvent[] DequeueUpTo(long sampleOffset);
    void Clear();
    MidiEvent Peek();
}
