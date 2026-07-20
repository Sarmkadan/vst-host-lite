namespace VstHostLite.Native;

/// <summary>
/// Utility class for detecting clipping in audio buffers.
/// Scans float buffers and reports clipped sample count, max absolute value, and first clip index.
/// Supports a configurable threshold (default 1.0f).
/// </summary>
public static class ClipDetector
{
    /// <summary>
    /// Detects clipping in a float buffer.
    /// </summary>
    /// <param name="buffer">The audio buffer to scan</param>
    /// <param name="threshold">The threshold above which a sample is considered clipped (default: 1.0f)</param>
    /// <returns>A ClipDetectionResult containing clipping statistics</returns>
    public static ClipDetectionResult Detect(float[] buffer, float threshold = 1.0f)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (float.IsNaN(threshold) || float.IsInfinity(threshold))
        {
            throw new ArgumentException("Threshold must be a valid finite number", nameof(threshold));
        }

        if (threshold <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be positive");
        }

        int clippedCount = 0;
        int firstClipIndex = -1;
        float maxAbsoluteValue = 0.0f;

        for (int i = 0; i < buffer.Length; i++)
        {
            float sample = buffer[i];
            float absSample = Math.Abs(sample);

            if (absSample > maxAbsoluteValue)
            {
                maxAbsoluteValue = absSample;
            }

            if (absSample > threshold)
            {
                clippedCount++;
                if (firstClipIndex == -1)
                {
                    firstClipIndex = i;
                }
            }
        }

        return new ClipDetectionResult
        {
            ClippedSampleCount = clippedCount,
            MaxAbsoluteValue = maxAbsoluteValue,
            FirstClipIndex = firstClipIndex
        };
    }

    /// <summary>
    /// Detects clipping in an AudioBuffer.
    /// </summary>
    /// <param name="buffer">The audio buffer to scan</param>
    /// <param name="threshold">The threshold above which a sample is considered clipped (default: 1.0f)</param>
    /// <returns>A ClipDetectionResult containing clipping statistics</returns>
    public static ClipDetectionResult Detect(AudioBuffer buffer, float threshold = 1.0f)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (float.IsNaN(threshold) || float.IsInfinity(threshold))
        {
            throw new ArgumentException("Threshold must be a valid finite number", nameof(threshold));
        }

        if (threshold <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold must be positive");
        }

        int clippedCount = 0;
        int firstClipIndex = -1;
        float maxAbsoluteValue = 0.0f;

        for (int frame = 0; frame < buffer.Frames; frame++)
        {
            for (int channel = 0; channel < buffer.Channels; channel++)
            {
                float sample = buffer[channel, frame];
                float absSample = Math.Abs(sample);

                if (absSample > maxAbsoluteValue)
                {
                    maxAbsoluteValue = absSample;
                }

                if (absSample > threshold)
                {
                    clippedCount++;
                    if (firstClipIndex == -1)
                    {
                        firstClipIndex = frame * buffer.Channels + channel;
                    }
                }
            }
        }

        return new ClipDetectionResult
        {
            ClippedSampleCount = clippedCount,
            MaxAbsoluteValue = maxAbsoluteValue,
            FirstClipIndex = firstClipIndex
        };
    }
}

/// <summary>
/// Contains the results of clipping detection.
/// </summary>
public sealed class ClipDetectionResult
{
    /// <summary>
    /// Gets the number of samples that exceeded the threshold.
    /// </summary>
    public int ClippedSampleCount { get; init; }

    /// <summary>
    /// Gets the maximum absolute value found in the buffer.
    /// </summary>
    public float MaxAbsoluteValue { get; init; }

    /// <summary>
    /// Gets the index of the first clipped sample, or -1 if no clipping was detected.
    /// </summary>
    public int FirstClipIndex { get; init; }

    /// <summary>
    /// Returns a string representation of the clip detection result.
    /// </summary>
    public override string ToString()
    {
        return $"Clipped: {ClippedSampleCount} samples, Max: {MaxAbsoluteValue:F6}, First: {FirstClipIndex}";
    }
}
