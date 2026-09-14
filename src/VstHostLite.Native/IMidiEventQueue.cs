using System.Collections.Generic;

namespace VstHostLite.Native;

/// <summary>
/// Represents a queue of MIDI events.
/// </summary>
public interface IMidiEventQueue
{
    /// <summary>
    /// Gets the maximum number of MIDI events that can be stored in the queue.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the current number of MIDI events in the queue.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Adds a MIDI event to the queue.
    /// </summary>
    /// <param name="e">The MIDI event to add.</param>
    void Enqueue(MidiEvent e);

    /// <summary>
    /// Adds a collection of MIDI events to the queue.
    /// </summary>
    /// <param name="events">The collection of MIDI events to add.</param>
    void EnqueueRange(IEnumerable<MidiEvent> events);

    /// <summary>
    /// Removes and returns up to a specified number of MIDI events from the queue.
    /// </summary>
    /// <param name="sampleOffset">The sample offset up to which events should be dequeued.</param>
    /// <returns>An array of MIDI events that were dequeued.</returns>
    MidiEvent[] DequeueUpTo(long sampleOffset);

    /// <summary>
    /// Removes all MIDI events from the queue.
    /// </summary>
    void Clear();

    /// <summary>
    /// Returns the MIDI event at the beginning of the queue without removing it.
    /// </summary>
    /// <returns>The MIDI event at the beginning of the queue.</returns>
    MidiEvent Peek();
}