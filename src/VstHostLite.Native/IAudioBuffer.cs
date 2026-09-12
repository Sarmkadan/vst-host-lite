/// <summary>
/// Defines operations for accessing and manipulating a multichannel audio buffer.
/// </summary>
public interface IAudioBuffer
{
    /// <summary>
    /// Gets the number of audio channels in the buffer.
    /// </summary>
    int Channels { get; }

    /// <summary>
    /// Gets the number of audio frames in each channel.
    /// </summary>
    int Frames { get; }

    /// <summary>
    /// Sets every sample in the buffer to zero.
    /// </summary>
    void Clear();

    /// <summary>
    /// Copies the samples from another audio buffer into this buffer.
    /// </summary>
    /// <param name="other">The audio buffer whose samples are copied.</param>
    void CopyFrom(AudioBuffer other);

    /// <summary>
    /// Creates a one-dimensional array containing the samples in this buffer.
    /// </summary>
    /// <returns>A one-dimensional array containing the buffer samples.</returns>
    float[] ToFlatArray();
}
