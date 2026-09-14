using System;

namespace VstHostLite.Native
{
    /// <summary>
    /// Represents a sine wave generator node.
    /// </summary>
    public interface ISineGeneratorNode
    {
        /// <summary>
        /// Gets the name of this sine generator node.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the output amplitude (0.0 to 1.0).
        /// </summary>
        float Amplitude { get; set; }

        /// <summary>
        /// Gets or sets the output frequency in Hz.
        /// </summary>
        float Frequency { get; set; }

        /// <summary>
        /// Gets the current sample rate in Hz.
        /// </summary>
        float SampleRate { get; }

        /// <summary>
        /// Gets the number of audio frames per buffer.
        /// </summary>
        int Frames { get; }

        /// <summary>
        /// Generates a sine wave into the provided mono buffer.
        /// </summary>
        /// <param name="buffer">Output buffer to fill with generated audio (must have Frames length)</param>
        /// <exception cref="ArgumentNullException">Thrown if buffer is null</exception>
        /// <exception cref="ArgumentException">Thrown if buffer length doesn't match Frames</exception>
        void Generate(float[] buffer);

        /// <summary>
        /// Resets the phase accumulator to zero.
        /// </summary>
        void Reset();
    }
}