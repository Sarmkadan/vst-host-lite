namespace VstHostLite.Native;

/// <summary>
/// A sine wave generator node that produces a mono audio signal with configurable frequency, amplitude, sample rate, and phase.
/// This is a processing node that can be added to the audio graph.
/// </summary>
public sealed class SineGeneratorNode
{
    private readonly int _frames;
    private readonly float _sampleRate;
    private float _frequency = 440.0f; // Default: A4 (440 Hz)
    private float _amplitude = 0.5f; // Default: -6dB
    private float _phase = 0.0f; // Phase accumulator in radians
    private float _phaseIncrement = 0.0f;

    /// <summary>
    /// Creates a new SineGeneratorNode.
    /// </summary>
    /// <param name="name">Node name for identification</param>
    /// <param name="sampleRate">Audio sample rate in Hz</param>
    /// <param name="frames">Number of audio frames per buffer</param>
    public SineGeneratorNode(string name, float sampleRate, int frames)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        if (sampleRate <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive");
        }

        if (frames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frames), "Frames must be positive");
        }

        _sampleRate = sampleRate;
        _frames = frames;
        UpdatePhaseIncrement();
    }

    /// <summary>
    /// Gets the name of this sine generator node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the output amplitude (0.0 to 1.0).
    /// </summary>
    public float Amplitude
    {
        get => _amplitude;
        set
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Amplitude must be a valid finite number", nameof(value));
            }

            if (value < 0.0f || value > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Amplitude must be between 0.0 and 1.0");
            }

            _amplitude = value;
        }
    }

    /// <summary>
    /// Gets or sets the output frequency in Hz.
    /// </summary>
    public float Frequency
    {
        get => _frequency;
        set
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Frequency must be a valid finite number", nameof(value));
            }

            if (value < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Frequency must be non-negative");
            }

            _frequency = value;
            UpdatePhaseIncrement();
        }
    }

    /// <summary>
    /// Gets the current sample rate in Hz.
    /// </summary>
    public float SampleRate => _sampleRate;

    /// <summary>
    /// Gets the number of audio frames per buffer.
    /// </summary>
    public int Frames => _frames;

    /// <summary>
    /// Generates a sine wave into the provided mono buffer.
    /// </summary>
    /// <param name="buffer">Output buffer to fill with generated audio (must have Frames length)</param>
    /// <exception cref="ArgumentNullException">Thrown if buffer is null</exception>
    /// <exception cref="ArgumentException">Thrown if buffer length doesn't match Frames</exception>
    public void Generate(float[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (buffer.Length != _frames)
        {
            throw new ArgumentException($"Buffer must have {_frames} frames, got {buffer.Length}", nameof(buffer));
        }

        // Generate sine wave with current parameters
        for (int i = 0; i < _frames; i++)
        {
            buffer[i] = (float)Math.Sin(_phase) * _amplitude;
            _phase += _phaseIncrement;
        }

        // Wrap phase to [-2π, 2π] to maintain precision
        if (_phase > Math.PI * 2.0f)
        {
            _phase -= (float)(Math.PI * 2.0f);
        }
        else if (_phase < -Math.PI * 2.0f)
        {
            _phase += (float)(Math.PI * 2.0f);
        }
    }

    /// <summary>
    /// Resets the phase accumulator to zero.
    /// </summary>
    public void Reset()
    {
        _phase = 0.0f;
    }

    /// <summary>
    /// Updates the phase increment based on current frequency and sample rate.
    /// Phase increment = 2π * frequency / sampleRate
    /// </summary>
    private void UpdatePhaseIncrement()
    {
        _phaseIncrement = (float)(Math.PI * 2.0 * _frequency / _sampleRate);
    }
}
