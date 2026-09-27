using System;

namespace VstHostLite.Native;

/// <summary>
/// Extension methods for <see cref="MeteringNode"/> that provide read-only utility operations.
/// </summary>
public static class MeteringNodeUtilityExtensions
{
    /// <summary>
    /// Gets the maximum peak value across all channels.
    /// </summary>
    /// <param name="node">The metering node to query.</param>
    /// <returns>The maximum peak value across all channels, or 0 if no channels.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="node"/> is <see langword="null"/>.</exception>
    public static float GetMaxPeak(this MeteringNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var metering = node.CurrentMetering;
        if (metering.Peak.Length == 0)
            return 0f;

        float max = metering.Peak[0];
        for (int i = 1; i < metering.Peak.Length; i++)
        {
            if (metering.Peak[i] > max)
                max = metering.Peak[i];
        }

        return max;
    }
}