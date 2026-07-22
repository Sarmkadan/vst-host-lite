using System;
using System.Threading;

public sealed class MidiEventQueue
{
    private const int Capacity = 1024;
    private readonly object[] _events = new object[Capacity];
    private volatile int _head;
    private volatile int _tail;

    public bool Enqueue(MidiEvent @event)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        int tail = Interlocked.Read(ref _tail);
        if (tail == _head)
        {
            // Queue is full, reject new events
            return false;
        }

        int index = (tail + 1) % Capacity;
        _events[index] = @event;
        Interlocked.CompareExchange(ref _tail, index, tail);
        return true;
    }

    public bool TryDequeue(out MidiEvent @event)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        int head = Interlocked.Read(ref _head);
        int tail = Interlocked.Read(ref _tail);
        if (head == tail)
        {
            // Queue is empty, return false
            @event = null;
            return false;
        }

        int index = (head + 1) % Capacity;
        @event = _events[index];
        Interlocked.CompareExchange(ref _head, index, head);
        return true;
    }
}
