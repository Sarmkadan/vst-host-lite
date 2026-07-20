namespace VstHostLite.Native;

/// <summary>
/// Represents a MIDI event with status, data1, data2, and sample offset.
/// </summary>
/// <param name="Status">The MIDI status byte (0x80-0xEF)</param>
/// <param name="Data1">First data byte (note number for note events, controller number for CC)</param>
/// <param name="Data2">Second data byte (velocity for note events, value for CC)</param>
/// <param name="SampleOffset">The sample offset at which this event should be processed</param>
public readonly record struct MidiEvent(
    byte Status,
    byte Data1,
    byte Data2,
    long SampleOffset)
{
    /// <summary>
    /// Creates a Note On MIDI event.
    /// </summary>
    /// <param name="note">The MIDI note number (0-127)</param>
    /// <param name="velocity">The note velocity (0-127)</param>
    /// <param name="sampleOffset">The sample offset at which this event should be processed</param>
    /// <returns>A new MidiEvent with Note On status</returns>
    public static MidiEvent NoteOn(byte note, byte velocity, long sampleOffset)
    {
        return new MidiEvent((byte)(0x90 | 0), note, velocity, sampleOffset);
    }

    /// <summary>
    /// Creates a Note Off MIDI event.
    /// </summary>
    /// <param name="note">The MIDI note number (0-127)</param>
    /// <param name="velocity">The note release velocity (0-127)</param>
    /// <param name="sampleOffset">The sample offset at which this event should be processed</param>
    /// <returns>A new MidiEvent with Note Off status</returns>
    public static MidiEvent NoteOff(byte note, byte velocity, long sampleOffset)
    {
        return new MidiEvent((byte)(0x80 | 0), note, velocity, sampleOffset);
    }

    /// <summary>
    /// Creates a Control Change (CC) MIDI event.
    /// </summary>
    /// <param name="controller">The controller number (0-127)</param>
    /// <param name="value">The controller value (0-127)</param>
    /// <param name="sampleOffset">The sample offset at which this event should be processed</param>
    /// <returns>A new MidiEvent with Control Change status</returns>
    public static MidiEvent CC(byte controller, byte value, long sampleOffset)
    {
        return new MidiEvent(0xB0, controller, value, sampleOffset);
    }

    /// <summary>
    /// Gets whether this is a Note On event.
    /// </summary>
    public bool IsNoteOn => (Status & 0xF0) == 0x90;

    /// <summary>
    /// Gets whether this is a Note Off event.
    /// </summary>
    public bool IsNoteOff => (Status & 0xF0) == 0x80;

    /// <summary>
    /// Gets whether this is a Control Change event.
    /// </summary>
    public bool IsControlChange => (Status & 0xF0) == 0xB0;

    /// <summary>
    /// Gets the channel from the status byte (0-15).
    /// </summary>
    public byte Channel => (byte)(Status & 0x0F);
}

/// <summary>
/// A thread-safe queue that stores MIDI events sorted by sample offset.
/// Events can be dequeued up to a specific sample offset.
/// </summary>
public sealed class MidiEventQueue
{
    private readonly List<MidiEvent> _events = [];
    private readonly object _lock = new();

    /// <summary>
    /// Gets the number of events currently in the queue.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _events.Count;
            }
        }
    }

    /// <summary>
    /// Enqueues a single MIDI event.
    /// </summary>
    /// <param name="e">The MIDI event to enqueue</param>
    public void Enqueue(MidiEvent e)
    {
        lock (_lock)
        {
            // Insert the event in sorted order by sample offset
            int index = _events.BinarySearch(e, MidiEventComparer.Instance);
            if (index < 0)
            {
                index = ~index;
            }
            _events.Insert(index, e);
        }
    }

    /// <summary>
    /// Enqueues multiple MIDI events.
    /// </summary>
    /// <param name="events">The MIDI events to enqueue</param>
    public void EnqueueRange(IEnumerable<MidiEvent> events)
    {
        if (events is null)
        {
            return;
        }

        lock (_lock)
        {
            foreach (var e in events)
            {
                Enqueue(e);
            }
        }
    }

    /// <summary>
    /// Dequeues all events with sample offset less than or equal to the specified offset.
    /// </summary>
    /// <param name="sampleOffset">The sample offset to dequeue up to (inclusive)</param>
    /// <returns>An array of events that should be processed</returns>
    public MidiEvent[] DequeueUpTo(long sampleOffset)
    {
        lock (_lock)
        {
            if (_events.Count == 0 || _events[0].SampleOffset > sampleOffset)
            {
                return [];
            }

            // Find the index where events exceed the sample offset
            int count = 0;
            while (count < _events.Count && _events[count].SampleOffset <= sampleOffset)
            {
                count++;
            }

            var result = new MidiEvent[count];
            _events.CopyTo(0, result, 0, count);
            _events.RemoveRange(0, count);
            return result;
        }
    }

    /// <summary>
    /// Clears all events from the queue.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _events.Clear();
        }
    }

    /// <summary>
    /// Gets the first event in the queue without removing it.
    /// </summary>
    /// <returns>The first event, or default if queue is empty</returns>
    public MidiEvent Peek()
    {
        lock (_lock)
        {
            return _events.Count > 0 ? _events[0] : default;
        }
    }

    /// <summary>
    /// Compares MidiEvent instances by their SampleOffset for sorting.
    /// </summary>
    private sealed class MidiEventComparer : IComparer<MidiEvent>
    {
        public static readonly MidiEventComparer Instance = new();

        public int Compare(MidiEvent x, MidiEvent y)
        {
            return x.SampleOffset.CompareTo(y.SampleOffset);
        }
    }
}
