using System;
using System.Collections.Generic;

namespace VstHostLite.Native.Tests
{
    /// <summary>
    /// Provides validation extensions for <see cref="AudioGraphTests"/>.
    /// </summary>
    public static class AudioGraphTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="AudioGraphTests"/> instance and returns a read‑only list of problems.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>A read‑only list of validation errors. Empty if the instance is valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> Validate(this AudioGraphTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            // The test class has no instance data to validate; return an empty list.
            return Array.Empty<string>();
        }

        /// <summary>
        /// Determines whether the <see cref="AudioGraphTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance.</param>
        /// <returns><c>true</c> if the instance is valid; otherwise, <c>false</c>.</returns>
        public static bool IsValid(this AudioGraphTests? value) =>
            value is not null && value.Validate().Count == 0;

        /// <summary>
        /// Ensures the <see cref="AudioGraphTests"/> instance is valid, throwing if not.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when validation errors are present.</exception>
        public static void EnsureValid(this AudioGraphTests? value)
        {
            ArgumentNullException.ThrowIfNull(value);
            var errors = value.Validate();
            if (errors.Count > 0)
                throw new ArgumentException($"AudioGraphTests is invalid: {string.Join(", ", errors)}", nameof(value));
        }
    }
}
