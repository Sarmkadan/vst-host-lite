namespace VstHostLite.Native;

/// <summary>
/// A mixer node that sums N input float buffers into one output buffer with per-input gain control.
/// This is a processing node that can be added to the audio graph via AudioGraph.AddNode().
/// </summary>
public sealed class MixerNode
{
    private readonly float[] _gains;
    private readonly int _inputCount;
    private readonly int _frames;

    /// <summary>
    /// Creates a new MixerNode.
    /// </summary>
    /// <param name="name">Node name for identification</param>
    /// <param name="inputCount">Number of input channels to mix</param>
    /// <param name="frames">Number of audio frames per buffer</param>
    public MixerNode(string name, int inputCount, int frames)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _inputCount = inputCount;
        _frames = frames;
        _gains = new float[inputCount];

        // Default to unity gain for all inputs
        for (int i = 0; i < inputCount; i++)
        {
            _gains[i] = 1.0f;
        }
    }

    /// <summary>
    /// Gets or sets the gain for a specific input channel.
    /// </summary>
    /// <param name="inputIndex">Zero-based input channel index</param>
    /// <returns>The gain value (0.0 to disable, 1.0 = unity, >1.0 = boost)</returns>
    public float GetGain(int inputIndex)
    {
        if (inputIndex < 0 || inputIndex >= _inputCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inputIndex));
        }
        return _gains[inputIndex];
    }

    /// <summary>
    /// Gets the name of this mixer node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Sets the gain for a specific input channel.
    /// </summary>
    /// <param name="inputIndex">Zero-based input channel index</param>
    /// <param name="gain">The gain value (0.0 to disable, 1.0 = unity, >1.0 = boost)</param>
    public void SetGain(int inputIndex, float gain)
    {
        if (inputIndex < 0 || inputIndex >= _inputCount)
        {
            throw new ArgumentOutOfRangeException(nameof(inputIndex));
        }

        if (float.IsNaN(gain) || float.IsInfinity(gain))
        {
            throw new ArgumentException("Gain must be a valid finite number", nameof(gain));
        }

        _gains[inputIndex] = gain;
    }

    /// <summary>
    /// Processes audio by summing all input buffers into the output buffer.
    /// </summary>
    /// <param name="inputs">Array of input buffers (one per input channel)</param>
    /// <param name="output">Output buffer to write the mixed result</param>
    /// <exception cref="ArgumentNullException">Thrown if inputs or output is null</exception>
    /// <exception cref="ArgumentException">Thrown if inputs/output dimensions don't match</exception>
    public void Process(float[][] inputs, float[] output)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(output);

        // Validate input count
        if (inputs.Length != _inputCount)
        {
            throw new ArgumentException($"Expected {_inputCount} input buffers, got {inputs.Length}", nameof(inputs));
        }

        // Validate output length
        if (output.Length != _frames)
        {
            throw new ArgumentException($"Output buffer must have {_frames} frames, got {output.Length}", nameof(output));
        }

        // Clear output buffer
        Array.Clear(output, 0, output.Length);

        // Sum all inputs with their respective gains
        for (int inputIdx = 0; inputIdx < _inputCount; inputIdx++)
        {
            float gain = _gains[inputIdx];

            // Skip if gain is zero or input is null
            if (gain == 0.0f || inputs[inputIdx] == null)
            {
                continue;
            }

            float[] inputBuffer = inputs[inputIdx];

            // Validate input buffer length
            if (inputBuffer.Length != _frames)
            {
                throw new ArgumentException(
                    $"Input buffer {inputIdx} must have {_frames} frames, got {inputBuffer.Length}",
                    nameof(inputs));
            }

            // Apply gain and sum
            for (int i = 0; i < _frames; i++)
            {
                output[i] += inputBuffer[i] * gain;
            }
        }
    }

    /// <summary>
    /// Gets the number of input channels this mixer expects.
    /// </summary>
    public int InputCount => _inputCount;

    /// <summary>
    /// Gets the number of audio frames per buffer.
    /// </summary>
    public int Frames => _frames;
}
