# ClipDetector

A utility class for detecting audio clipping in PCM sample data. Provides static methods to analyze audio buffers and identify clipped samples, along with instance properties to retrieve details about the detected clipping events.

## API

### Detect(float[] samples, float threshold)

Analyzes an array of audio samples to detect clipping.

**Parameters**  
- `samples` (float[]): The audio samples to analyze.  
- `threshold` (float): The absolute value threshold above which a sample is considered clipped.

**Return Value**  
`ClipDetectionResult`: A result object containing clipping statistics.

**Exceptions**  
- `ArgumentNullException`: Thrown when `samples` is null.  
- `ArgumentOutOfRangeException`: Thrown when `threshold` is less than or equal to zero.

---

### Detect(ReadOnlySpan<float> samples, float threshold)

Analyzes a span of audio samples to detect clipping.

**Parameters**  
- `samples` (ReadOnlySpan<float>): The audio samples to analyze.  
- `threshold` (float): The absolute value threshold above which a sample is considered clipped.

**Return Value**  
`ClipDetectionResult`: A result object containing clipping statistics.

**Exceptions**  
- `ArgumentOutOfRangeException`: Thrown when `threshold` is less than or equal to zero.

---

### ClippedSampleCount

Gets the number of samples that exceeded the threshold during the last detection.

**Return Value**  
`int`: The count of clipped samples. Returns 0 if no detection has been performed or no samples were clipped.

---

### MaxAbsoluteValue

Gets the maximum absolute value of all samples analyzed during the last detection.

**Return Value**  
`float`: The highest absolute sample value encountered. Returns 0 if no detection has been performed.

---

### FirstClipIndex

Gets the index of the first clipped sample in the analyzed buffer.

**Return Value**  
`int`: The zero-based index of the first clipped sample. Returns -1 if no samples were clipped.

---

### ToString()

Returns a string representation of the clipping detection results.

**Return Value**  
`string`: A formatted string containing `ClippedSampleCount`, `MaxAbsoluteValue`, and `FirstClipIndex`.

**Exceptions**  
None.

## Usage

### Example 1: Detecting Clipping in an Array

```csharp
float[] audioBuffer = { 0.5f, 0.8f, 1.2f, -1.1f, 0.3f };
float threshold = 1.0f;

var result = ClipDetector.Detect(audioBuffer, threshold);

Console.WriteLine($"Clipped samples: {result.ClippedSampleCount}");
Console.WriteLine($"Max absolute value: {result.MaxAbsoluteValue}");
Console.WriteLine($"First clip at index: {result.FirstClipIndex}");
// Output:
// Clipped samples: 2
// Max absolute value: 1.2
// First clip at index: 2
```

### Example 2: Using Span for Stack-Allocated Data

```csharp
Span<float> audioSpan = stackalloc float[] { 0.9f, 1.05f, 0.7f, -1.0f };
float threshold = 1.0f;

var result = ClipDetector.Detect(audioSpan, threshold);

Console.WriteLine(result.ToString());
// Output:
// ClippedSampleCount=1, MaxAbsoluteValue=1.05, FirstClipIndex=1
```

## Notes

- Both `Detect` overloads perform the same analysis but accept different sample container types. Use `ReadOnlySpan<float>` for stack-allocated or slice-based scenarios to avoid heap allocations.  
- `FirstClipIndex` returns -1 when no clipping is detected. Callers should check `ClippedSampleCount` before using this value.  
- `MaxAbsoluteValue` reflects the peak amplitude regardless of whether clipping occurred. This is useful for gain staging analysis.  
- The static `Detect` methods are thread-safe for concurrent invocations with separate input buffers. However, the instance properties (`ClippedSampleCount`, `MaxAbsoluteValue`, `FirstClipIndex`) are not thread-safe if the same `ClipDetector` instance is reused across threads without synchronization.  
- Empty input buffers (length 0) will result in `ClippedSampleCount=0`, `MaxAbsoluteValue=0`, and `FirstClipIndex=-1`.  
- Thresholds must be positive values. A threshold of 0.0 will throw an exception, as it would classify all samples as clipped.
