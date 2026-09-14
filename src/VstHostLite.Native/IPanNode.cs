namespace VstHostLite.Native;

/// <summary>
/// Defines a panning node that can be used in an audio graph.
/// </summary>
public interface IPanNode
{
    /// <summary>
    /// Gets the name of this panning node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the number of audio frames per buffer.
    /// </summary>
    int Frames { get; }

    /// <summary>
    /// Gets or sets the pan position (-1.0 = fully left, 0.0 = center, 1.0 = fully right).
    /// </summary>
    float Pan { get; set; }

    /// <summary>
    /// Processes a mono input buffer and pans it to stereo output using constant-power panning.
    /// </summary>
    /// <param name="monoInput">Mono input audio buffer (must have Frames length)</param>
    /// <param name="left">Left output buffer to write the panned result (must have Frames length)</param>
    /// <param name="right">Right output buffer to write the panned result (must have Frames length)</param>
    /// <exception cref="ArgumentNullException">Thrown if any buffer is null</exception>
    /// <exception cref="ArgumentException">Thrown if buffer lengths don't match</exception>
    void Process(float[] monoInput, float[] left, float[] right);
}