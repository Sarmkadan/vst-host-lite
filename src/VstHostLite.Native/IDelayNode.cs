namespace VstHostLite.Native;

/// <summary>
/// Defines a delay node that can be used in an audio graph.
/// </summary>
public interface IDelayNode
{
    /// <summary>
    /// Gets the name of this delay node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets the current delay time in samples.
    /// </summary>
    int DelaySamples { get; set; }

    /// <summary>
    /// Gets or sets the feedback amount (0.0 to 1.0).
    /// </summary>
    float Feedback { get; set; }

    /// <summary>
    /// Gets or sets the dry/wet mix (0.0 = all dry, 1.0 = all wet).
    /// </summary>
    float DryWetMix { get; set; }

    /// <summary>
    /// Gets the maximum delay time in samples.
    /// </summary>
    int MaxDelaySamples { get; }

    /// <summary>
    /// Processes audio by applying delay effect to the input buffer.
    /// </summary>
    /// <param name="input">Input audio buffer</param>
    /// <param name="output">Output audio buffer to write the processed result</param>
    /// <exception cref="ArgumentNullException">Thrown if input or output is null</exception>
    /// <exception cref="ArgumentException">Thrown if input/output dimensions don't match</exception>
    void Process(float[] input, float[] output);

    /// <summary>
    /// Resets the delay buffer (clears all delay memory).
    /// </summary>
    void Reset();
}