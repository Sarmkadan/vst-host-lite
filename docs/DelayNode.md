# DelayNode

`DelayNode` is a mono processing node that implements a circular-buffer delay line with feedback and a dry/wet mix. It processes fixed-length `float[]` buffers and retains delayed samples between calls to `Process` until `Reset` is called.

## Constructor

### `DelayNode(string name, float maxDelayTimeMs, int sampleRate, int frames)`

Creates a delay node and allocates its delay buffer.

- `name`: The node name. It must not be `null`.
- `maxDelayTimeMs`: The maximum delay time in milliseconds. It must be greater than zero.
- `sampleRate`: The audio sample rate in hertz. It must be greater than zero.
- `frames`: The exact number of samples accepted by each input and output buffer passed to `Process`. It must be greater than zero.

The maximum delay length is calculated as `(int)(maxDelayTimeMs * sampleRate / 1000.0f)` and is at least one sample. The initial delay is 250 milliseconds converted to samples, then clamped to the available range. `Feedback` and `DryWetMix` both initially equal `0.5f`.

Exceptions:

- `ArgumentNullException` when `name` is `null`.
- `ArgumentOutOfRangeException` when `maxDelayTimeMs`, `sampleRate`, or `frames` is less than or equal to zero.

## Properties

### `string Name { get; }`

Gets the node name supplied to the constructor.

### `int DelaySamples { get; set; }`

Gets or sets the current delay length in samples. Valid values range from zero through `MaxDelaySamples`, inclusive. Setting a value outside that range throws `ArgumentOutOfRangeException`.

### `float Feedback { get; set; }`

Gets or sets the feedback amount. The value must be finite and in the inclusive range `0.0f` to `1.0f`. The default is `0.5f`.

- `ArgumentException` is thrown for `NaN` or infinity.
- `ArgumentOutOfRangeException` is thrown for a finite value outside the valid range.

### `float DryWetMix { get; set; }`

Gets or sets the dry/wet mix. `0.0f` selects the dry input and `1.0f` selects the processed output. The value must be finite and in the inclusive range `0.0f` to `1.0f`. The default is `0.5f`.

- `ArgumentException` is thrown for `NaN` or infinity.
- `ArgumentOutOfRangeException` is thrown for a finite value outside the valid range.

### `int MaxDelaySamples { get; }`

Gets the maximum supported delay length in samples, calculated when the node is constructed.

## Methods

### `void Process(float[] input, float[] output)`

Processes one fixed-length mono buffer. Both arrays must contain exactly the `frames` count supplied to the constructor. The output array is cleared before samples are processed.

For each sample, the node reads from the circular buffer at the current delay offset, calculates `inputSample + delayedSample * Feedback`, stores that result back into the delay buffer, and writes the dry/wet blend to the output:

```text
output = inputSample * (1 - DryWetMix) + processedSample * DryWetMix
```

- `ArgumentNullException` is thrown when `input` or `output` is `null`.
- `ArgumentException` is thrown when either array length differs from the configured frame count.

### `void Reset()`

Clears every sample in the delay buffer and resets the circular-buffer write position to zero. The configured delay, feedback, and dry/wet mix are unchanged.

### `bool Equals(DelayNode? other)`

Returns `true` when `other` has the same maximum delay length, frame count, delay-buffer contents, circular-buffer write position, feedback, and dry/wet mix. The node name and current `DelaySamples` value are not compared. Returns `false` when `other` is `null`.

### `bool Equals(object? obj)`

Returns the result of `Equals(DelayNode?)` when `obj` is a `DelayNode`; otherwise returns `false`.

### `int GetHashCode()`

Returns a hash code composed from the maximum delay length, frame count, delay-buffer array, circular-buffer write position, feedback, and dry/wet mix.

## Operators

### `DelayNode? left == DelayNode? right`

Compares two nodes using `EqualityComparer<DelayNode>.Default`.

### `DelayNode? left != DelayNode? right`

Returns the inverse of the equality operator.
