namespace VstHostLite.Native;

/// <summary>
/// Provides validation helpers for <see cref="MidiEventQueue"/> instances.
/// </summary>
public static class MidiEventQueueValidation
{
    /// <summary>
    /// Validates the specified MIDI event queue.
    /// </summary>
    /// <param name="value">The MIDI event queue to validate</param>
    /// <returns>A list of validation problems; empty if the queue is valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this MidiEventQueue? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate internal state
        lock (value.GetLock())
        {
            // Check that the internal list is not null
            if (value.GetEvents() is null)
            {
                problems.Add("Internal events list is null");
            }

            // Validate each event in the queue
            foreach (var e in value.GetEvents())
            {
                ValidateEvent(e, problems);
            }

            // Check that events are properly sorted by sample offset
            if (value.GetEvents() is not null)
            {
                for (int i = 1; i < value.GetEvents().Count; i++)
                {
                    if (value.GetEvents()[i - 1].SampleOffset > value.GetEvents()[i].SampleOffset)
                    {
                        problems.Add($"Events at index {i - 1} and {i} are not sorted by sample offset");
                    }
                }
            }
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified MIDI event queue is valid.
    /// </summary>
    /// <param name="value">The MIDI event queue to check</param>
    /// <returns><see langword="true"/> if the queue is valid; otherwise, <see langword="false"/></returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static bool IsValid(this MidiEventQueue? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified MIDI event queue is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The MIDI event queue to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown if the queue contains validation problems</exception>
    public static void EnsureValid(this MidiEventQueue? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"MidiEventQueue is invalid:{Environment.NewLine}  - {
                    string.Join($"{Environment.NewLine}  - ", problems)
                }");
        }
    }

    private static void ValidateEvent(MidiEvent e, List<string> problems)
    {
        // Validate Status byte (0x80-0xEF for MIDI events)
        if (e.Status < 0x80 || e.Status > 0xEF)
        {
            problems.Add($"Event has invalid status byte {e.Status} (expected 0x80-0xEF)");
        }

        // Validate Data1 based on event type
        if (e.IsNoteOn || e.IsNoteOff)
        {
            // Note number (0-127)
            if (e.Data1 > 127)
            {
                problems.Add($"Note event has invalid note number {e.Data1} (expected 0-127)");
            }
        }
        else if (e.IsControlChange)
        {
            // Controller number (0-127)
            if (e.Data1 > 127)
            {
                problems.Add($"Control Change event has invalid controller number {e.Data1} (expected 0-127)");
            }
        }

        // Validate Data2 (velocity for notes, value for CC, both 0-127)
        if (e.Data2 > 127)
        {
            problems.Add($"Event has invalid data2 value {e.Data2} (expected 0-127)");
        }

        // Validate SampleOffset (must be non-negative)
        if (e.SampleOffset < 0)
        {
            problems.Add($"Event has negative sample offset {e.SampleOffset} (expected >= 0)");
        }
    }

    // Reflection-based access to private members for validation
    // This is necessary since MidiEventQueue has private fields we need to validate
    private static object GetLock(this MidiEventQueue queue)
    {
        var field = typeof(MidiEventQueue).GetField("_lock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(queue) ?? throw new InvalidOperationException("Could not access _lock field");
    }

    private static List<MidiEvent> GetEvents(this MidiEventQueue queue)
    {
        var field = typeof(MidiEventQueue).GetField("_events", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(queue) as List<MidiEvent> ?? throw new InvalidOperationException("Could not access _events field");
    }
}