using System;
using System.Collections.Generic;
using System.Globalization;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Provides validation helpers for <see cref="PluginScanCacheTests"/> instances.
/// </summary>
public static class PluginScanCacheTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="PluginScanCacheTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>An enumerable of human-readable problem descriptions; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this PluginScanCacheTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate all public methods are non-null and valid
        // TryGetFresh_ReturnsFalse_WhenPluginDoesNotExist - no parameters to validate
        // SaveAndTryGetFresh_RoundtripWorks - no parameters to validate
        // TryGetFresh_ReturnsFalse_WhenCacheIsStale - no parameters to validate
        // Clear_RemovesCacheFile - no parameters to validate
        // ClearAll_RemovesAllCacheFiles - no parameters to validate

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PluginScanCacheTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this PluginScanCacheTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PluginScanCacheTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this PluginScanCacheTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PluginScanCacheTests instance is invalid. Problems: {string.Join("; ", problems)}");
        }
    }
}
