namespace VstHostLite.Native;

internal static class PanNodeConstants
{
    /// <summary>
    /// The pan position for the fully left channel.
    /// </summary>
    public const float MinimumPan = -1.0f;

    /// <summary>
    /// The pan position for the center of the stereo field.
    /// </summary>
    public const float CenterPan = 0.0f;

    /// <summary>
    /// The pan position for the fully right channel.
    /// </summary>
    public const float MaximumPan = 1.0f;

    /// <summary>
    /// One quarter of pi, used to calculate constant-power pan gains.
    /// </summary>
    public const float QuarterPi = MathF.PI / 4.0f;
}
