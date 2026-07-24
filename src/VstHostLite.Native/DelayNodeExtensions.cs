using System;

namespace VstHostLite.Native;

/// <summary>
/// Provides extension methods for the <see cref="DelayNode"/> class.
/// </summary>
public static class DelayNodeExtensions
{
    /// <summary>
    /// Resets the delay node and processes the provided audio buffers.
    /// </summary>
    /// <param name="node">The <see cref="DelayNode"/> instance.</param>
    /// <param name="input">The input audio buffer.</param>
    /// <param name="output">The output audio buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/>, <paramref name="input"/>, or <paramref name="output"/> is <c>null</c>.</exception>
    public static void ResetAndProcess(this DelayNode node, float[] input, float[] output)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(output);

        node.Reset();
        node.Process(input, output);
    }

    /// <summary>
    /// Returns a formatted string summary of the <see cref="DelayNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="DelayNode"/> instance.</param>
    /// <returns>A string summary containing the node's name.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="node"/> is <c>null</c>.</exception>
    public static string GetNodeSummary(this DelayNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        return $"DelayNode: {node.Name}";
    }
}
