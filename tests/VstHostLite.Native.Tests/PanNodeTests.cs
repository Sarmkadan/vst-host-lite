using System;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests
{
    public class PanNodeTests
    {
        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange & Act
            var panNode = new PanNode("test-pan", 128);

            // Assert
            Assert.Equal("test-pan", panNode.Name);
            Assert.Equal(128, panNode.Frames);
            Assert.Equal(0.0f, panNode.Pan);
        }

        [Fact]
        public void Constructor_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PanNode(null!, 128));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Constructor_WithNonPositiveFrames_ThrowsArgumentOutOfRangeException(int frames)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new PanNode("test", frames));
        }

        [Fact]
        public void Name_Getter_ReturnsCorrectValue()
        {
            // Arrange
            var panNode = new PanNode("left-channel", 256);

            // Act
            var name = panNode.Name;

            // Assert
            Assert.Equal("left-channel", name);
        }

        [Fact]
        public void Frames_Getter_ReturnsCorrectValue()
        {
            // Arrange
            const int expectedFrames = 512;
            var panNode = new PanNode("test", expectedFrames);

            // Act
            var frames = panNode.Frames;

            // Assert
            Assert.Equal(expectedFrames, frames);
        }

        [Fact]
        public void Pan_Getter_ReturnsDefaultValue()
        {
            // Arrange
            var panNode = new PanNode("test", 128);

            // Act
            var pan = panNode.Pan;

            // Assert
            Assert.Equal(0.0f, pan);
        }

        [Theory]
        [InlineData(-1.0f)]
        [InlineData(0.0f)]
        [InlineData(1.0f)]
        public void Pan_Getter_ReturnsSetValue(float panValue)
        {
            // Arrange
            var panNode = new PanNode("test", 128);
            panNode.Pan = panValue;

            // Act
            var pan = panNode.Pan;

            // Assert
            Assert.Equal(panValue, pan);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void Pan_Setter_WithInvalidValue_ThrowsArgumentException(float invalidPan)
        {
            // Arrange
            var panNode = new PanNode("test", 128);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => panNode.Pan = invalidPan);
        }

        [Theory]
        [InlineData(-1.1f)]
        [InlineData(1.1f)]
        [InlineData(2.0f)]
        [InlineData(-2.0f)]
        public void Pan_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(float invalidPan)
        {
            // Arrange
            var panNode = new PanNode("test", 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => panNode.Pan = invalidPan);
        }

        [Fact]
        public void Process_WithPanCenter_OutputsEqualLeftAndRight()
        {
            // Arrange
            const int frames = 4;
            var panNode = new PanNode("center-pan", frames);

            var monoInput = new float[] { 1.0f, 0.5f, 0.0f, -0.5f };
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - At center (pan=0), cos(π/4) = sin(π/4) = √2/2 ≈ 0.707
            // So both channels should have the same output
            for (int i = 0; i < frames; i++)
            {
                Assert.Equal(leftOutput[i], rightOutput[i], 5);
            }
        }

        [Fact]
        public void Process_WithPanFullyLeft_LeftChannelHasFullGainRightChannelSilent()
        {
            // Arrange
            const int frames = 3;
            var panNode = new PanNode("left-pan", frames);
            panNode.Pan = -1.0f; // Fully left

            var monoInput = new float[] { 1.0f, 0.5f, 0.0f };
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - At pan=-1, angle=0, cos(0)=1, sin(0)=0
            // Left channel should have full input, right channel should be silent
            Assert.Equal(monoInput[0], leftOutput[0], 5);
            Assert.Equal(monoInput[1], leftOutput[1], 5);
            Assert.Equal(monoInput[2], leftOutput[2], 5);
            Assert.Equal(0.0f, rightOutput[0], 5);
            Assert.Equal(0.0f, rightOutput[1], 5);
            Assert.Equal(0.0f, rightOutput[2], 5);
        }

        [Fact]
        public void Process_WithPanFullyRight_RightChannelHasFullGainLeftChannelSilent()
        {
            // Arrange
            const int frames = 3;
            var panNode = new PanNode("right-pan", frames);
            panNode.Pan = 1.0f; // Fully right

            var monoInput = new float[] { 1.0f, 0.5f, 0.0f };
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - At pan=1, angle=π/2, cos(π/2)=0, sin(π/2)=1
            // Right channel should have full input, left channel should be silent
            Assert.Equal(0.0f, leftOutput[0], 5);
            Assert.Equal(0.0f, leftOutput[1], 5);
            Assert.Equal(0.0f, leftOutput[2], 5);
            Assert.Equal(monoInput[0], rightOutput[0], 5);
            Assert.Equal(monoInput[1], rightOutput[1], 5);
            Assert.Equal(monoInput[2], rightOutput[2], 5);
        }

        [Fact]
        public void Process_WithPanHalfLeft_AppliesCorrectGains()
        {
            // Arrange
            const int frames = 2;
            var panNode = new PanNode("half-left", frames);
            panNode.Pan = -0.5f; // Half left

            var monoInput = new float[] { 1.0f, 0.5f };
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - At pan=-0.5, angle=(π/4)*(1-0.5)=π/8
            // cos(π/8) ≈ 0.9239, sin(π/8) ≈ 0.3827
            // Left channel should have ~92% of input, right channel ~38%
            Assert.Equal(1.0f * 0.9239f, leftOutput[0], 3);
            Assert.Equal(0.5f * 0.9239f, leftOutput[1], 3);
            Assert.Equal(1.0f * 0.3827f, rightOutput[0], 3);
            Assert.Equal(0.5f * 0.3827f, rightOutput[1], 3);
        }

        [Fact]
        public void Process_WithPanHalfRight_AppliesCorrectGains()
        {
            // Arrange
            const int frames = 2;
            var panNode = new PanNode("half-right", frames);
            panNode.Pan = 0.5f; // Half right

            var monoInput = new float[] { 1.0f, 0.5f };
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - At pan=0.5, angle=(π/4)*(1+0.5)=3π/8
            // cos(3π/8) ≈ 0.3827, sin(3π/8) ≈ 0.9239
            // Left channel should have ~38% of input, right channel ~92%
            Assert.Equal(1.0f * 0.3827f, leftOutput[0], 3);
            Assert.Equal(0.5f * 0.3827f, leftOutput[1], 3);
            Assert.Equal(1.0f * 0.9239f, rightOutput[0], 3);
            Assert.Equal(0.5f * 0.9239f, rightOutput[1], 3);
        }

        [Fact]
        public void Process_WithNullMonoInput_ThrowsArgumentNullException()
        {
            // Arrange
            const int frames = 2;
            var panNode = new PanNode("test", frames);
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => panNode.Process(null!, leftOutput, rightOutput));
        }

        [Fact]
        public void Process_WithNullLeftOutput_ThrowsArgumentNullException()
        {
            // Arrange
            const int frames = 2;
            var panNode = new PanNode("test", frames);
            var monoInput = new float[frames];
            var rightOutput = new float[frames];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => panNode.Process(monoInput, null!, rightOutput));
        }

        [Fact]
        public void Process_WithNullRightOutput_ThrowsArgumentNullException()
        {
            // Arrange
            const int frames = 2;
            var panNode = new PanNode("test", frames);
            var monoInput = new float[frames];
            var leftOutput = new float[frames];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => panNode.Process(monoInput, leftOutput, null!));
        }

        [Fact]
        public void Process_WithMismatchedInputLength_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 4;
            var panNode = new PanNode("test", frames);
            var monoInput = new float[] { 1.0f, 0.5f, 0.0f }; // Wrong length
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => panNode.Process(monoInput, leftOutput, rightOutput));
            Assert.Contains("must have 4 frames", ex.Message);
        }

        [Fact]
        public void Process_WithMismatchedLeftOutputLength_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 4;
            var panNode = new PanNode("test", frames);
            var monoInput = new float[frames];
            var leftOutput = new float[] { 1.0f, 0.5f }; // Wrong length
            var rightOutput = new float[frames];

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => panNode.Process(monoInput, leftOutput, rightOutput));
            Assert.Contains("must have 4 frames", ex.Message);
        }

        [Fact]
        public void Process_WithMismatchedRightOutputLength_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 4;
            var panNode = new PanNode("test", frames);
            var monoInput = new float[frames];
            var leftOutput = new float[frames];
            var rightOutput = new float[] { 1.0f, 0.5f }; // Wrong length

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => panNode.Process(monoInput, leftOutput, rightOutput));
            Assert.Contains("must have 4 frames", ex.Message);
        }

        [Fact]
        public void Process_WithAllZeroInput_ProducesAllZeroOutput()
        {
            // Arrange
            const int frames = 5;
            var panNode = new PanNode("test", frames);
            panNode.Pan = 0.75f; // Any pan position

            var monoInput = new float[frames]; // All zeros
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - All outputs should be zero
            for (int i = 0; i < frames; i++)
            {
                Assert.Equal(0.0f, leftOutput[i], 5);
                Assert.Equal(0.0f, rightOutput[i], 5);
            }
        }

        [Fact]
        public void Process_WithLargeBuffer_ProcessesAllFrames()
        {
            // Arrange
            const int frames = 8192;
            var panNode = new PanNode("large-buffer", frames);
            panNode.Pan = 0.33f;

            var monoInput = new float[frames];
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Fill input with some data (avoid sin(0) which is always 0)
            for (int i = 0; i < frames; i++)
            {
                monoInput[i] = (float)Math.Sin(i * 0.1f + 1.0f);
            }

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - All frames should be processed (outputs should differ from inputs due to gain)
            for (int i = 0; i < frames; i++)
            {
                Assert.NotEqual(monoInput[i], leftOutput[i]);
                Assert.NotEqual(monoInput[i], rightOutput[i]);
            }
        }

        [Fact]
        public void Process_WithNegativeInputValues_AppliesGainsCorrectly()
        {
            // Arrange
            const int frames = 2;
            var panNode = new PanNode("negative-test", frames);
            panNode.Pan = -0.3f;

            var monoInput = new float[] { -1.0f, -0.5f };
            var leftOutput = new float[frames];
            var rightOutput = new float[frames];

            // Act
            panNode.Process(monoInput, leftOutput, rightOutput);

            // Assert - Gains should be applied correctly to negative values
            Assert.NotEqual(monoInput[0], leftOutput[0]);
            Assert.NotEqual(monoInput[1], leftOutput[1]);
        }
    }
}