using System;

namespace VstHostLite.Native
{
    /// <summary>
    /// Options for audio processing, including sample rate, block size, and channel count.
    /// </summary>
    public sealed class AudioProcessingOptions
    {
        /// <summary>
        /// Gets the sample rate in Hz.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the block size (number of samples per channel per block).
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// Gets the number of audio channels.
        /// </summary>
        public int ChannelCount { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioProcessingOptions"/> class.
        /// </summary>
        /// <param name="sampleRate">The sample rate in Hz. Must be positive.</param>
        /// <param name="blockSize">The block size (samples per channel). Must be positive.</param>
        /// <param name="channelCount">The number of channels. Must be positive.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any parameter is less than or equal to zero.
        /// </exception>
        public AudioProcessingOptions(int sampleRate, int blockSize, int channelCount)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be positive.");
            if (blockSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(blockSize), "Block size must be positive.");
            if (channelCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(channelCount), "Channel count must be positive.");

            SampleRate = sampleRate;
            BlockSize = blockSize;
            ChannelCount = channelCount;
        }
    }
}