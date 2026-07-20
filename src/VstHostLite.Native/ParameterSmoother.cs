using System;

namespace VstHostLite.Native
{
    /// <summary>
    /// Smooths a parameter value towards a target using exponential smoothing.
    /// The smoothing speed is defined by a time constant (in seconds) and the sample rate.
    /// </summary>
    public sealed class ParameterSmoother
    {
        private readonly float _sampleRate;
        private readonly float _timeConstantSeconds;

        // Smoothing coefficient calculated from sample rate and time constant.
        // alpha = 1 - exp(-1 / (sampleRate * timeConstant))
        private readonly float _alpha;

        private float _current;
        private float _target;

        /// <summary>
        /// Creates a new <see cref="ParameterSmoother"/>.
        /// </summary>
        /// <param name="sampleRate">Audio sample rate (samples per second).</param>
        /// <param name="timeConstantSeconds">Time constant for the exponential smoothing (seconds).</param>
        /// <param name="initialValue">Initial value of the parameter.</param>
        public ParameterSmoother(float sampleRate, float timeConstantSeconds, float initialValue = 0f)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive.");
            if (timeConstantSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeConstantSeconds), "Time constant must be positive.");

            _sampleRate = sampleRate;
            _timeConstantSeconds = timeConstantSeconds;
            _alpha = 1f - (float)Math.Exp(-1.0 / (_sampleRate * _timeConstantSeconds));

            _current = initialValue;
            _target = initialValue;
        }

        /// <summary>
        /// Gets the current smoothed value.
        /// </summary>
        public float Current => _current;

        /// <summary>
        /// Gets or sets the target value the smoother moves towards.
        /// </summary>
        public float Target
        {
            get => _target;
            set => _target = value;
        }

        /// <summary>
        /// Instantly jumps the current value to the target value, bypassing smoothing.
        /// </summary>
        public void SnapToTarget()
        {
            _current = _target;
        }

        /// <summary>
        /// Calculates the next smoothed value for a single sample.
        /// </summary>
        /// <returns>The next smoothed value.</returns>
        public float NextValue()
        {
            // y[n] = y[n-1] + (target - y[n-1]) * alpha
            _current += (_target - _current) * _alpha;
            return _current;
        }

        /// <summary>
        /// Processes a block of samples, writing the smoothed values into the supplied buffer.
        /// The buffer is overwritten with the smoothed values.
        /// </summary>
        /// <param name="destination">Array that will receive the smoothed values.</param>
        public void Process(float[] destination)
        {
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = NextValue();
            }
        }
    }
}
