# SampleRateConverter

Static utility class for converting audio sample rates using linear interpolation.

## Purpose

Provides a simple, dependency-free method to resample audio data from one sample rate to another using linear interpolation. This is useful for audio processing tasks where sample rate conversion is needed without external libraries.

## Public API

### `SampleRateConverter.Convert`

```csharp
public static float[] Convert(float[] input, int inputRate, int outputRate)
```

Converts an array of audio samples from `inputRate` Hz to `outputRate` Hz.

#### Parameters

- `input`: Array of audio samples as 32-bit floating point values in the range [-1.0, 1.0].
- `inputRate`: Sample rate of the input audio in Hz (e.g., 44100).
- `outputRate`: Desired output sample rate in Hz (e.g., 48000).

#### Returns

A new array containing the resampled audio data. The length of the array is calculated as:
`Math.Round(input.Length * (double)outputRate / inputRate)`

#### Behavior

- If `input.Length` is 0, returns an empty array.
- If `inputRate` equals `outputRate`, returns the input array unchanged (no allocation).
- Otherwise, performs linear interpolation to compute each output sample:
  - For each output sample index `i`, computes the corresponding position in the input stream: `position = i * inputRate / outputRate`
  - Uses the integer part of `position` as the base index and the fractional part for interpolation between adjacent input samples.
  - Handles edge cases by clamping to the last input sample when the position exceeds the input bounds.

#### Notes

- This implementation uses linear interpolation which is computationally efficient but may not provide the highest quality resampling for all audio signals.
- The method does not modify the input array; it always returns a new array unless the sample rates are equal.
- For real-time audio processing, consider the allocation overhead when sample rates differ.

## Example Usage

```csharp
// Convert 44.1kHz audio to 48kHz
float[] input = LoadAudioFromFile("audio.wav"); // 44100 Hz
float[] output = SampleRateConverter.Convert(input, 44100, 48000);
// output now contains 48kHz audio
```

## Thread Safety

This class is thread-safe as it contains no mutable state and only operates on its parameters.