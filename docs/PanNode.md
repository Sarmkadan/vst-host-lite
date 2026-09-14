# PanNode

## Purpose
A panning node that implements constant-power panning using the cos/sin law. This ensures that the perceived volume remains constant as the pan position changes. Pan value of -1.0 pans fully left, 0.0 pans center, and 1.0 pans fully right. This is a processing node that can be added to the audio graph.

## Constructor
### PanNode(string name, int frames)
Creates a new PanNode.

- **name**: Node name for identification (cannot be null)
- **frames**: Number of audio frames per buffer (must be positive)

## Properties
### Name
Gets the name of this pan node.

### Pan
Gets or sets the pan position (-1.0 = fully left, 0.0 = center, 1.0 = fully right).
Setting this value immediately changes the pan position without smoothing.

### Frames
Gets the number of audio frames per buffer.

## Methods
### SetPanSmoothed(float target, int rampFrames)
Smoothly changes the pan position over the specified number of audio frames.

- **target**: Target pan position (-1.0 = fully left, 1.0 = fully right)
- **rampFrames**: Number of audio frames over which to reach the target (must be positive)

### Process(float[] monoInput, float[] left, float[] right)
Processes a mono input buffer and pans it to stereo output using constant-power panning.

- **monoInput**: Mono input audio buffer (must have Frames length)
- **left**: Left output buffer to write the panned result (must have Frames length)
- **right**: Right output buffer to write the panned result (must have Frames length)

**Exceptions**:
- `ArgumentNullException`: Thrown if any buffer is null
- `ArgumentException`: Thrown if buffer lengths don't match

**Implementation Details**:
Constant-power panning uses the cos/sin law:
- Left channel: cos(θ) * input
- Right channel: sin(θ) * input
- where θ = (π/4) * (1.0 + pan)
This ensures constant power across the pan range.