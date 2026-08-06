using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
        return new MidiEvent(MidiEventQueueConstants.NoteOnStatus, note, velocity, sampleOffset);
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
        return new MidiEvent(MidiEventQueueConstants.NoteOffStatus, note, velocity, sampleOffset);
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
        return new MidiEvent(MidiEventQueueConstants.ControlChangeStatus, controller, value, sampleOffset);
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
/// <remarks>
/// This queue includes bounds checking and DoS protection to prevent memory exhaustion
/// from untrusted MIDI sources or malformed plugin callbacks.
/// </remarks>
public sealed class MidiEventQueue : IMidiEventQueue, IEquatable<MidiEventQueue>
{
    // Maximum capacity to prevent memory exhaustion from event floods
    // Default of 16384 events (~128KB) balances memory usage with DoS protection
    private const int DefaultCapacity = 16384;

    // Overflow policy: when the queue is full, drop oldest events to make room
    private const OverflowPolicy DefaultOverflowPolicy = OverflowPolicy.DropOldest;

    private readonly List<MidiEvent> _events = [];
    private readonly object _lock = new();
    private readonly OverflowPolicy _overflowPolicy;

    /// <summary>
    /// Gets the maximum capacity of the queue.
    /// </summary>
    public int Capacity { get; }

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
    /// Initializes a new instance of the <see cref="MidiEventQueue"/> class.
    /// </summary>
    /// <param name="capacity">Maximum number of events the queue can hold. Must be positive.</param>
    /// <param name="overflowPolicy">Policy for handling overflow when the queue is full.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is not positive.</exception>
    public MidiEventQueue(int capacity = DefaultCapacity, OverflowPolicy overflowPolicy = DefaultOverflowPolicy)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(capacity, 0);

        Capacity = capacity;
        _overflowPolicy = overflowPolicy;
    }

    /// <summary>
    /// Enqueues a single MIDI event after validating its fields.
    /// </summary>
    /// <param name="e">The MIDI event to enqueue.</param>
    /// <exception cref="ArgumentNullException">Thrown when event is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when event fields are out of valid range.</exception>
    public void Enqueue(MidiEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);

        ValidateMidiEvent(e);

        lock (_lock)
        {
            // Apply overflow policy if queue is full
            if (_events.Count >= Capacity)
            {
                switch (_overflowPolicy)
                {
                    case OverflowPolicy.DropOldest:
                        _events.RemoveAt(0);
                        break;
                    case OverflowPolicy.DropNewest:
                        return; // Drop the new event
                    case OverflowPolicy.Throw:
                        throw new InvalidOperationException($"MIDI event queue is full (capacity: {Capacity}).");
                    default:
                        throw new InvalidOperationException($"Unknown overflow policy: {_overflowPolicy}");
                }
            }

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
    /// Enqueues multiple MIDI events after validating each one.
    /// </summary>
    /// <param name="events">The MIDI events to enqueue.</param>
    /// <exception cref="ArgumentNullException">Thrown when events is null.</exception>
    public void EnqueueRange(IEnumerable<MidiEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

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
    /// Validates that a MIDI event's fields are within their valid ranges.
    /// </summary>
    /// <param name="e">The event to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when event fields are invalid.</exception>
    private static void ValidateMidiEvent(MidiEvent e)
    {
        // MIDI channel: 0-15 (16 channels total)
        // Status byte contains channel in lower 4 bits
        if (e.Channel > MidiEventQueueConstants.MaxMidiChannel)
        {
            throw new ArgumentOutOfRangeException(
                nameof(e),
                $"MIDI channel must be between 0 and {MidiEventQueueConstants.MaxMidiChannel}, got {e.Channel}.");
        }

        // Data1 contains note number for note events, controller number for CC events
        // Must be 0-127 (7-bit MIDI data)
        if (e.Data1 > MidiEventQueueConstants.MaxMidiDataValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(e.Data1),
                $"MIDI data1 must be between 0 and {MidiEventQueueConstants.MaxMidiDataValue}, got {e.Data1}.");
        }

        // Data2 contains velocity for note events, value for CC events
        // Must be 0-127 (7-bit MIDI data)
        if (e.Data2 > MidiEventQueueConstants.MaxMidiDataValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(e.Data2),
                $"MIDI data2 must be between 0 and {MidiEventQueueConstants.MaxMidiDataValue}, got {e.Data2}.");
        }

        // SampleOffset must be non-negative
        if (e.SampleOffset < MidiEventQueueConstants.MinSampleOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(e.SampleOffset),
                $"MIDI event sample offset must be non-negative, got {e.SampleOffset}.");
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

    /// <summary>
    /// Policy for handling queue overflow when capacity is reached.
    /// </summary>
    public enum OverflowPolicy
    {
        /// <summary>
        /// Drop the oldest event to make room for the new one.
        /// This ensures newer events are prioritized and prevents memory exhaustion.
        /// </summary>
        DropOldest,

        /// <summary>
        /// Drop the newest event (do not enqueue it).
        /// This preserves existing events and rejects new ones when full.
        /// </summary>
        DropNewest,

        /// <summary>
        /// Throw an exception when the queue is full.
        /// This alerts the caller to the overflow condition for explicit handling.
        /// </summary>
        Throw
    }

    // ------------------------------------------------------------------------
    // Equality members
    // ------------------------------------------------------------------------

    /// <summary>
    /// Determines whether this instance is equal to another <see cref="MidiEventQueue"/>.
    /// Equality is based on <see cref="Capacity"/>, the configured <see cref="OverflowPolicy"/>,
    /// and the sequence of queued <see cref="MidiEvent"/> items.
    /// </summary>
    public bool Equals(MidiEventQueue? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        // Lock both queues in a deterministic order to avoid deadlocks.
        // Use the hash code of the objects to decide lock order.
        var first = this;
        var second = other;
        if (RuntimeHelpers.GetHashCode(first) > RuntimeHelpers.GetHashCode(second))
        {
            (first, second) = (second, first);
        }

        lock (first._lock)
        {
            lock (second._lock)
            {
                if (Capacity != other.Capacity)
                    return false;
                if (_overflowPolicy != other._overflowPolicy)
                    return false;
                if (_events.Count != other._events.Count)
                    return false;
                return _events.SequenceEqual(other._events);
            }
        }
    }

    public override bool Equals(object? obj) => obj is MidiEventQueue other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Capacity);
        hash.Add(_overflowPolicy);
        foreach (var ev in _events)
        {
            hash.Add(ev);
        }
        return hash.ToHashCode();
    }

    public static bool operator ==(MidiEventQueue? left, MidiEventQueue? right) =>
        EqualityComparer<MidiEventQueue>.Default.Equals(left, right);

    public static bool operator !=(MidiEventQueue? left, MidiEventQueue? right) => !(left == right);
}
