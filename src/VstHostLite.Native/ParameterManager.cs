using System;
using System.Collections.Generic;

namespace VstHostLite.Native
{
    /// <summary>
    /// Manages a collection of named parameters with normalization, automation support, and preset functionality.
    /// Parameters are normalized to the range [0, 1] internally and can be mapped to different ranges for display or automation.
    /// </summary>
    public sealed class ParameterManager
    {
        private readonly Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();
        private readonly object _syncLock = new object();

        /// <summary>
        /// Creates a new <see cref="ParameterManager"/>.
        /// </summary>
        public ParameterManager()
        {
        }

        /// <summary>
        /// Gets the number of parameters managed by this instance.
        /// </summary>
        public int Count => _parameters.Count;

        /// <summary>
        /// Gets a value indicating whether this instance is thread-safe for concurrent access.
        /// All public methods of this class are thread-safe and can be called from multiple threads simultaneously.
        /// </summary>
        public bool IsThreadSafe => true;

        /// <summary>
        /// Adds a new parameter with the specified name and default value.
        /// </summary>
        /// <param name="name">The unique name of the parameter.</param>
        /// <param name="defaultValue">The default normalized value (0.0 to 1.0).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="defaultValue"/> is not between 0 and 1.</exception>
        /// <exception cref="ArgumentException">Thrown when a parameter with <paramref name="name"/> already exists.</exception>
        public void AddParameter(string name, float defaultValue = 0.5f)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (defaultValue < 0f || defaultValue > 1f)
                throw new ArgumentOutOfRangeException(nameof(defaultValue), "Default value must be between 0 and 1");

            lock (_syncLock)
            {
                if (_parameters.ContainsKey(name))
                    throw new ArgumentException($"A parameter with name '{name}' already exists.", nameof(name));

                _parameters[name] = new Parameter(name, defaultValue);
            }
        }

        /// <summary>
        /// Removes the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter to remove.</param>
        /// <returns>true if the parameter was found and removed; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        public bool RemoveParameter(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_syncLock)
            {
                return _parameters.Remove(name);
            }
        }

        /// <summary>
        /// Gets the normalized value (0.0 to 1.0) of the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The normalized value of the parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no parameter with <paramref name="name"/> exists.</exception>
        public float GetNormalizedValue(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_syncLock)
            {
                if (!_parameters.TryGetValue(name, out var parameter))
                    throw new KeyNotFoundException($"No parameter found with name '{name}'.");

                return parameter.NormalizedValue;
            }
        }

        /// <summary>
        /// Sets the normalized value (0.0 to 1.0) of the parameter with the specified name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The normalized value to set (0.0 to 1.0).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not between 0 and 1.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no parameter with <paramref name="name"/> exists.</exception>
        public void SetNormalizedValue(string name, float value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (value < 0f || value > 1f)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 1");

            lock (_syncLock)
            {
                if (!_parameters.TryGetValue(name, out var parameter))
                    throw new KeyNotFoundException($"No parameter found with name '{name}'.");

                parameter.NormalizedValue = value;
            }
        }

        /// <summary>
        /// Gets the value of the parameter with the specified name, converted to the specified range.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="min">The minimum value of the target range.</param>
        /// <param name="max">The maximum value of the target range.</param>
        /// <returns>The parameter value mapped to the range [<paramref name="min"/>, <paramref name="max"/>].</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no parameter with <paramref name="name"/> exists.</exception>
        public float GetValue(string name, float min, float max)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            lock (_syncLock)
            {
                if (!_parameters.TryGetValue(name, out var parameter))
                    throw new KeyNotFoundException($"No parameter found with name '{name}'.");

                return parameter.GetValue(min, max);
            }
        }

        /// <summary>
        /// Sets the value of the parameter with the specified name, converting from the specified range to normalized form.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="min">The minimum value of the source range.</param>
        /// <param name="max">The maximum value of the source range.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not between <paramref name="min"/> and <paramref name="max"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when no parameter with <paramref name="name"/> exists.</exception>
        public void SetValue(string name, float value, float min, float max)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (min >= max)
                throw new ArgumentOutOfRangeException(nameof(min), "Min must be less than max");
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(nameof(value), $"Value must be between {min} and {max}");

            lock (_syncLock)
            {
                if (!_parameters.TryGetValue(name, out var parameter))
                    throw new KeyNotFoundException($"No parameter found with name '{name}'.");

                parameter.SetValue(value, min, max);
            }
        }

        /// <summary>
        /// Gets all parameter names managed by this instance.
        /// </summary>
        /// <returns>An array containing the names of all parameters.</returns>
        public string[] GetParameterNames()
        {
            lock (_syncLock)
            {
                return new List<string>(_parameters.Keys).ToArray();
            }
        }

        /// <summary>
        /// Creates a preset containing the current values of all parameters.
        /// </summary>
        /// <param name="presetName">The name to assign to the preset.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="presetName"/> is null.</exception>
        public void CreatePreset(string presetName)
        {
            if (presetName == null) throw new ArgumentNullException(nameof(presetName));

            lock (_syncLock)
            {
                var preset = new ParameterPreset(presetName);
                foreach (var kvp in _parameters)
                {
                    preset.AddParameterValue(kvp.Key, kvp.Value.NormalizedValue);
                }
                // In a full implementation, presets would be stored in a collection
                // For this minimal implementation, we just create the preset object
                // A real implementation would store presets for later recall
            }
        }

        /// <summary>
        /// Applies a preset, setting all parameters to their stored values.
        /// </summary>
        /// <param name="preset">The preset to apply.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="preset"/> is null.</exception>
        public void ApplyPreset(ParameterPreset preset)
        {
            if (preset == null) throw new ArgumentNullException(nameof(preset));

            lock (_syncLock)
            {
                foreach (var kvp in preset.GetParameterValues())
                {
                    if (_parameters.TryGetValue(kvp.Key, out var parameter))
                    {
                        parameter.NormalizedValue = kvp.Value;
                    }
                    // Parameters not found in this manager are ignored
                }
            }
        }

        /// <summary>
        /// Represents a single parameter with normalization and range conversion capabilities.
        /// </summary>
        private class Parameter
        {
            private readonly string _name;
            private float _normalizedValue;

            /// <summary>
            /// Creates a new <see cref="Parameter"/> instance.
            /// </summary>
            /// <param name="name">The name of the parameter.</param>
            /// <param name="normalizedValue">The initial normalized value (0.0 to 1.0).</param>
            public Parameter(string name, float normalizedValue)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
                NormalizedValue = normalizedValue;
            }

            /// <summary>
            /// Gets the name of this parameter.
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// Gets or sets the normalized value (0.0 to 1.0) of this parameter.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not between 0 and 1.</exception>
            public float NormalizedValue
            {
                get => _normalizedValue;
                set
                {
                    if (value < 0f || value > 1f)
                        throw new ArgumentOutOfRangeException(nameof(value), "Normalized value must be between 0 and 1");
                    _normalizedValue = value;
                }
            }

            /// <summary>
            /// Gets the value of this parameter converted to the specified range.
            /// </summary>
            /// <param name="min">The minimum value of the target range.</param>
            /// <param name="max">The maximum value of the target range.</param>
            /// <returns>The parameter value mapped to the range [<paramref name="min"/>, <paramref name="max"/>].</returns>
            public float GetValue(float min, float max)
            {
                return min + (_normalizedValue * (max - min));
            }

            /// <summary>
            /// Sets the normalized value based on a value in the specified range.
            /// </summary>
            /// <param name="value">The value to convert from the source range.</param>
            /// <param name="min">The minimum value of the source range.</param>
            /// <param name="max">The maximum value of the source range.</param>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not between <paramref name="min"/> and <paramref name="max"/>.</exception>
            public void SetValue(float value, float min, float max)
            {
                if (min >= max)
                    throw new ArgumentOutOfRangeException(nameof(min), "Min must be less than max");
                if (value < min || value > max)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Value must be between {min} and {max}");

                _normalizedValue = (value - min) / (max - min);
            }
        }

        /// <summary>
        /// Represents a collection of parameter values that can be saved and recalled as a preset.
        /// </summary>
        public class ParameterPreset
        {
            private readonly string _name;
            private readonly Dictionary<string, float> _parameterValues = new Dictionary<string, float>();

            /// <summary>
            /// Creates a new <see cref="ParameterPreset"/> instance.
            /// </summary>
            /// <param name="name">The name of the preset.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
            public ParameterPreset(string name)
            {
                _name = name ?? throw new ArgumentNullException(nameof(name));
            }

            /// <summary>
            /// Gets the name of this preset.
            /// </summary>
            public string Name => _name;

            /// <summary>
            /// Adds a parameter value to this preset.
            /// </summary>
            /// <param name="parameterName">The name of the parameter.</param>
            /// <param name="normalizedValue">The normalized value (0.0 to 1.0) to store.</param>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameterName"/> is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="normalizedValue"/> is not between 0 and 1.</exception>
            public void AddParameterValue(string parameterName, float normalizedValue)
            {
                if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
                if (normalizedValue < 0f || normalizedValue > 1f)
                    throw new ArgumentOutOfRangeException(nameof(normalizedValue), "Normalized value must be between 0 and 1");

                _parameterValues[parameterName] = normalizedValue;
            }

            /// <summary>
            /// Gets all parameter names and their normalized values in this preset.
            /// </summary>
            /// <returns>A dictionary containing parameter names and their normalized values.</returns>
            public IReadOnlyDictionary<string, float> GetParameterValues()
            {
                return new Dictionary<string, float>(_parameterValues);
            }
        }
    }
}