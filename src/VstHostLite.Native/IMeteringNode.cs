namespace VstHostLite.Native;

/// <summary>
/// Interface for metering node operations.
/// </summary>
public interface IMeteringNode
{
    void Process(float[] buffer);
    void Reset();
    Metering CurrentMetering { get; }
}