namespace VstHostLite.Native;

/// <summary>
/// A delay node that implements a circular-buffer delay line with configurable delay time, feedback, and dry/wet mix.
/// This is a processing node that can be added to the audio graph.
/// </summary>
public sealed class DelayNode
{
    private readonly int _maxDelaySamples;
    private readonly int _frames;
    private readonly float[] _delayBuffer;
    private int _writeIndex;
    private float _feedback = 0.5f;
    private float _dryWetMix = 0.5f;
    private int _delaySamples = 44100 / 4; // Default: 1/4 second at 44.1kHz

    /// <summary>
    /// Creates a new DelayNode.
    /// </summary>
    /// <param name="name">Node name for identification</param>
    /// <param name="maxDelayTimeMs">Maximum delay time in milliseconds</param>
    /// <param name="sampleRate">Audio sample rate in Hz</param>
    /// <param name="frames">Number of audio frames per buffer</param>
    public DelayNode(string name, float maxDelayTimeMs, int sampleRate, int frames)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        if (maxDelayTimeMs <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDelayTimeMs), "Maximum delay time must be positive");
        }

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive");
        }

        if (frames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frames), "Frames must be positive");
        }

        _frames = frames;
        var maxDelaySamples = (int)(maxDelayTimeMs * sampleRate / 1000.0f);
        _maxDelaySamples = Math.Max(1, maxDelaySamples);
        _delayBuffer = new float[_maxDelaySamples];
        _writeIndex = 0;
    }

    /// <summary>
    /// Gets the name of this delay node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the current delay time in samples.
    /// </summary>
    public int DelaySamples
    {
        get => _delaySamples;
        set
        {
            if (value < 0 || value > _maxDelaySamples)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Delay samples must be between 0 and {_maxDelaySamples}");
            }
            _delaySamples = value;
        }
    }

    /// <summary>
    /// Gets or sets the feedback amount (0.0 to 1.0).
    /// </summary>
    public float Feedback
    {
        get => _feedback;
        set
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Feedback must be a valid finite number", nameof(value));
            }

            if (value < 0.0f || value > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    "Feedback must be between 0.0 and 1.0");
            }

            _feedback = value;
        }
    }

    /// <summary>
    /// Gets or sets the dry/wet mix (0.0 = all dry, 1.0 = all wet).
    /// </summary>
    public float DryWetMix
    {
        get => _dryWetMix;
        set
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("DryWetMix must be a valid finite number", nameof(value));
            }

            if (value < 0.0f || value > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    "DryWetMix must be between 0.0 and 1.0");
            }

            _dryWetMix = value;
        }
    }

    /// <summary>
    /// Gets the maximum delay time in samples.
    /// </summary>
    public int MaxDelaySamples => _maxDelaySamples;

    /// <summary>
    /// Processes audio by applying delay effect to the input buffer.
    /// </summary>
    /// <param name="input">Input audio buffer</param>
    /// <param name="output">Output audio buffer to write the processed result</param>
    /// <exception cref="ArgumentNullException">Thrown if input or output is null</exception>
    /// <exception cref="ArgumentException">Thrown if input/output dimensions don't match</exception>
    public void Process(float[] input, float[] output)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        // Validate buffer lengths
        if (input.Length != _frames)
        {
            throw new ArgumentException($"Input buffer must have {_frames} frames, got {input.Length}", nameof(input));
        }

        if (output.Length != _frames)
        {
            throw new ArgumentException($"Output buffer must have {_frames} frames, got {output.Length}", nameof(output));
        }

        // Clear output buffer
        Array.Clear(output, 0, output.Length);

        // Apply delay effect
        for (int i = 0; i < _frames; i++)
        {
            // Read delayed sample from circular buffer
            int readIndex = (_writeIndex - _delaySamples + _maxDelaySamples) % _maxDelaySamples;
            float delayedSample = _delayBuffer[readIndex];

            // Calculate output sample with feedback
            float outputSample = input[i] + (delayedSample * _feedback);

            // Store in delay buffer (with feedback)
            _delayBuffer[_writeIndex] = outputSample;
            _writeIndex = (_writeIndex + 1) % _maxDelaySamples;

            // Apply dry/wet mix
            output[i] = input[i] * (1.0f - _dryWetMix) + outputSample * _dryWetMix;
        }
    }

    /// <summary>
    /// Resets the delay buffer (clears all delay memory).
    /// </summary>
    public void Reset()
    {
        Array.Clear(_delayBuffer, 0, _delayBuffer.Length);
        _writeIndex = 0;
    }
}
