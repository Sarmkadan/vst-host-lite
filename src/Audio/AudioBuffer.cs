using System;

public sealed class AudioBuffer
{
    private readonly float[] _backingArray;
    public int FrameCount { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioBuffer"/> class.
    /// </summary>
    /// <param name="channels">The number of channels in the buffer.</param>
    /// <param name="maxBlockSize">The maximum block size of the buffer.</param>
    public AudioBuffer(int channels, int maxBlockSize)
    {
        ArgumentNullException.ThrowIfNull(nameof(channels) is null || nameof(maxBlockSize) is null);
        ArgumentException.ThrowIfLessThan(nameof(channels), channels, 1);
        ArgumentException.ThrowIfLessThan(nameof(maxBlockSize), maxBlockSize, 1);

        _backingArray = new float[channels * maxBlockSize];
    }

    /// <summary>
    /// Gets a span of floats representing the specified channel.
    /// </summary>
    /// <param name="channel">The channel to get the span for.</param>
    /// <returns>A span of floats representing the specified channel.</returns>
    public Span<float> GetChannel(int channel)
    {
        ArgumentNullException.ThrowIfNull(nameof(channel) is null);
        ArgumentException.ThrowIfLessThan(nameof(channel), channel, 0);
        ArgumentException.ThrowIfGreaterThanOrEqualTo(nameof(channel), channel, _backingArray.Length / FrameCount);

        return _backingArray.AsSpan(channel * FrameCount, FrameCount);
    }

    /// <summary>
    /// Sets the frame count of the buffer.
    /// </summary>
    /// <param name="frameCount">The new frame count.</param>
    public void SetFrameCount(int frameCount)
    {
        ArgumentNullException.ThrowIfNull(nameof(frameCount) is null);
        ArgumentException.ThrowIfLessThan(nameof(frameCount), frameCount, 0);
        ArgumentException.ThrowIfGreaterThanOrEqualTo(nameof(frameCount), frameCount, _backingArray.Length / GetChannelCount());

        FrameCount = frameCount;
    }

    /// <summary>
    /// Gets the number of channels in the buffer.
    /// </summary>
    /// <returns>The number of channels in the buffer.</returns>
    public int GetChannelCount()
    {
        return _backingArray.Length / GetMaxBlockSize();
    }

    /// <summary>
    /// Gets the maximum block size of the buffer.
    /// </summary>
    /// <returns>The maximum block size of the buffer.</returns>
    public int GetMaxBlockSize()
    {
        return _backingArray.Length / GetChannelCount();
    }

    /// <summary>
    /// Clears the buffer by zeroing the active frames.
    /// </summary>
    public void Clear()
    {
        Span<float> span = _backingArray.AsSpan(0, FrameCount * GetChannelCount());
        span.Clear();
    }
}
