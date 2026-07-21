using System;
using System.Collections.Generic;

namespace VstHostLite.Native
{
    /// <summary>
    /// Provides extension methods for <see cref="AudioBuffer"/> operations.
    /// </summary>
    public static class AudioBufferExtensions
    {
        /// <summary>
        /// Creates a new audio buffer with the same dimensions and copies the data from this buffer.
        /// </summary>
        /// <param name="buffer">The source audio buffer.</param>
        /// <returns>A new audio buffer with identical data.</returns>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        public static AudioBuffer Clone(this AudioBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            var clone = new AudioBuffer(buffer.Channels, buffer.Frames);
            Array.Copy(buffer.ToFlatArray(), clone.ToFlatArray(), buffer.ToFlatArray().Length);
            return clone;
        }

        /// <summary>
        /// Copies data from a source buffer to a specific channel and frame offset in this buffer.
        /// </summary>
        /// <param name="source">The source audio buffer to copy from.</param>
        /// <param name="targetChannel">The target channel index in this buffer.</param>
        /// <param name="targetFrameOffset">The starting frame offset in this buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
        /// <exception cref="ArgumentException">Thrown when source has different number of channels or when target parameters are out of range.</exception>
        public static void CopyToChannel(this AudioBuffer buffer, AudioBuffer source, int targetChannel, int targetFrameOffset = 0)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(source);

            if (targetChannel < 0 || targetChannel >= buffer.Channels)
                throw new ArgumentException($"Target channel {targetChannel} is out of range [0, {buffer.Channels - 1}]", nameof(targetChannel));

            if (targetFrameOffset < 0 || targetFrameOffset >= buffer.Frames)
                throw new ArgumentException($"Target frame offset {targetFrameOffset} is out of range [0, {buffer.Frames - 1}]", nameof(targetFrameOffset));

            if (source.Channels != 1)
                throw new ArgumentException("Source buffer must have exactly one channel", nameof(source));

            int availableFrames = buffer.Frames - targetFrameOffset;
            int framesToCopy = Math.Min(source.Frames, availableFrames);

            for (int i = 0; i < framesToCopy; i++)
            {
                buffer[targetChannel, targetFrameOffset + i] = source[i, 0];
            }
        }

        /// <summary>
        /// Gets an enumerator to iterate through each channel's data as a separate buffer.
        /// </summary>
        /// <param name="buffer">The audio buffer to iterate over.</param>
        /// <returns>An enumerable of single-channel audio buffers.</returns>
        /// <exception cref="ArgumentNullException">Thrown when buffer is null.</exception>
        public static IEnumerable<AudioBuffer> GetChannelBuffers(this AudioBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            for (int channel = 0; channel < buffer.Channels; channel++)
            {
                var channelBuffer = new AudioBuffer(1, buffer.Frames);
                for (int frame = 0; frame < buffer.Frames; frame++)
                {
                    channelBuffer[0, frame] = buffer[channel, frame];
                }
                yield return channelBuffer;
            }
        }

        /// <summary>
        /// Mixes this audio buffer with another buffer by adding their samples.
        /// </summary>
        /// <param name="buffer">The target audio buffer.</param>
        /// <param name="other">The audio buffer to mix with.</param>
        /// <param name="mixFactor">The factor to multiply the other buffer's samples by before adding (0.0 to 1.0).</param>
        /// <exception cref="ArgumentNullException">Thrown when buffer or other is null.</exception>
        /// <exception cref="ArgumentException">Thrown when buffers have different dimensions.</exception>
        public static void MixWith(this AudioBuffer buffer, AudioBuffer other, float mixFactor = 1.0f)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(other);

            if (buffer.Channels != other.Channels || buffer.Frames != other.Frames)
                throw new ArgumentException("AudioBuffer dimensions must match for mixing", nameof(other));

            if (mixFactor < 0.0f || mixFactor > 1.0f)
                throw new ArgumentException("Mix factor must be between 0.0 and 1.0", nameof(mixFactor));

            float[] otherSamples = other.ToFlatArray();
            float[] thisSamples = buffer.ToFlatArray();

            for (int i = 0; i < thisSamples.Length; i++)
            {
                thisSamples[i] += otherSamples[i] * mixFactor;
            }
        }
    }
}