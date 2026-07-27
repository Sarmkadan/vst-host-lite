using System;
using System.Collections.Generic;
using System.Globalization;

namespace VstHostLite.Native;

/// <summary>
/// Provides validation extensions for <see cref="ClipDetectionResult"/>.
/// </summary>
public static class ClipDetectorValidation
{
    /// <summary>
    /// Validates the <see cref="ClipDetectionResult"/> and returns a list of human‑readable problems.
    /// </summary>
    /// <param name="value">The result to validate.</param>
    /// <returns>A read‑only list of validation errors. Returns an empty list if the result is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this ClipDetectionResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.ClippedSampleCount < 0)
        {
            problems.Add("ClippedSampleCount must be non‑negative.");
        }

        if (value.MaxAbsoluteValue < 0.0f)
        {
            problems.Add("MaxAbsoluteValue must be non‑negative.");
        }

        if (value.FirstClipIndex < -1)
        {
            problems.Add("FirstClipIndex must be -1 or non‑negative.");
        }

        if (value.ClippedSampleCount > 0 && value.FirstClipIndex == -1)
        {
            problems.Add("FirstClipIndex must be non‑negative when ClippedSampleCount > 0.");
        }

        if (value.ClippedSampleCount == 0 && value.FirstClipIndex != -1)
        {
            problems.Add("FirstClipIndex must be -1 when ClippedSampleCount == 0.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the <see cref="ClipDetectionResult"/> is valid.
    /// </summary>
    /// <param name="value">The result to check.</param>
    /// <returns><c>true</c> if the result is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this ClipDetectionResult value)
    {
        return value is not null && value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the <see cref="ClipDetectionResult"/> is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The result to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if the result contains invalid data.</exception>
    public static void EnsureValid(this ClipDetectionResult value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"ClipDetectionResult is invalid: {string.Join(", ", errors)}",
                nameof(value));
        }
    }
}
