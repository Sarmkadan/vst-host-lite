using System;
using System.Collections.Concurrent;

public sealed class AudioBufferPool
{
    private readonly ConcurrentQueue<AudioBuffer> _pool;
    private readonly int _channels;
    private readonly int _maxBlockSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioBufferPool"/> class.
    /// </summary>
    /// <param name="channels">The number of channels in the buffers.</param>
    /// <param name="maxBlockSize">The maximum block size of the buffers.</param>
    public AudioBufferPool(int channels, int maxBlockSize)
    {
        ArgumentNullException.ThrowIfNull(nameof(channels) is null || nameof(maxBlockSize) is null);
        ArgumentException.ThrowIfLessThan(nameof(channels), channels, 1);
        ArgumentException.ThrowIfLessThan(nameof(maxBlockSize), maxBlockSize, 1);

        _pool = new ConcurrentQueue<AudioBuffer>();
        _channels = channels;
        _maxBlockSize = maxBlockSize;
    }

    /// <summary>
    /// Rents an audio buffer from the pool.
    /// </summary>
    /// <returns>An audio buffer from the pool.</returns>
    public AudioBuffer Rent()
    {
        if (_pool.TryDequeue(out AudioBuffer buffer))
        {
            return buffer;
        }

        return new AudioBuffer(_channels, _maxBlockSize);
    }

    /// <summary>
    /// Returns an audio buffer to the pool.
    /// </summary>
    /// <param name="buffer">The audio buffer to return.</param>
    public void Return(AudioBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        buffer.Clear();
        _pool.Enqueue(buffer);
    }
}
