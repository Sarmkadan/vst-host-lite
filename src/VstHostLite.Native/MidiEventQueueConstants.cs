using System;

namespace VstHostLite.Native;

/// <summary>
/// Constants for MIDI event processing.
/// </summary>
internal static class MidiEventQueueConstants
{
    /// <summary>
    /// Default maximum capacity of the MIDI event queue.
    /// </>
    public const int DefaultCapacity = 16384;

    /// <summary>
    /// Default overflow policy when the queue is full.
    /// </summary>
    public static readonly MidiEventQueue.OverflowPolicy DefaultOverflowPolicy = MidiEventQueue.OverflowPolicy.DropOldest;

    /// <summary>
    /// Maximum MIDI channel number (0-15).
    /// </summary>
    public const int MaxMidiChannel = 15;

    /// <summary>
    /// Maximum value for MIDI data bytes (0-127).
    /// </summary>
    public const int MaxMidiDataValue = 127;

    /// <summary>
    /// Minimum sample offset for MIDI events.
    /// </summary>
    public const long MinSampleOffset = 0;

    /// <summary>
    /// MIDI status byte for Note On events (channel 0).
    /// </summary>
    public const byte NoteOnStatus = 0x90;

    /// <summary>
    /// MIDI status byte for Note Off events (channel 0).
    /// </summary>
    public const byte NoteOffStatus = 0x80;

    /// <summary>
    /// MIDI status byte for Control Change events (channel 0).
    /// </summary>
    public const byte ControlChangeStatus = 0xB0;

    /// <summary>
    /// Mask to extract the status nibble from a MIDI status byte.
    /// </summary>
    public const byte StatusMask = 0xF0;

    /// <summary>
    /// Mask to extract the channel nibble from a MIDI status byte.
    /// </summary>
    public const byte ChannelMask = 0x0F;
}