namespace VstHostLite.Native;

/// <summary>
/// Provides validation helpers for <see cref="AudioGraphExtensions"/> extension methods.
/// These methods validate the parameters that AudioGraphExtensions methods would receive.
/// </summary>
public static class AudioGraphExtensionsValidation
{
    /// <summary>
    /// Validates an <see cref="AudioGraph"/> instance for use with AudioGraphExtensions methods.
    /// </summary>
    /// <param name="graph">The audio graph instance to validate.</param>
    /// <returns>An enumerable of validation problems; empty if the graph is valid for extension method usage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> is null.</exception>
    public static IReadOnlyList<string> Validate(AudioGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        // AudioGraphExtensions extension methods only require that the graph parameter is not null.
        // All other validation (node null checks, etc.) is handled within each extension method.
        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether an <see cref="AudioGraph"/> instance is valid for use with AudioGraphExtensions methods.
    /// </summary>
    /// <param name="graph">The audio graph instance to check.</param>
    /// <returns>True if the graph is valid for extension method usage; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> is null.</exception>
    public static bool IsValid(AudioGraph graph)
        => Validate(graph).Count == 0;

    /// <summary>
    /// Ensures that an <see cref="AudioGraph"/> instance is valid for use with AudioGraphExtensions methods,
    /// throwing an exception if it is not.
    /// </summary>
    /// <param name="graph">The audio graph instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="graph"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the graph is not valid for extension method usage.</exception>
    public static void EnsureValid(AudioGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);

        var problems = Validate(graph);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "AudioGraph is not valid for AudioGraphExtensions method usage.",
                nameof(graph));
        }
    }
}