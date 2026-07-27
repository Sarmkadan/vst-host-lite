using System;
using System.Collections.Generic;

namespace VstHostLite.Native;

/// <summary>
/// Provides validation extensions for <see cref="AudioBuffer"/>.
/// </summary>
public static class AudioBufferValidation
{
    /// <summary>
    /// Validates the <see cref="AudioBuffer"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The buffer to validate.</param>
    /// <returns>A read-only list of validation errors. Returns an empty list if the buffer is valid.</returns>
    public static IReadOnlyList<string> Validate(this AudioBuffer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        List<string> problems = new();

        if (value.Channels < 0)
        {
            problems.Add("Channels must be non-negative.");
        }

        if (value.Frames < 0)
        {
            problems.Add("Frames must be non-negative.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the <see cref="AudioBuffer"/> is valid.
    /// </summary>
    /// <param name="value">The buffer to check.</param>
    /// <returns><c>true</c> if the buffer is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this AudioBuffer value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the <see cref="AudioBuffer"/> is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The buffer to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the buffer contains invalid data.</exception>
    public static void EnsureValid(this AudioBuffer value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException($"AudioBuffer is invalid: {string.Join(", ", errors)}", nameof(value));
        }
    }
}
