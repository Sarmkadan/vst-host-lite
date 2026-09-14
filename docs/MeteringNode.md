# MeteringNode

A pass‑through node that tracks the running peak and RMS (root‑mean‑square) levels per channel. The node does not modify the audio data; it simply observes it. Call `Reset` to clear the accumulated statistics. The current statistics can be obtained via the `CurrentMetering` property, which returns a `Metering` record.

## Namespace

`VstHostLite.Native`

## Constructors

### MeteringNode(int channelCount)

Initializes a new instance of `MeteringNode`.

#### Parameters

- `channelCount`: Number of interleaved channels the node will process.

#### Exceptions

- `ArgumentOutOfRangeException`: When `channelCount` is less than 1.

## Methods

### Process(float[] buffer)

Processes an interleaved audio buffer, updating peak and RMS statistics. The buffer is left unchanged.

#### Parameters

- `buffer`: Interleaved audio samples (length must be a multiple of the channel count).

#### Exceptions

- `ArgumentNullException`: If `buffer` is null.
- `ArgumentException`: If `buffer` length is not a multiple of the channel count.

### Reset()

Resets all accumulated statistics (peak, RMS and sample count).

## Properties

### CurrentMetering

Gets the current metering values as a `Metering` record.

## Metering

Record that holds per‑channel peak and RMS values.

#### Properties

- `Peak`: Array of peak values (absolute maximum) per channel.
- `RMS`: Array of RMS values per channel.