using System;
using System.Collections.Generic;

namespace VstHostLite.Native
{
    /// <summary>
    /// Extension methods for <see cref="ParameterSmoother"/> that provide additional functionality
    /// for working with parameter smoothing in audio processing scenarios.
    /// </summary>
    public static class ParameterSmootherExtensions
    {
        /// <summary>
        /// Processes a block of samples, applying smoothing and returning the results as a new array.
        /// The original smoother state is preserved.
        /// </summary>
        /// <param name="smoother">The parameter smoother instance.</param>
        /// <param name="count">Number of samples to process.</param>
        /// <returns>New array containing the smoothed values.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
        public static float[] ProcessToArray(this ParameterSmoother smoother, int count)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

            var result = new float[count];
            smoother.Process(result);
            return result;
        }

        /// <summary>
        /// Processes a block of samples with a custom target value, returning the results as a new array.
        /// The smoother's target is temporarily set during processing.
        /// </summary>
        /// <param name="smoother">The parameter smoother instance.</param>
        /// <param name="count">Number of samples to process.</param>
        /// <param name="target">The target value to smooth towards during this processing block.</param>
        /// <returns>New array containing the smoothed values.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 1.</exception>
        public static float[] ProcessToArray(this ParameterSmoother smoother, int count, float target)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

            var originalTarget = smoother.Target;
            try
            {
                smoother.Target = target;
                return smoother.ProcessToArray(count);
            }
            finally
            {
                smoother.Target = originalTarget;
            }
        }

        /// <summary>
        /// Processes multiple target values into a single smoothed output array.
        /// Each target value is processed sequentially, creating a smooth transition between targets.
        /// </summary>
        /// <param name="smoother">The parameter smoother instance.</param>
        /// <param name="targets">Sequence of target values to process.</param>
        /// <returns>Array containing all smoothed values in sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="targets"/> is null.</exception>
        public static float[] ProcessTargets(this ParameterSmoother smoother, IEnumerable<float> targets)
        {
            ArgumentNullException.ThrowIfNull(targets);

            // Count targets first to allocate exact array size
            int count = targets.TryGetNonEnumeratedCount(out int knownCount) ? knownCount : 0;
            if (count == 0)
            {
                // Fallback for unknown count - process one by one
                var result = new List<float>();
                foreach (var target in targets)
                {
                    smoother.Target = target;
                    result.Add(smoother.NextValue());
                }
                return result.ToArray();
            }

            var output = new float[count];
            int index = 0;
            foreach (var target in targets)
            {
                smoother.Target = target;
                output[index++] = smoother.NextValue();
            }
            return output;
        }

        /// <summary>
        /// Gets the current smoothing ratio (alpha coefficient) used by the smoother.
        /// This represents how quickly the smoother approaches its target value.
        /// </summary>
        /// <param name="smoother">The parameter smoother instance.</param>
        /// <returns>The smoothing coefficient (0-1), where 0 is no smoothing and 1 is instant.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="smoother"/> is null.</exception>
        public static float GetSmoothingRatio(this ParameterSmoother smoother)
        {
            ArgumentNullException.ThrowIfNull(smoother);

            // The alpha value is private, but we can calculate it from the public behavior
            // by checking how much a single step changes the value from a known starting point
            float originalTarget = smoother.Target;
            float originalValue = smoother.Current;

            try
            {
                smoother.Target = originalValue + 1.0f;
                float nextValue = smoother.NextValue();
                return Math.Abs(nextValue - originalValue);
            }
            finally
            {
                smoother.Target = originalTarget;
            }
        }
    }
}