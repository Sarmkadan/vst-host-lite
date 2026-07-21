namespace VstHostLite.Native;

/// <summary>
/// Provides validation helpers for <see cref="MixerNode"/> instances.
/// </summary>
public static class MixerNodeValidation
{
    /// <summary>
    /// Validates a MixerNode instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The MixerNode to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this MixerNode value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name cannot be null or whitespace");
        }

        // Validate InputCount
        if (value.InputCount <= 0)
        {
            errors.Add("InputCount must be greater than zero");
        }

        // Validate Frames
        if (value.Frames <= 0)
        {
            errors.Add("Frames must be greater than zero");
        }

        // Validate all gains are valid finite numbers
        for (int i = 0; i < value.InputCount; i++)
        {
            float gain = value.GetGain(i);
            if (float.IsNaN(gain))
            {
                errors.Add($"Gain at index {i} is NaN");
            }
            else if (float.IsInfinity(gain))
            {
                errors.Add($"Gain at index {i} is infinite");
            }
            else if (gain < 0.0f)
            {
                errors.Add($"Gain at index {i} cannot be negative (got {gain})");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a MixerNode instance is valid.
    /// </summary>
    /// <param name="value">The MixerNode to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    public static bool IsValid(this MixerNode value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a MixerNode instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The MixerNode to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid, with a list of problems</exception>
    public static void EnsureValid(this MixerNode value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"MixerNode is invalid:{Environment.NewLine}  - {
                    string.Join($"{Environment.NewLine}  - ", errors)
                }");
        }
    }
}
