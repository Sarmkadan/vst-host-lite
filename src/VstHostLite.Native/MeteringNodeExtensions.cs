using System;
using System.Collections.Generic;

namespace VstHostLite.Native;

/// <summary>
/// Provides extension methods for <see cref="MeteringNode"/> to simplify common metering operations.
/// </summary>
public static class MeteringNodeExtensions
{
    /// <summary>
    /// Gets the peak level across all channels as a single normalized value (0.0 to 1.0).
    /// </summary>
    /// <param name="node">The metering node instance.</param>
    /// <returns>The peak level across all channels, or 0.0 if no samples have been processed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static float GetPeakLevel(this MeteringNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var metering = node.CurrentMetering;
        return metering.Peak.Length == 0 ? 0.0f : metering.Peak.Max();
    }

    /// <summary>
    /// Gets the RMS level across all channels as a single normalized value (0.0 to 1.0).
    /// </summary>
    /// <param name="node">The metering node instance.</param>
    /// <returns>The RMS level across all channels, or 0.0 if no samples have been processed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static float GetRmsLevel(this MeteringNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var metering = node.CurrentMetering;
        return metering.RMS.Length == 0 ? 0.0f : metering.RMS.Max();
    }

    /// <summary>
    /// Gets the peak and RMS levels for each channel as a sequence of tuples.
    /// </summary>
    /// <param name="node">The metering node instance.</param>
    /// <returns>A sequence of tuples containing (ChannelIndex, Peak, RMS) for each channel.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static IEnumerable<(int ChannelIndex, float Peak, float RMS)> GetChannelMetering(this MeteringNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var metering = node.CurrentMetering;
        int channelCount = Math.Min(metering.Peak.Length, metering.RMS.Length);

        for (int i = 0; i < channelCount; i++)
        {
            yield return (i, metering.Peak[i], metering.RMS[i]);
        }
    }

    /// <summary>
    /// Gets the highest peak channel index and its value.
    /// </summary>
    /// <param name="node">The metering node instance.</param>
    /// <returns>A tuple containing the channel index with the highest peak and its value, or (-1, 0.0f) if no channels exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="node"/> is null.</exception>
    public static (int ChannelIndex, float Peak) GetPeakChannel(this MeteringNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var metering = node.CurrentMetering;
        if (metering.Peak.Length == 0)
        {
            return (-1, 0.0f);
        }

        int maxIndex = 0;
        float maxValue = metering.Peak[0];

        for (int i = 1; i < metering.Peak.Length; i++)
        {
            if (metering.Peak[i] > maxValue)
            {
                maxIndex = i;
                maxValue = metering.Peak[i];
            }
        }

        return (maxIndex, maxValue);
    }
}
