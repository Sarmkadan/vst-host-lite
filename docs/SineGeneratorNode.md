# SineGeneratorNode

A sine wave generator node that produces a mono audio signal with configurable frequency, amplitude, sample rate, and phase. This is a processing node that can be added to the audio graph.

## Constructor

```csharp
public SineGeneratorNode(string name, float sampleRate, int frames)
```

### Parameters

- `name`: Node name for identification. Cannot be null.
- `sampleRate`: Audio sample rate in Hz. Must be positive.
- `frames`: Number of audio frames per buffer. Must be positive.

### Exceptions

- `ArgumentNullException`: If `name` is null.
- `ArgumentOutOfRangeException`: If `sampleRate` is not positive.
- `ArgumentOutOfRangeException`: If `frames` is not positive.

## Properties

### Name

```csharp
public string Name { get; }
```

Gets the name of this sine generator node.

### Amplitude

```csharp
public float Amplitude { get; set; }
```

Gets or sets the output amplitude (0.0 to 1.0).

#### Exceptions

- `ArgumentException`: If the value is not a finite number (NaN or infinity).
- `ArgumentOutOfRangeException`: If the value is less than 0.0 or greater than 1.0.

### Frequency

```csharp
public float Frequency { get; set; }
```

Gets or sets the output frequency in Hz.

#### Exceptions

- `ArgumentException`: If the value is not a finite number (NaN or infinity).
- `ArgumentOutOfRangeException`: If the value is negative.

### SampleRate

```csharp
public float SampleRate { get; }
```

Gets the current sample rate in Hz.

### Frames

```csharp
public int Frames { get; }
```

Gets the number of audio frames per buffer.

## Methods

### Generate

```csharp
public void Generate(float[] buffer)
```

Generates a sine wave into the provided mono buffer.

#### Parameters

- `buffer`: Output buffer to fill with generated audio. Must have length equal to `Frames`.

#### Exceptions

- `ArgumentNullException`: If `buffer` is null.
- `ArgumentException`: If `buffer` length does not match `Frames`.

#### Remarks

The method fills the buffer with sine wave samples calculated using the current frequency, amplitude, and phase. The phase is updated for each sample and wrapped to the range [-2π, 2π] to maintain numerical precision.

### Reset

```csharp
public void Reset()
```

Resets the phase accumulator to zero.

## Thread Safety

This class is not thread-safe. Concurrent access from multiple threads requires external synchronization.