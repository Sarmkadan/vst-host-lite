# AudioBuffer

Represents a multi-channel audio buffer with a fixed number of frames, used for storing and manipulating raw audio sample data in the VST host environment.

## API

### `int Channels`
Gets the number of audio channels in the buffer.

- **Return value**: The number of channels (e.g., 1 for mono, 2 for stereo).
- **Exceptions**: None.

---

### `int Frames`
Gets the number of audio frames (samples per channel) in the buffer.

- **Return value**: The total number of frames.
- **Exceptions**: None.

---

### `AudioBuffer(int channels, int frames)`
Initializes a new instance of the `AudioBuffer` class with the specified number of channels and frames.

- **Parameters**:
  - `channels` (int): The number of channels. Must be non-negative.
  - `frames` (int): The number of frames. Must be non-negative.
- **Exceptions**:
  - `ArgumentOutOfRangeException`: Thrown if `channels` or `frames` is negative.

---

### `void Clear()`
Sets all samples in the buffer to zero.

- **Parameters**: None.
- **Return value**: void.
- **Exceptions**:
  - `InvalidOperationException`: Thrown if the buffer has not been initialized (e.g., `Channels` or `Frames` is zero).

---

### `void CopyFrom(AudioBuffer source, int startFrame = 0)`
Copies samples from another `AudioBuffer` into this buffer, starting at the specified frame index.

- **Parameters**:
  - `source` (AudioBuffer): The source buffer to copy from. Must not be null.
  - `startFrame` (int, optional): The starting frame index in this buffer. Defaults to 0.
- **Return value**: void.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `source` is null.
  - `ArgumentOutOfRangeException`: Thrown if `startFrame` is negative or exceeds the buffer's frame count.
  - `InvalidOperationException`: Thrown if the source buffer's channel count does not match this buffer's channel count.

---

### `float[] ToFlatArray()`
Converts the buffer's samples into a flat array of floats, with interleaved channel data (e.g., for stereo: [L0, R0, L1, R1, ...]).

- **Parameters**: None.
- **Return value**: A new `float[]` containing all samples in interleaved format.
- **Exceptions**: None.

---

### `static AudioBuffer Interleave(float[][] channels, int frames)`
Creates a new interleaved `AudioBuffer` from separate channel arrays.

- **Parameters**:
  - `channels` (float[][]): An array of per-channel sample arrays. Each array must have exactly `frames` elements.
  - `frames` (int): The number of frames per channel.
- **Return value**: A new `AudioBuffer` with interleaved samples.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `channels` is null.
  - `ArgumentException`: Thrown if any channel array has a length different from `frames`.

---

### `static float[][] Deinterleave(AudioBuffer buffer, int channels, int frames)`
Splits an interleaved `AudioBuffer` into separate channel arrays.

- **Parameters**:
  - `buffer` (AudioBuffer): The interleaved buffer to split. Must not be null.
  - `channels` (int): The number of channels to extract.
  - `frames` (int): The number of frames per channel.
- **Return value**: A `float[][]` where each element is a per-channel array of samples.
- **Exceptions**:
  - `ArgumentNullException`: Thrown if `buffer` is null.
  - `ArgumentException`: Thrown if `channels` or `frames` does not match the buffer's dimensions.

---

## Usage

### Example 1: Basic buffer manipulation
```csharp
// Create a stereo buffer with 512 frames
var buffer = new AudioBuffer(channels: 2, frames: 512);

// Clear the buffer to silence
buffer.Clear();

// Create another buffer to copy from
var source = new AudioBuffer(channels: 2, frames: 256);
source.Clear(); // Initialize with zeros

// Copy source into buffer starting at frame 100
buffer.CopyFrom(source, startFrame: 100);
```

### Example 2: Interleaving and deinterleaving
```csharp
// Prepare separate mono channels
float[] left = { 0.1f, 0.2f, 0.3f };
float[] right = { 0.4f, 0.5f, 0.6f };

// Interleave into a stereo buffer
var interleaved = AudioBuffer.Interleave(new[] { left, right }, frames: 3);

// Deinterleave back into separate channels
float[][] channels = AudioBuffer.Deinterleave(interleaved, channels: 2, frames: 3);
```

---

## Notes

- `Channels` and `Frames` are read-only properties set during construction. Attempting to modify them will throw an exception.
- `Clear()` and `CopyFrom()` require the buffer to be properly initialized (non-zero `Channels` and `Frames`).
- `ToFlatArray()` returns a copy of the buffer's data. Modifying the returned array does not affect the original buffer.
- `Interleave()` and `Deinterleave()` assume strict adherence to the provided `channels` and `frames` parameters. Mismatches will result in exceptions.
- This type is not thread-safe. Concurrent access to the same instance from multiple threads may result in data corruption or undefined behavior. External synchronization is required for multi-threaded scenarios.
