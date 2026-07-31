using System;

namespace VstHostLite.Native
{
    /// <summary>
    /// Interface exposing the public smoothing operations of <see cref="ParameterSmoother"/>.
    /// </summary>
    public interface IParameterSmoother
    {
        /// <summary>
        /// Instantly jumps the current value to the target value, bypassing smoothing.
        /// </summary>
        void SnapToTarget();

        /// <summary>
        /// Calculates the next smoothed value for a single sample.
        /// </summary>
        /// <returns>The next smoothed value.</returns>
        float NextValue();

        /// <summary>
        /// Processes a block of samples, writing the smoothed values into the supplied buffer.
        /// The buffer is overwritten with the smoothed values.
        /// </summary>
        /// <param name="destination">Array that will receive the smoothed values.</param>
        void Process(float[] destination);
    }
}
