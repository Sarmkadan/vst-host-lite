# ParameterSmoother

## Purpose
Smooths a parameter value towards a target using exponential smoothing.
The smoothing speed is defined by a time constant (in seconds) and the sample rate.

## Public API

### Constructor
```csharp
public ParameterSmoother(float sampleRate, float timeConstantSeconds, float initialValue = 0f)
```
Creates a new `ParameterSmoother`.

**Parameters:**
- `sampleRate`: Audio sample rate (samples per second). Must be positive.
- `timeConstantSeconds`: Time constant for the exponential smoothing (seconds). Must be positive.
- `initialValue`: Initial value of the parameter (default: 0f).

**Exceptions:**
- `ArgumentOutOfRangeException` if `sampleRate` or `timeConstantSeconds` is not positive.

### Properties
#### Current
```csharp
public float Current { get; }
```
Gets the current smoothed value.

#### Target
```csharp
public float Target { get; set; }
```
Gets or sets the target value the smoother moves towards.

### Methods
#### SnapToTarget
```csharp
public void SnapToTarget()
```
Instantly jumps the current value to the target value, bypassing smoothing.

#### NextValue
```csharp
public float NextValue()
```
Calculates the next smoothed value for a single sample.

**Returns:**
The next smoothed value.

**Remarks:**
The smoothing formula is: `y[n] = y[n-1] + (target - y[n-1]) * alpha`
where `alpha = 1 - exp(-1 / (sampleRate * timeConstant))`.

#### Process
```csharp
public void Process(float[] destination)
```
Processes a block of samples, writing the smoothed values into the supplied buffer.
The buffer is overwritten with the smoothed values.

**Parameters:**
- `destination`: Array that will receive the smoothed values.

**Exceptions:**
- `ArgumentNullException` if `destination` is null.

### Operators
#### Equality (==)
```csharp
public static bool operator ==(ParameterSmoother? left, ParameterSmoother? right)
```
Returns true if both `ParameterSmoother` instances have the same sample rate and time constant.

#### Inequality (!=)
```csharp
public static bool operator !=(ParameterSmoother? left, ParameterSmoother? right)
```
Returns true if the `ParameterSmoother` instances do not have the same sample rate and time constant.

### Equals and GetHashCode
- `Equals(ParameterSmoother? other)`: Compares sample rate and time constant.
- `Equals(object? obj)`: Delegates to the typed `Equals`.
- `GetHashCode()`: Combines hash codes of sample rate and time constant.

## Implementation Details
- The smoothing coefficient `_alpha` is precomputed as `1f - exp(-1.0 / (sampleRate * timeConstantSeconds))`.
- The class is sealed and implements `IParameterSmoother` and `IEquatable<ParameterSmoother>`.
- The current and target values are stored as `float`.