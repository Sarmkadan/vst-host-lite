namespace VstHostLite.Native;

/// <summary>
/// Defines a mixer node that can be used in an audio graph.
/// </summary>
public interface IMixerNode
{
    /// <summary>
    /// Gets the name of this mixer node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the number of input channels this mixer expects.
    /// </summary>
    int InputCount { get; }

    /// <summary>
    /// Gets the number of audio frames per buffer.
    /// </summary>
    int Frames { get; }

    /// <summary>
    /// Gets the gain for a specific input channel.
    /// </summary>
    /// <param name="inputIndex">Zero-based input channel index</param>
    /// <returns>The gain value (0.0 to disable, 1.0 = unity, >1.0 = boost)</returns>
    float GetGain(int inputIndex);

    /// <summary>
    /// Sets the gain for a specific input channel.
    /// </summary>
    /// <param name="inputIndex">Zero-based input channel index</param>
    /// <param name="gain">The gain value (0.0 to disable, 1.0 = unity, >1.0 = boost)</param>
    void SetGain(int inputIndex, float gain);

    /// <summary>
    /// Processes audio by summing all input buffers into the output buffer.
    /// </summary>
    /// <param name="inputs">Array of input buffers (one per input channel)</param>
    /// <param name="output">Output buffer to write the mixed result</param>
    /// <exception cref="ArgumentNullException">Thrown if inputs or output is null</exception>
    /// <exception cref="ArgumentException">Thrown if inputs/output dimensions don't match</exception>
    void Process(float[][] inputs, float[] output);
}