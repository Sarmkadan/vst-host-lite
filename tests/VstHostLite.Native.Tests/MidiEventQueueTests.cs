using System;
using System.Collections.Generic;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class MidiEventQueueTests
{
    [Fact]
    public void MidiEvent_FactoryMethods_CreateCorrectValues()
    {
        var noteOn = MidiEvent.NoteOn(60, 100, 123);
        Assert.Equal(0x90, noteOn.Status);
        Assert.Equal(60, noteOn.Data1);
        Assert.Equal(100, noteOn.Data2);
        Assert.Equal(123, noteOn.SampleOffset);
        Assert.True(noteOn.IsNoteOn);
        Assert.False(noteOn.IsNoteOff);
        Assert.False(noteOn.IsControlChange);
        Assert.Equal(0, noteOn.Channel);

        var noteOff = MidiEvent.NoteOff(61, 0, 124);
        Assert.Equal(0x80, noteOff.Status);
        Assert.Equal(61, noteOff.Data1);
        Assert.Equal(0, noteOff.Data2);
        Assert.Equal(124, noteOff.SampleOffset);
        Assert.True(noteOff.IsNoteOff);
        Assert.False(noteOff.IsNoteOn);
        Assert.False(noteOff.IsControlChange);
        Assert.Equal(0, noteOff.Channel);

        var cc = MidiEvent.CC(7, 127, 125);
        Assert.Equal(0xB0, cc.Status);
        Assert.Equal(7, cc.Data1);
        Assert.Equal(127, cc.Data2);
        Assert.Equal(125, cc.SampleOffset);
        Assert.True(cc.IsControlChange);
        Assert.False(cc.IsNoteOn);
        Assert.False(cc.IsNoteOff);
        Assert.Equal(0, cc.Channel);
    }

    [Fact]
    public void Enqueue_InsertsInSortedOrder()
    {
        var queue = new MidiEventQueue();

        // Insert out of order
        queue.Enqueue(MidiEvent.NoteOn(60, 100, 300));
        queue.Enqueue(MidiEvent.NoteOn(61, 100, 100));
        queue.Enqueue(MidiEvent.NoteOn(62, 100, 200));

        // Peek should give the smallest offset (100)
        var first = queue.Peek();
        Assert.Equal(100, first.SampleOffset);
        Assert.Equal(0x90, first.Status);
        Assert.Equal(61, first.Data1);
    }

    [Fact]
    public void EnqueueRange_NullOrEmpty_DoesNotThrowOrChangeQueue()
    {
        var queue = new MidiEventQueue();

        // Null range – should be a no‑op
        queue.EnqueueRange(null);
        Assert.Equal(0, queue.Count);

        // Empty range – also a no‑op
        queue.EnqueueRange(Array.Empty<MidiEvent>());
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    public void DequeueUpTo_ReturnsCorrectSubsetAndRemovesThem()
    {
        var queue = new MidiEventQueue();

        queue.Enqueue(MidiEvent.NoteOn(60, 100, 10));
        queue.Enqueue(MidiEvent.NoteOn(61, 100, 20));
        queue.Enqueue(MidiEvent.NoteOn(62, 100, 30));

        // Dequeue up to offset 20 (inclusive) – should get first two events
        var result = queue.DequeueUpTo(20);
        Assert.Equal(2, result.Length);
        Assert.Equal(10, result[0].SampleOffset);
        Assert.Equal(20, result[1].SampleOffset);

        // Remaining count should be 1 (the event at offset 30)
        Assert.Equal(1, queue.Count);
        Assert.Equal(30, queue.Peek().SampleOffset);
    }

    [Fact]
    public void DequeueUpTo_NoMatchingEvents_ReturnsEmptyArray()
    {
        var queue = new MidiEventQueue();

        queue.Enqueue(MidiEvent.NoteOn(60, 100, 50));
        var result = queue.DequeueUpTo(40); // offset before any event
        Assert.Empty(result);
        Assert.Equal(1, queue.Count); // original event still present
    }

    [Fact]
    public void Peek_EmptyQueue_ReturnsDefaultMidiEvent()
    {
        var queue = new MidiEventQueue();

        var peeked = queue.Peek();
        Assert.Equal(default(MidiEvent), peeked);
    }

    [Fact]
    public void Clear_EmptiesQueue()
    {
        var queue = new MidiEventQueue();

        queue.Enqueue(MidiEvent.NoteOn(60, 100, 10));
        queue.Enqueue(MidiEvent.NoteOff(60, 0, 20));

        Assert.Equal(2, queue.Count);
        queue.Clear();
        Assert.Equal(0, queue.Count);
        Assert.Empty(queue.DequeueUpTo(long.MaxValue));
    }
}
