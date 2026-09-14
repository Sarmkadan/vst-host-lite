namespace VstHostLite.Native;

/// <summary>
/// Interface for metering node operations.
/// </summary>
public interface IMeteringNode
{
    /// <summary>
    /// Processes the specified buffer.
    /// </summary>
    /// <param name="buffer">The buffer to process.</param>
    void Process(float[] buffer);

    /// <summary>
    /// Resets the metering node to its initial state.
    /// </summary>
    void Reset();

    /// <summary>
    /// Gets the current metering.
    /// </summary>
    Metering CurrentMetering { get; }
}