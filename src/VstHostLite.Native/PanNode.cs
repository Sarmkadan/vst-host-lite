namespace VstHostLite.Native;

/// <summary>
/// A panning node that implements constant-power panning using the cos/sin law.
/// This ensures that the perceived volume remains constant as the pan position changes.
/// Pan value of -1.0 pans fully left, 0.0 pans center, and 1.0 pans fully right.
/// This is a processing node that can be added to the audio graph.
/// </summary>
public sealed class PanNode
{
    private readonly int _frames;
    private float _pan = 0.0f; // -1.0 (left) to 1.0 (right)

    /// <summary>
    /// Creates a new PanNode.
    /// </summary>
    /// <param name="name">Node name for identification</param>
    /// <param name="frames">Number of audio frames per buffer</param>
    public PanNode(string name, int frames)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        if (frames <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frames), "Frames must be positive");
        }

        _frames = frames;
    }

    /// <summary>
    /// Gets the name of this pan node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the pan position (-1.0 = fully left, 0.0 = center, 1.0 = fully right).
    /// </summary>
    public float Pan
    {
        get => _pan;
        set
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Pan must be a valid finite number", nameof(value));
            }

            if (value < -1.0f || value > 1.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Pan must be between -1.0 and 1.0");
            }

            _pan = value;
        }
    }

    /// <summary>
    /// Gets the number of audio frames per buffer.
    /// </summary>
    public int Frames => _frames;

    /// <summary>
    /// Processes a mono input buffer and pans it to stereo output using constant-power panning.
    /// </summary>
    /// <param name="monoInput">Mono input audio buffer (must have Frames length)</param>
    /// <param name="left">Left output buffer to write the panned result (must have Frames length)</param>
    /// <param name="right">Right output buffer to write the panned result (must have Frames length)</param>
    /// <exception cref="ArgumentNullException">Thrown if any buffer is null</exception>
    /// <exception cref="ArgumentException">Thrown if buffer lengths don't match</exception>
    public void Process(float[] monoInput, float[] left, float[] right)
    {
        ArgumentNullException.ThrowIfNull(monoInput);
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        if (monoInput.Length != _frames)
        {
            throw new ArgumentException($"Mono input buffer must have {_frames} frames, got {monoInput.Length}", nameof(monoInput));
        }

        if (left.Length != _frames)
        {
            throw new ArgumentException($"Left output buffer must have {_frames} frames, got {left.Length}", nameof(left));
        }

        if (right.Length != _frames)
        {
            throw new ArgumentException($"Right output buffer must have {_frames} frames, got {right.Length}", nameof(right));
        }

        // Constant-power panning using cos/sin law
        // Left channel: cos(θ) * input
        // Right channel: sin(θ) * input
        // where θ = (π/4) * (1.0 + pan)
        // This ensures constant power across the pan range

        float angle = (MathF.PI / 4.0f) * (1.0f + _pan);
        float cosGain = MathF.Cos(angle);
        float sinGain = MathF.Sin(angle);

        for (int i = 0; i < _frames; i++)
        {
            float sample = monoInput[i];
            left[i] = sample * cosGain;
            right[i] = sample * sinGain;
        }
    }
}
