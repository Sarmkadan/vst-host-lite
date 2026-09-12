namespace VstHostLite.Native;

/// <summary>
/// Defines an audio processing node that can participate in an audio graph.
/// </summary>
/// <remarks>
/// Implementations can represent generators, effects, mixers, or other audio-processing components.
/// </remarks>
public interface IAudioNode
{
    /// <summary>
    /// Gets the name of this audio node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Prepares the node to process audio at the specified sample rate and maximum block size.
    /// </summary>
    /// <param name="sampleRate">The audio sample rate, in hertz.</param>
    /// <param name="maxBlock">The maximum number of frames in a processing block.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="sampleRate"/> or <paramref name="maxBlock"/> is not positive.
    /// </exception>
    void Prepare(float sampleRate, int maxBlock);

    /// <summary>
    /// Processes a block of audio through the node.
    /// </summary>
    /// <param name="inputs">
    /// The input audio buffers. This array can be empty for nodes that generate audio.
    /// </param>
    /// <param name="output">The audio buffer to which the processed block is written.</param>
    /// <exception cref="ArgumentNullException"><paramref name="output"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The input or output buffer dimensions are invalid.</exception>
    void Process(in AudioBuffer[] inputs, AudioBuffer output);

    /// <summary>
    /// Resets the node's internal processing state.
    /// </summary>
    void Reset();
}
