/// <summary>
/// Represents an audio buffer with interleaved samples for multiple channels.
/// </summary>
public class AudioBuffer : IAudioBuffer
{
    private float[] buffer;
    /// <summary>
    /// Gets the number of channels in the buffer.
    /// </summary>
    public int Channels { get; private set; }
    /// <summary>
    /// Gets the number of frames (samples per channel) in the buffer.
    /// </summary>
    public int Frames { get; private set; }

    /// <summary>
    /// Initializes a new instance of the AudioBuffer class with the specified number of channels and frames.
    /// </summary>
    /// <param name="channels">The number of channels in the buffer. Must be non-negative.</param>
    /// <param name="frames">The number of frames (samples per channel) in the buffer. Must be non-negative.</param>
    public AudioBuffer(int channels, int frames)
    {
        if (channels < 0)
            throw new OverflowException("channels must be non-negative");
        if (frames < 0)
            throw new OverflowException("frames must be non-negative");

        Channels = channels;
        Frames = frames;
        buffer = new float[channels * frames];
    }

    /// <summary>
    /// Sets all samples in the buffer to zero.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0;
        }
    }

    /// <summary>
    /// Copies the samples from another AudioBuffer instance into this buffer.
    /// </summary>
    /// <param name="other">The AudioBuffer to copy from. Must not be null and must have the same dimensions as this buffer.</param>
    public void CopyFrom(AudioBuffer other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (other.Channels != Channels || other.Frames != Frames)
            throw new ArgumentException("AudioBuffer dimensions do not match", nameof(other));

        Array.Copy(other.buffer, buffer, other.buffer.Length);
    }

    /// <summary>
    /// Returns a copy of the buffer as a flat array of floats (interleaved).
    /// </summary>
    /// <returns>A new float array containing a copy of the buffer's samples.</returns>
    public float[] ToFlatArray()
    {
        return (float[])buffer.Clone();
    }

    /// <summary>
    /// Gets a span over the buffer's samples.
    /// </summary>
    /// <returns>A Span<float> that references the buffer's samples.</returns>
    public Span<float> AsSpan() => buffer;

    /// <summary>
    /// Creates a new buffer by interleaving two buffers (appending buffer2 after buffer1).
    /// </summary>
    /// <param name="buffer1">The first buffer to interleave.</param>
    /// <param name="buffer2">The second buffer to interleave. Must have the same number of channels as buffer1.</param>
    /// <returns>A new AudioBuffer containing the samples of buffer1 followed by buffer2.</returns>
    public static AudioBuffer Interleave(AudioBuffer buffer1, AudioBuffer buffer2)
    {
        if (buffer1 == null)
            throw new ArgumentNullException(nameof(buffer1));
        if (buffer2 == null)
            throw new ArgumentNullException(nameof(buffer2));

        if (buffer1.Channels != buffer2.Channels)
            throw new ArgumentException("Channel counts do not match", nameof(buffer1));

        AudioBuffer result = new AudioBuffer(buffer1.Channels, buffer1.Frames + buffer2.Frames);
        int frameOffset = 0;
        for (int i = 0; i < buffer1.Frames; i++)
        {
            Array.Copy(buffer1.buffer, 0, result.buffer, frameOffset, buffer1.Channels);
            frameOffset += buffer1.Channels;
        }
        for (int i = 0; i < buffer2.Frames; i++)
        {
            Array.Copy(buffer2.buffer, 0, result.buffer, frameOffset, buffer2.Channels);
            frameOffset += buffer2.Channels;
        }
        return result;
    }

    /// <summary>
    /// Deinterleaves the buffer into a single-channel buffer where each channel's samples are placed sequentially.
    /// </summary>
    /// <param name="buffer">The buffer to deinterleave. Must have at least two channels.</param>
    /// <returns>A new AudioBuffer with one channel and a frame count equal to (original channels * original frames).</returns>
    public static AudioBuffer Deinterleave(AudioBuffer buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (buffer.Channels < 2)
            throw new ArgumentException("At least two channels required for deinterleaving", nameof(buffer));

        AudioBuffer result = new AudioBuffer(1, buffer.Channels * buffer.Frames);
        for (int i = 0; i < buffer.Frames; i++)
        {
            for (int c = 0; c < buffer.Channels; c++)
            {
                result.buffer[i * buffer.Channels + c] = buffer.buffer[c * buffer.Frames + i];
            }
        }
        return result;
    }

    /// <summary>
    /// Gets or sets the sample at the specified channel and frame.
    /// </summary>
    /// <param name="channel">The zero-based channel index. Must be between 0 and Channels-1.</param>
    /// <param name="frame">The zero-based frame index. Must be between 0 and Frames-1.</param>
    /// <value>The sample at the specified channel and frame.</value>
    public float this[int channel, int frame]
    {
        get
        {
            if (channel < 0 || channel >= Channels)
                throw new IndexOutOfRangeException("Channel index is out of range");
            if (frame < 0 || frame >= Frames)
                throw new IndexOutOfRangeException("Frame index is out of range");

            return buffer[frame * Channels + channel];
        }
        set
        {
            if (channel < 0 || channel >= Channels)
                throw new IndexOutOfRangeException("Channel index is out of range");
            if (frame < 0 || frame >= Frames)
                throw new IndexOutOfRangeException("Frame index is out of range");

            buffer[frame * Channels + channel] = value;
        }
    }
}
