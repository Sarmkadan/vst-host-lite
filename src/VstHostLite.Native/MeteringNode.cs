using System;
using System.Linq;

namespace VstHostLite.Native;

/// <summary>
/// A pass‑through node that tracks the running peak and RMS (root‑mean‑square)
/// levels per channel. The node does not modify the audio data; it simply
/// observes it. Call <see cref="Reset"/> to clear the accumulated statistics.
/// The current statistics can be obtained via the <see cref="CurrentMetering"/>
/// property, which returns a <see cref="Metering"/> record.
/// </summary>
public sealed class MeteringNode
{
    private readonly int _channelCount;
    private readonly float[] _peak;          // per‑channel maximum absolute value
    private readonly double[] _sumSquares;   // per‑channel sum of squares for RMS calculation
    private long _sampleCount;               // total number of samples processed per channel

    /// <summary>
    /// Initializes a new instance of <see cref="MeteringNode"/>.
    /// </summary>
    /// <param name="channelCount">Number of interleaved channels the node will process.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="channelCount"/> is less than 1.</exception>
    public MeteringNode(int channelCount)
    {
        if (channelCount < 1)
            throw new ArgumentOutOfRangeException(nameof(channelCount), "Channel count must be at least 1.");

        _channelCount = channelCount;
        _peak = new float[channelCount];
        _sumSquares = new double[channelCount];
        _sampleCount = 0;
    }

    /// <summary>
    /// Processes an interleaved audio buffer, updating peak and RMS statistics.
    /// The buffer is left unchanged.
    /// </summary>
    /// <param name="buffer">Interleaved audio samples (length must be a multiple of the channel count).</param>
    /// <exception cref="ArgumentException">If <paramref name="buffer"/> length is not a multiple of the channel count.</exception>
    public void Process(float[] buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (buffer.Length % _channelCount != 0)
            throw new ArgumentException("Buffer length must be a multiple of the channel count.", nameof(buffer));

        int sampleFrames = buffer.Length / _channelCount;

        for (int frame = 0; frame < sampleFrames; frame++)
        {
            int baseIndex = frame * _channelCount;
            for (int ch = 0; ch < _channelCount; ch++)
            {
                float sample = buffer[baseIndex + ch];
                float abs = Math.Abs(sample);

                // Update peak
                if (abs > _peak[ch])
                    _peak[ch] = abs;

                // Accumulate square for RMS
                _sumSquares[ch] += sample * sample;
            }
        }

        _sampleCount += sampleFrames;
    }

    /// <summary>
    /// Resets all accumulated statistics (peak, RMS and sample count).
    /// </summary>
    public void Reset()
    {
        Array.Clear(_peak, 0, _peak.Length);
        Array.Clear(_sumSquares, 0, _sumSquares.Length);
        _sampleCount = 0;
    }

    /// <summary>
    /// Gets the current metering values as a <see cref="Metering"/> record.
    /// </summary>
    public Metering CurrentMetering => new Metering(
        Peak: _peak.ToArray(),
        RMS: ComputeRms()
    );

    private float[] ComputeRms()
    {
        if (_sampleCount == 0)
            return new float[_channelCount];

        var rms = new float[_channelCount];
        for (int i = 0; i < _channelCount; i++)
        {
            rms[i] = (float)Math.Sqrt(_sumSquares[i] / _sampleCount);
        }
        return rms;
    }
}

/// <summary>
/// Record that holds per‑channel peak and RMS values.
/// </summary>
/// <param name="Peak">Array of peak values (absolute maximum) per channel.</param>
/// <param name="RMS">Array of RMS values per channel.</param>
public readonly record struct Metering(float[] Peak, float[] RMS);
