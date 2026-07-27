using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VstHostLite.Native;

/// <summary>
/// Validation helpers for the JSON DTO types used by <see cref="AudioGraphJsonExtensions"/>.
/// </summary>
public static class AudioGraphJsonExtensionsValidation
{
    /// <summary>
    /// Validates the <see cref="AudioGraphNodeDto"/> instance and returns a list of human‑readable problems.
    /// </summary>
    /// <param name="value">The DTO to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation error messages; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    internal static IReadOnlyList<string> Validate(this AudioGraphNodeDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Name must be non‑null and non‑empty.
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name must be a non‑empty string.");
        }

        // Component (nint) must not be zero (IntPtr.Zero).
        if (value.Component == nint.Zero)
        {
            problems.Add("Component must be a non‑zero pointer.");
        }

        // NextIndex must be -1 (no next) or a non‑negative index.
        if (value.NextIndex < -1)
        {
            problems.Add($"NextIndex ({value.NextIndex}) is out of range; must be -1 or a non‑negative integer.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the <see cref="AudioGraphNodeDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The DTO to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    internal static bool IsValid(this AudioGraphNodeDto value) =>
        !value.Validate().Any();

    /// <summary>
    /// Ensures that the <see cref="AudioGraphNodeDto"/> instance is valid.
    /// </summary>
    /// <param name="value">The DTO to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the DTO contains validation problems; the exception message lists all problems.</exception>
    internal static void EnsureValid(this AudioGraphNodeDto value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            var message = string.Join("; ", problems);
            throw new ArgumentException(message, nameof(value));
        }
    }
}
