using System;

namespace VstHostLite.Native;

/// <summary>
/// A pass-through node that detects silence in audio buffers.
/// Silently passes through audio data while tracking RMS levels.
/// The node considers audio silent when the RMS level falls below the threshold
/// for the specified number of consecutive buffers.
/// </summary>
public sealed class SilenceDetectorNode
{
    private readonly int _channelCount;
    private readonly int _requiredSilentBuffers;
    private readonly float _silenceThreshold;
    private readonly float[] _sumSquares; // per-channel sum of squares for RMS calculation
    private int _silentBufferCount;
    private bool _isSilent;

    /// <summary>
    /// Gets whether the audio is currently considered silent.
    /// </summary>
    public bool IsSilent => _isSilent;

    /// <summary>
    /// Gets the number of consecutive silent buffers detected.
    /// </summary>
    public int SilentBufferCount => _silentBufferCount;

    /// <summary>
    /// Initializes a new instance of <see cref="SilenceDetectorNode"/>.
    /// </summary>
    /// <param name="channelCount">Number of interleaved channels the node will process.</param>
    /// <param name="requiredSilentBuffers">Number of consecutive buffers below threshold required to consider audio silent.</param>
    /// <param name="silenceThreshold">RMS threshold below which audio is considered silent (0.0 to 1.0).</param>
    /// <exception cref="ArgumentOutOfRangeException">When parameters are invalid.</exception>
    public SilenceDetectorNode(int channelCount, int requiredSilentBuffers = 2, float silenceThreshold = 0.0001f)
    {
        if (channelCount < 1)
            throw new ArgumentOutOfRangeException(nameof(channelCount), "Channel count must be at least 1.");

        if (requiredSilentBuffers < 1)
            throw new ArgumentOutOfRangeException(nameof(requiredSilentBuffers), "Required silent buffers must be at least 1.");

        if (silenceThreshold <= 0.0f || silenceThreshold > 1.0f)
            throw new ArgumentOutOfRangeException(nameof(silenceThreshold), "Silence threshold must be between 0.0 and 1.0");

        _channelCount = channelCount;
        _requiredSilentBuffers = requiredSilentBuffers;
        _silenceThreshold = silenceThreshold;
        _sumSquares = new float[channelCount];
        _silentBufferCount = 0;
        _isSilent = false;
    }

    /// <summary>
    /// Processes an audio buffer, updating silence detection statistics.
    /// The buffer is passed through unchanged.
    /// </summary>
    /// <param name="buffer">Interleaved audio samples (length must be a multiple of the channel count).</param>
    /// <exception cref="ArgumentException">If buffer length is not a multiple of the channel count.</exception>
    public void Process(float[] buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        if (buffer.Length % _channelCount != 0)
            throw new ArgumentException("Buffer length must be a multiple of the channel count.", nameof(buffer));

        int sampleFrames = buffer.Length / _channelCount;

        // Calculate RMS for this buffer
        for (int frame = 0; frame < sampleFrames; frame++)
        {
            int baseIndex = frame * _channelCount;
            for (int ch = 0; ch < _channelCount; ch++)
            {
                float sample = buffer[baseIndex + ch];
                _sumSquares[ch] += sample * sample;
            }
        }

        // Check if current buffer is silent
        bool currentBufferSilent = true;
        for (int ch = 0; ch < _channelCount; ch++)
        {
            float rms = (float)Math.Sqrt(_sumSquares[ch] / sampleFrames);
            if (rms > _silenceThreshold)
            {
                currentBufferSilent = false;
                break;
            }
        }

        // Reset sumSquares for next buffer
        Array.Clear(_sumSquares, 0, _sumSquares.Length);

        // Update silent buffer count
        if (currentBufferSilent)
        {
            _silentBufferCount++;
            if (_silentBufferCount >= _requiredSilentBuffers)
            {
                _isSilent = true;
            }
        }
        else
        {
            _silentBufferCount = 0;
            _isSilent = false;
        }
    }

    /// <summary>
    /// Processes an AudioBuffer, updating silence detection statistics.
    /// The buffer is passed through unchanged.
    /// </summary>
    /// <param name="buffer">Audio buffer to process.</param>
    public void Process(AudioBuffer buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        if (buffer.Channels != _channelCount)
            throw new ArgumentException("AudioBuffer channel count does not match SilenceDetectorNode channel count.", nameof(buffer));

        // Calculate RMS for this buffer
        for (int frame = 0; frame < buffer.Frames; frame++)
        {
            for (int ch = 0; ch < _channelCount; ch++)
            {
                float sample = buffer[ch, frame];
                _sumSquares[ch] += sample * sample;
            }
        }

        // Check if current buffer is silent
        bool currentBufferSilent = true;
        for (int ch = 0; ch < _channelCount; ch++)
        {
            float rms = (float)Math.Sqrt(_sumSquares[ch] / buffer.Frames);
            if (rms > _silenceThreshold)
            {
                currentBufferSilent = false;
                break;
            }
        }

        // Reset sumSquares for next buffer
        Array.Clear(_sumSquares, 0, _sumSquares.Length);

        // Update silent buffer count
        if (currentBufferSilent)
        {
            _silentBufferCount++;
            if (_silentBufferCount >= _requiredSilentBuffers)
            {
                _isSilent = true;
            }
        }
        else
        {
            _silentBufferCount = 0;
            _isSilent = false;
        }
    }

    /// <summary>
    /// Resets the silence detection state.
    /// </summary>
    public void Reset()
    {
        _silentBufferCount = 0;
        _isSilent = false;
        Array.Clear(_sumSquares, 0, _sumSquares.Length);
    }
}