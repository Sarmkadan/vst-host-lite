# SilenceDetectorNode

A pass-through node that detects silence in audio buffers. Silently passes through audio data while tracking RMS levels. The node considers audio silent when the RMS level falls below the threshold for the specified number of consecutive buffers.

## Public API

### Constructor

```csharp
public SilenceDetectorNode(int channelCount, int requiredSilentBuffers = 2, float silenceThreshold = 0.0001f)
```

Initializes a new instance of `SilenceDetectorNode`.

#### Parameters
- `channelCount`: Number of interleaved channels the node will process.
- `requiredSilentBuffers`: Number of consecutive buffers below threshold required to consider audio silent. Default is 2.
- `silenceThreshold`: RMS threshold below which audio is considered silent (0.0 to 1.0). Default is 0.0001f.

#### Exceptions
- `ArgumentOutOfRangeException`: When `channelCount` is less than 1.
- `ArgumentOutOfRangeException`: When `requiredSilentBuffers` is less than 1.
- `ArgumentOutOfRangeException`: When `silenceThreshold` is not between 0.0 and 1.0 (exclusive of 0.0, inclusive of 1.0).

### Properties

#### IsSilent
```csharp
public bool IsSilent { get; }
```
Gets whether the audio is currently considered silent.

#### SilentBufferCount
```csharp
public int SilentBufferCount { get; }
```
Gets the number of consecutive silent buffers detected.

### Methods

#### Process(float[] buffer)
```csharp
public void Process(float[] buffer)
```
Processes an audio buffer, updating silence detection statistics. The buffer is passed through unchanged.

##### Parameters
- `buffer`: Interleaved audio samples (length must be a multiple of the channel count).

##### Exceptions
- `ArgumentNullException`: If `buffer` is null.
- `ArgumentException`: If buffer length is not a multiple of the channel count.

#### Process(AudioBuffer buffer)
```csharp
public void Process(AudioBuffer buffer)
```
Processes an `AudioBuffer`, updating silence detection statistics. The buffer is passed through unchanged.

##### Parameters
- `buffer`: Audio buffer to process.

##### Exceptions
- `ArgumentNullException`: If `buffer` is null.
- `ArgumentException`: If `AudioBuffer` channel count does not match `SilenceDetectorNode` channel count.

#### Reset()
```csharp
public void Reset()
```
Resets the silence detection state.