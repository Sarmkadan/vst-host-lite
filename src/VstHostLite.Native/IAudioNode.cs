namespace VstHostLite.Native;

/// <summary>
/// Represents an audio processing node that can be part of an audio graph.
/// Implementations provide audio processing functionality for specific node types
/// like generators, effects, and mixers.
/// </summary>
public interface IAudioNode
{
    /// <summary>
    /// Gets the name of this audio node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Prepares the node for processing with the given sample rate and maximum block size.
    /// </summary>
    /// <param name="sampleRate">The audio sample rate in Hz</param>
    /// <param name="maxBlock">The maximum number of frames per processing block</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if sampleRate or maxBlock is not positive</exception>
    void Prepare(float sampleRate, int maxBlock);

    /// <summary>
    /// Processes audio data through this node.
    /// </summary>
    /// <param name="inputs">Array of input audio buffers (may be empty for generators)</param>
    /// <param name="output">Output audio buffer to write processed result</param>
    /// <exception cref="ArgumentNullException">Thrown if output is null</exception>
    /// <exception cref="ArgumentException">Thrown if input/output dimensions are invalid</exception>
    void Process(in AudioBuffer[] inputs, AudioBuffer output);

    /// <summary>
    /// Resets the internal state of this node (clears buffers, resets phase, etc.)
    /// </summary>
    void Reset();
}
