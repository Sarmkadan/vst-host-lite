# MidiEventQueue
`MidiEventQueue` is a simple FIFO container for MIDI events used within the VST host to buffer incoming or outgoing MIDI messages before they are processed by the audio graph or sent to output devices.

## API
### MidiEvent (readonly record struct)
Represents a single MIDI message. The struct is immutable and can be compared with other instances via the `Compare` method.

### NoteOn (static MidiEvent)
Pre‑initialized `MidiEvent` representing a MIDI Note On message (status 0x90, velocity 64, channel 0). Intended for convenience when enqueuing note‑on events.

### NoteOff (static MidiEvent)
Pre‑initialized `MidiEvent` representing a MIDI Note Off message (status 0x80, velocity 0, channel 0). Intended for convenience when enqueuing note‑off events.

### CC (static MidiEvent)
Pre‑initialized `MidiEvent` representing a MIDI Control Change message (status 0xB0, control number 0, value 0, channel 0). Intended for convenience when enqueuing generic control‑change events.

### Enqueue(MidiEvent @event)
Adds a single MIDI event to the end of the queue.  
- **Parameters**  
  - `@event`: The `MidiEvent` to enqueue.  
- **Return value**  
  - None.  
- **Exceptions**  
  - Throws `InvalidOperationException` if the queue is in a state that prevents enqueuing (e.g., after being disposed, if applicable).  
  - Throws if the internal buffer is full (for bounded implementations).

### EnqueueRange(IEnumerable<MidiEvent> events)
Adds a sequence of MIDI events to the end of the queue, preserving their order.  
- **Parameters**  
  - `events`: The events to enqueue.  
- **Return value**  
  - None.  
- **Exceptions**  
  - Throws `ArgumentNullException` if `events` is `null`.  
  - Throws `InvalidOperationException` if the queue cannot accept additional events (e.g., full buffer).  
  - May propagate exceptions thrown by the enumerator.

### DequeueUpTo(int maxCount)
Removes and returns up to `maxCount` events from the front of the queue.  
- **Parameters**  
  - `maxCount`: The maximum number of events to dequeue. Must be non‑negative.  
- **Return value**  
  - An array containing the dequeued events; length may be less than `maxCount` if fewer events are available, or zero if the queue is empty.  
- **Exceptions**  
  - Throws `ArgumentOutOfRangeException` if `maxCount` is negative.  
  - Throws `InvalidOperationException` if the queue is in an invalid state that prevents dequeuing.

### Clear()
Removes all events from the queue.  
- **Parameters**  
  - None.  
- **Return value**  
  - None.  
- **Exceptions**  
  - None under normal operation.

### Peek()
Returns the event at the front of the queue without removing it.  
- **Parameters**  
  - None.  
- **Return value**  
  - The `MidiEvent` at the head of the queue.  
- **Exceptions**  
  - Throws `InvalidOperationException` if the queue is empty.

### Compare(MidiEvent other)
Compares this instance with another `MidiEvent` for ordering (e.g., by timestamp or status).  
- **Parameters**  
  - `other`: The `MidiEvent` to compare against.  
- **Return value**  
  - A negative integer if this instance precedes `other`, zero if they are equal, or a positive integer if this instance follows `other`.  
- **Exceptions**  
  - None.

## Usage
```csharp
var queue = new MidiEventQueue();

// Enqueue a note‑on followed by a note‑off for middle C (MIDI note 60)
queue.Enqueue(MidiEventQueue.NoteOn with { Data1 = 60, Data2 = 100 });
queue.Enqueue(MidiEventQueue.NoteOff with { Data1 = 60 });

// Dequeue up to 2 events and process them
var events = queue.DequeueUpTo(2);
foreach (var ev in events)
{
    // Send ev to MIDI output or feed to synth
}
```

```csharp
var buffer = new[]
{
    new MidiEvent(0xB0, 7, 127), // CC#7 volume max
    new MidiEvent(0x90, 65, 90), // NoteOn F4 vel 90
    new MidiEvent(0x80, 65, 0)   // NoteOff F4
};

var queue = new MidiEventQueue();
queue.EnqueueRange(buffer);

// Inspect the next event without removing it
var next = queue.Peek(); // Expected: CC#7

// Remove all events from the queue
queue.Clear();
```

## Notes
- `Peek` and `DequeueUpTo` both throw `InvalidOperationException` when called on an empty queue; callers should check the queue’s state (e.g., by attempting a `DequeueUpTo(0)` which returns an empty array) or handle the exception.  
- `EnqueueRange` accepts any `IEnumerable<MidiEvent>`; passing `null` results in an `ArgumentNullException`.  
- The queue does not provide built‑in thread‑safety. Concurrent access from multiple threads requires external synchronization (e.g., locking around `Enqueue`, `DequeueUpTo`, `Peek`, and `Clear`).  
- The `Compare` method implements a deterministic ordering based on the internal layout of `MidiEvent` (status byte, data bytes, and any timestamp if present). It is suitable for use with sorting algorithms or sorted collections.  
- After invoking `Clear`, subsequent calls to `Peek` will again throw `InvalidOperationException` until new events are enqueued.  
- The static `NoteOn`, `NoteOff`, and `CC` instances are read‑only; modifying their properties (if the struct exposes setters) will not affect the stored values. Use the `with` expression to create modified copies as shown in the usage examples.
