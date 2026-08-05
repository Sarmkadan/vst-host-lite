using System;

namespace VstHostLite.Native
{
    /// <summary>
    /// Provides analysis and shaping extension methods for raw <see cref="float"/> sample buffers.
    /// </summary>
    public static class BufferExtensions
    {
        /// <summary>
        /// Computes the root-mean-square level of the buffer.
        /// </summary>
        /// <param name="buffer">The sample buffer.</param>
        /// <returns>The RMS value, or 0 when the buffer is empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        public static float Rms(this float[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (buffer.Length == 0)
                return 0f;

            double sumOfSquares = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                sumOfSquares += (double)buffer[i] * buffer[i];
            }

            return (float)Math.Sqrt(sumOfSquares / buffer.Length);
        }

        /// <summary>
        /// Computes the peak absolute sample value in the buffer.
        /// </summary>
        /// <param name="buffer">The sample buffer.</param>
        /// <returns>The maximum absolute sample value, or 0 when the buffer is empty.</returns>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        public static float Peak(this float[] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            float peak = 0f;
            for (int i = 0; i < buffer.Length; i++)
            {
                float abs = Math.Abs(buffer[i]);
                if (abs > peak)
                    peak = abs;
            }

            return peak;
        }

        /// <summary>
        /// Scales the buffer in place so its peak absolute value equals <paramref name="targetPeak"/>.
        /// </summary>
        /// <param name="buffer">The sample buffer to normalize.</param>
        /// <param name="targetPeak">The desired peak absolute value; must be non-negative.</param>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when targetPeak is negative.</exception>
        public static void NormalizeTo(this float[] buffer, float targetPeak)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (targetPeak < 0f)
                throw new ArgumentOutOfRangeException(nameof(targetPeak), targetPeak, "Target peak must be non-negative.");

            float currentPeak = buffer.Peak();
            if (currentPeak <= float.Epsilon)
                return;

            float scale = targetPeak / currentPeak;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] *= scale;
            }
        }

        /// <summary>
        /// Multiplies every sample in the buffer by the given gain, in place.
        /// </summary>
        /// <param name="buffer">The sample buffer.</param>
        /// <param name="gain">The linear gain factor to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        public static void ApplyGain(this float[] buffer, float gain)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] *= gain;
            }
        }

        /// <summary>
        /// Applies a linear fade-in over the first <paramref name="samples"/> samples of the buffer, in place.
        /// </summary>
        /// <param name="buffer">The sample buffer.</param>
        /// <param name="samples">The number of samples the fade should span; must be non-negative.</param>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when samples is negative.</exception>
        public static void FadeIn(this float[] buffer, int samples)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (samples < 0)
                throw new ArgumentOutOfRangeException(nameof(samples), samples, "Sample count must be non-negative.");

            int length = Math.Min(samples, buffer.Length);
            if (length == 0)
                return;

            for (int i = 0; i < length; i++)
            {
                float factor = length == 1 ? 1f : (float)i / (length - 1);
                buffer[i] *= factor;
            }
        }

        /// <summary>
        /// Applies a linear fade-out over the last <paramref name="samples"/> samples of the buffer, in place.
        /// </summary>
        /// <param name="buffer">The sample buffer.</param>
        /// <param name="samples">The number of samples the fade should span; must be non-negative.</param>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when samples is negative.</exception>
        public static void FadeOut(this float[] buffer, int samples)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (samples < 0)
                throw new ArgumentOutOfRangeException(nameof(samples), samples, "Sample count must be non-negative.");

            int length = Math.Min(samples, buffer.Length);
            if (length == 0)
                return;

            int start = buffer.Length - length;
            for (int i = 0; i < length; i++)
            {
                float factor = length == 1 ? 0f : 1f - (float)i / (length - 1);
                buffer[start + i] *= factor;
            }
        }
    }
}
