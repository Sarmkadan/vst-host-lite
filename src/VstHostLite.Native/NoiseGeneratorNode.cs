using System;

namespace VstHostLite.Native;

/// <summary>
/// A node that generates white noise. The noise is generated using a
/// <see cref="System.Random"/> instance seeded either explicitly or with the
/// current time. The amplitude can be set in the range 0.0‑1.0 (inclusive).
/// This node follows the same conventions as <see cref="MixerNode"/> and can
/// be added to an <see cref="AudioGraph"/> via custom handling if required.
/// </summary>
public sealed class NoiseGeneratorNode : INoiseGeneratorNode, IEquatable<NoiseGeneratorNode>
{
    private readonly Random _random;
    private readonly int _frames;
    private float _amplitude;

    /// <summary>
    /// Creates a new <see cref="NoiseGeneratorNode"/>.
    /// </summary>
    /// <param name="name">Node name for identification.</param>
    /// <param name="frames">Number of audio frames per buffer.</param>
    /// <param name="seed">
    /// Optional seed for the random number generator. If <c>null</c>, the
    /// generator is seeded with <c>Environment.TickCount</c>.
    /// </param>
    public NoiseGeneratorNode(string name, int frames, int? seed = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        if (frames <= 0)
            throw new ArgumentOutOfRangeException(nameof(frames), "Frames must be positive.");

        _frames = frames;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _amplitude = NoiseGeneratorNodeConstants.DefaultAmplitude; // default to full amplitude
    }

    /// <summary>
    /// Gets the name of this noise generator node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the amplitude of the generated noise.
    /// Value must be between 0.0 and 1.0 inclusive.
    /// </summary>
    public float Amplitude
    {
        get => _amplitude;
        set
        {
            if (value < NoiseGeneratorNodeConstants.MinAmplitude || 
                value > NoiseGeneratorNodeConstants.MaxAmplitude || 
                float.IsNaN(value) || 
                float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(nameof(value), $"Amplitude must be a finite number in the range [{NoiseGeneratorNodeConstants.MinAmplitude},{NoiseGeneratorNodeConstants.MaxAmplitude}].");
            _amplitude = value;
        }
    }

    /// <summary>
    /// Gets the number of frames per buffer for this node.
    /// </summary>
    public int Frames => _frames;

    public bool Equals(NoiseGeneratorNode? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name &&
               _amplitude == other._amplitude &&
               _frames == other._frames;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as NoiseGeneratorNode);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, _amplitude, _frames);
    }

    public static bool operator ==(NoiseGeneratorNode? left, NoiseGeneratorNode? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(NoiseGeneratorNode? left, NoiseGeneratorNode? right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Generates a block of white noise into the provided output buffer.
    /// </summary>
    /// <param name="output">
    /// Output buffer that will receive the generated samples. Its length must
    /// match the <c>frames</c> value supplied to the constructor.
    /// </param>
    /// <exception cref="ArgumentNullException">If <paramref name="output"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// If <paramref name="output"/> length does not match the configured frame count.
    /// </exception>
    public void Process(float[] output)
    {
        ArgumentNullException.ThrowIfNull(output);

        if (output.Length != _frames)
            throw new ArgumentException($"Output buffer must have {_frames} frames, got {output.Length}.", nameof(output));

        // Generate white noise in the range [-1, 1] and apply amplitude scaling.
        for (int i = 0; i < _frames; i++)
        {
            // Random.NextDouble returns [0.0, 1.0); shift to [-1, 1)
            double sample = (_random.NextDouble() * NoiseGeneratorNodeConstants.NoiseRangeFactor) - NoiseGeneratorNodeConstants.NoiseRangeOffset;
            output[i] = (float)(sample * _amplitude);
        }
    }
}
