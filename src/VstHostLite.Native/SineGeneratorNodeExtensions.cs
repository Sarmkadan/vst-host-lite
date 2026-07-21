namespace VstHostLite.Native;

/// <summary>
/// Provides extension methods for <see cref="SineGeneratorNode"/> to simplify common operations
/// and enhance functionality when working with sine wave generators in audio processing scenarios.
/// </summary>
public static class SineGeneratorNodeExtensions
{
    /// <summary>
    /// Sets the frequency of the sine generator using a note name (e.g., "A4", "C#3").
    /// </summary>
    /// <param name="node">The sine generator node to modify</param>
    /// <param name="noteName">Note name in scientific pitch notation (e.g., "A4", "C#3", "Bb5")</param>
    /// <exception cref="ArgumentNullException">Thrown if node is null</exception>
    /// <exception cref="ArgumentException">Thrown if noteName is null, empty, or invalid</exception>
    public static void SetFrequencyFromNote(this SineGeneratorNode node, string noteName)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrEmpty(noteName);

        float frequency = ParseNoteNameToFrequency(noteName);
        node.Frequency = frequency;
    }

    /// <summary>
    /// Generates a sine wave with the specified frequency and amplitude, returning the result as an array.
    /// </summary>
    /// <param name="node">The sine generator node</param>
    /// <param name="frequency">Frequency in Hz</param>
    /// <param name="amplitude">Amplitude between 0.0 and 1.0</param>
    /// <returns>Array containing the generated sine wave samples</returns>
    /// <exception cref="ArgumentNullException">Thrown if node is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if amplitude is outside [0, 1] range</exception>
    public static float[] Generate(this SineGeneratorNode node, float frequency, float amplitude)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (float.IsNaN(amplitude) || float.IsInfinity(amplitude))
        {
            throw new ArgumentException("Amplitude must be a valid finite number", nameof(amplitude));
        }

        if (amplitude < 0.0f || amplitude > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(amplitude), "Amplitude must be between 0.0 and 1.0");
        }

        float originalFrequency = node.Frequency;
        float originalAmplitude = node.Amplitude;

        try
        {
            node.Frequency = frequency;
            node.Amplitude = amplitude;
            node.Reset();

            float[] buffer = new float[node.Frames];
            node.Generate(buffer);
            return buffer;
        }
        finally
        {
            node.Frequency = originalFrequency;
            node.Amplitude = originalAmplitude;
        }
    }

    /// <summary>
    /// Generates a sine wave with the specified frequency and amplitude, returning the result as an array.
    /// </summary>
    /// <param name="node">The sine generator node</param>
    /// <param name="noteName">Note name in scientific pitch notation</param>
    /// <param name="amplitude">Amplitude between 0.0 and 1.0</param>
    /// <returns>Array containing the generated sine wave samples</returns>
    /// <exception cref="ArgumentNullException">Thrown if node is null</exception>
    /// <exception cref="ArgumentException">Thrown if noteName is null or empty</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if amplitude is outside [0, 1] range</exception>
    public static float[] Generate(this SineGeneratorNode node, string noteName, float amplitude)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrEmpty(noteName);

        float frequency = ParseNoteNameToFrequency(noteName);
        return node.Generate(frequency, amplitude);
    }

    /// <summary>
    /// Generates multiple buffers of sine wave data with the specified parameters.
    /// </summary>
    /// <param name="node">The sine generator node</param>
    /// <param name="frequency">Frequency in Hz</param>
    /// <param name="amplitude">Amplitude between 0.0 and 1.0</param>
    /// <param name="bufferCount">Number of buffers to generate</param>
    /// <returns>Read-only list of arrays containing generated sine wave samples</returns>
    /// <exception cref="ArgumentNullException">Thrown if node is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if amplitude is outside [0, 1] range or bufferCount is not positive</exception>
    public static IReadOnlyList<float[]> GenerateBuffers(this SineGeneratorNode node, float frequency, float amplitude, int bufferCount)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (bufferCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferCount), "Buffer count must be positive");
        }

        var buffers = new List<float[]>(bufferCount);
        float originalFrequency = node.Frequency;
        float originalAmplitude = node.Amplitude;

        try
        {
            node.Frequency = frequency;
            node.Amplitude = amplitude;

            for (int i = 0; i < bufferCount; i++)
            {
                node.Reset();
                float[] buffer = new float[node.Frames];
                node.Generate(buffer);
                buffers.Add(buffer);
            }

            return buffers.AsReadOnly();
        }
        finally
        {
            node.Frequency = originalFrequency;
            node.Amplitude = originalAmplitude;
        }
    }

    /// <summary>
    /// Parses a note name in scientific pitch notation to its corresponding frequency in Hz.
    /// Supports note names like "C4", "C#4", "Db4", "A4", "Bb3", etc.
    /// </summary>
    /// <param name="noteName">Note name in scientific pitch notation</param>
    /// <returns>Frequency in Hz</returns>
    /// <exception cref="ArgumentException">Thrown if noteName is invalid</exception>
    private static float ParseNoteNameToFrequency(string noteName)
    {
        ArgumentException.ThrowIfNullOrEmpty(noteName);

        // Remove any whitespace
        noteName = noteName.Trim();

        if (noteName.Length < 2)
        {
            throw new ArgumentException("Note name must be at least 2 characters (e.g., 'A4')", nameof(noteName));
        }

        // Extract note name and octave
        string notePart = noteName.Substring(0, noteName.Length - 1);
        if (!int.TryParse(noteName[^1..], out int octave))
        {
            throw new ArgumentException("Note name must end with an octave number (e.g., 'A4')", nameof(noteName));
        }

        // Parse note part (e.g., "C", "C#", "Db")
        float noteValue = ParseNotePart(notePart);

        // Calculate frequency: A4 = 440 Hz, equal temperament
        // frequency = 440 * 2^((n - 69)/12) where n is MIDI note number
        // C4 = MIDI note 60, A4 = MIDI note 69
        int midiNote = (octave + 1) * 12 + (int)noteValue;
        return 440.0f * MathF.Pow(2.0f, (midiNote - 69) / 12.0f);
    }

    /// <summary>
    /// Parses the note part (e.g., "C", "C#", "Db") to a semitone offset from C.
    /// Returns the MIDI note number offset from C0.
    /// </summary>
    private static float ParseNotePart(string notePart)
    {
        return notePart.ToLowerInvariant() switch
        {
            "c" => 0,
            "c#" or "db" => 1,
            "d" => 2,
            "d#" or "eb" => 3,
            "e" => 4,
            "f" => 5,
            "f#" or "gb" => 6,
            "g" => 7,
            "g#" or "ab" => 8,
            "a" => 9,
            "a#" or "bb" => 10,
            "b" or "h" => 11,
            _ => throw new ArgumentException($"Invalid note name: '{notePart}'", nameof(notePart))
        };
    }
}