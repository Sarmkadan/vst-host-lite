using System;

namespace VstHostLite.Native
{
    /// <summary>
    /// Represents a noise generator node.
    /// </summary>
    public interface INoiseGeneratorNode
    {
        /// <summary>
        /// Gets the name of the noise generator node.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Processes the noise and writes the output to the provided buffer.
        /// </summary>
        /// <param name="output">The buffer to write the noise samples to.</param>
        void Process(float[] output);
    }
}
