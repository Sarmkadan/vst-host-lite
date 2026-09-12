# MixerNode

`MixerNode` is a processing node that sums a configured number of input `float[]` buffers into one output buffer. Each input has an independent gain that is applied sample by sample before the input is added to the output. The node processes fixed-size buffers and can be added to an audio graph with `AudioGraph.AddNode()`.

For input `j` and sample `i`, processing produces:

```text
output[i] += inputs[j][i] * gain[j]
```

The output buffer is cleared before the inputs are summed.

## Constructor

### `MixerNode(string name, int inputCount, int frames)`

Creates a mixer for `inputCount` input buffers, each containing `frames` samples.

- `name`: The node name. It must not be `null`.
- `inputCount`: The exact number of input buffers accepted by `Process`. It must be greater than zero.
- `frames`: The configured length of the output buffer and each processed input buffer. It must be greater than zero.

Every input gain is initialized to `1.0f` (unity gain).

Exceptions:

- `ArgumentNullException` when `name` is `null`.
- `ArgumentOutOfRangeException` when `inputCount` or `frames` is less than or equal to zero.

## Properties

### `string Name { get; }`

Gets the node name supplied to the constructor.

### `int InputCount { get; }`

Gets the configured number of input buffers.

### `int Frames { get; }`

Gets the configured number of samples per buffer.

## Methods

### `float GetGain(int inputIndex)`

Gets the gain for the zero-based input index. A gain of `0.0f` disables that input, `1.0f` is unity gain, and values greater than `1.0f` boost it.

Throws `ArgumentOutOfRangeException` when `inputIndex` is less than zero or greater than or equal to `InputCount`.

### `void SetGain(int inputIndex, float gain)`

Sets the gain for the zero-based input index. The gain must be finite. The implementation accepts zero, positive, and negative finite values; it does not otherwise restrict the gain range.

- `ArgumentOutOfRangeException` is thrown when `inputIndex` is less than zero or greater than or equal to `InputCount`.
- `ArgumentException` is thrown when `gain` is `NaN` or infinity.

### `void Process(float[][] inputs, float[] output)`

Clears `output`, applies each input's gain, and sums the resulting samples into `output`.

`inputs` must contain exactly `InputCount` buffers, and none of its entries may be `null`. `output` must contain exactly `Frames` samples. Each input with a nonzero gain must also contain exactly `Frames` samples. An input whose gain is exactly `0.0f` is skipped before its buffer length is checked.

- `ArgumentNullException` is thrown when `inputs` or `output` is `null`.
- `ArgumentException` is thrown when the input count is incorrect, the output length is incorrect, an input entry is `null`, or a nonzero-gain input buffer has an incorrect length.
