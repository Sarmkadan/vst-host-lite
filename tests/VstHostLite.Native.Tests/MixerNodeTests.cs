using System;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests
{
    public class MixerNodeTests
    {
        [Fact]
        public void TwoInputs_SumCorrectly()
        {
            // Arrange
            const int frames = 4;
            var mixer = new MixerNode("test-mixer", inputCount: 2, frames: frames);
            float[][] inputs = new[]
            {
                new float[] { 1f, 2f, 3f, 4f },
                new float[] { 0.5f, 0.5f, 0.5f, 0.5f }
            };
            var output = new float[frames];

            // Act
            mixer.Process(inputs, output);

            // Assert
            var expected = new float[] { 1.5f, 2.5f, 3.5f, 4.5f };
            Assert.Equal(expected, output);
        }

        [Fact]
        public void SingleInput_Passthrough()
        {
            // Arrange
            const int frames = 3;
            var mixer = new MixerNode("single", inputCount: 1, frames: frames);
            float[][] inputs = new[]
            {
                new float[] { 1f, -1f, 0.5f }
            };
            var output = new float[frames];

            // Act
            mixer.Process(inputs, output);

            // Assert
            var expected = new float[] { 1f, -1f, 0.5f };
            Assert.Equal(expected, output);
        }

        [Fact]
        public void InvalidInputCount_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 2;
            var mixer = new MixerNode("invalid-count", inputCount: 2, frames: frames);
            float[][] inputs = new[]
            {
                new float[] { 0f, 0f } // only one input provided
            };
            var output = new float[frames];

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => mixer.Process(inputs, output));
            Assert.Contains("Expected 2 input buffers", ex.Message);
        }

        [Fact]
        public void InvalidInputBufferLength_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 3;
            var mixer = new MixerNode("invalid-buffer", inputCount: 1, frames: frames);
            float[][] inputs = new[]
            {
                new float[] { 1f, 2f } // length 2 instead of 3
            };
            var output = new float[frames];

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => mixer.Process(inputs, output));
            Assert.Contains("must have 3 frames", ex.Message);
        }

        [Fact]
        public void NullInputs_ThrowsArgumentNullException()
        {
            // Arrange
            const int frames = 1;
            var mixer = new MixerNode("null-inputs", inputCount: 1, frames: frames);
            float[][] inputs = null!;
            var output = new float[frames];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => mixer.Process(inputs, output));
        }

        [Fact]
        public void AllGainsZero_ProducesSilence()
        {
            // Arrange
            const int frames = 5;
            var mixer = new MixerNode("silence", inputCount: 2, frames: frames);
            mixer.SetGain(0, 0f);
            mixer.SetGain(1, 0f);

            float[][] inputs = new[]
            {
                new float[] { 1f, 1f, 1f, 1f, 1f },
                new float[] { -1f, -1f, -1f, -1f, -1f }
            };
            var output = new float[frames];

            // Act
            mixer.Process(inputs, output);

            // Assert
            var expected = new float[frames]; // all zeros
            Assert.Equal(expected, output);
        }
    }
}
