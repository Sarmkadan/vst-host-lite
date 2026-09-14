# NoiseGeneratorNode

## Purpose
A node that generates white noise. The noise is generated using a `System.Random` instance seeded either explicitly or with the current time. The amplitude can be set in the range 0.0‑1.0 (inclusive). This node follows the same conventions as `MixerNode` and can be added to an `AudioGraph` via custom handling if required.

## Constructor
```csharp
public NoiseGeneratorNode(string name, int frames, int? seed = null)
```
Creates a new `NoiseGeneratorNode`.

### Parameters
- `name`: Node name for identification. Must not be null.
- `frames`: Number of audio frames per buffer. Must be positive.
- `seed`: Optional seed for the random number generator. If `null`, the generator is seeded with `Environment.TickCount`.

### Exceptions
- `ArgumentNullException`: If `name` is null.
- `ArgumentOutOfRangeException`: If `frames` is not positive.

## Properties
### Name
```csharp
public string Name { get; }
```
Gets the name of this noise generator node.

### Amplitude
```csharp
public float Amplitude { get; set; }
```
Gets or sets the amplitude of the generated noise. Value must be between 0.0 and 1.0 inclusive.

### Exceptions
- `ArgumentOutOfRangeException`: If the value is not a finite number in the range [`MinAmplitude`, `MaxAmplitude`] (defined in `NoiseGeneratorNodeConstants`).

### Frames
```csharp
public int Frames => _frames;
```
Gets the number of frames per buffer for this node.

## Methods
### Process
```csharp
public void Process(float[] output)
```
Generates a block of white noise into the provided output buffer.

#### Parameters
- `output`: Output buffer that will receive the generated samples. Its length must match the `frames` value supplied to the constructor.

#### Exceptions
- `ArgumentNullException`: If `output` is null.
- `ArgumentException`: If `output` length does not match the configured frame count.

#### Remarks
The noise is generated in the range [-1, 1] and then scaled by the `Amplitude` property.

### Equals
```csharp
public bool Equals(NoiseGeneratorNode? other)
```
Indicates whether the current object is equal to another object of the same type.

#### Returns
`true` if the current object is equal to the `other` parameter; otherwise, `false`.

### Equals (object)
```csharp
public override bool Equals(object? obj)
```
Determines whether the specified object is equal to the current object.

#### Returns
`true` if the specified object is a `NoiseGeneratorNode` and is equal to the current object; otherwise, `false`.

### GetHashCode
```csharp
public override int GetHashCode()
```
Serves as the default hash function.

#### Returns
A hash code for the current object.

### Operators
```csharp
public static bool operator ==(NoiseGeneratorNode? left, NoiseGeneratorNode? right)
```
Returns a value that indicates whether two `NoiseGeneratorNode` instances are equal.

```csharp
public static bool operator !=(NoiseGeneratorNode? left, NoiseGeneratorNode? right)
```
Returns a value that indicates whether two `NoiseGeneratorNode` instances are not equal.

## Remarks
- The class is `sealed` and implements `INoiseGeneratorNode` and `IEquatable<NoiseGeneratorNode>`.
- The random number generator is initialized once in the constructor and reused for all calls to `Process`.
- The amplitude defaults to `NoiseGeneratorNodeConstants.DefaultAmplitude` (full amplitude).
- The noise generation uses `Random.NextDouble()` to produce values in [0.0, 1.0), which are shifted to [-1.0, 1.0) and then scaled by the amplitude.