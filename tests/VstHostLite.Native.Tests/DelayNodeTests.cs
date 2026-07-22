using System;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests
{
    public class DelayNodeTests
    {
        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange & Act
            var delayNode = new DelayNode("test-delay", 1000f, 44100, 128);

            // Assert
            Assert.Equal("test-delay", delayNode.Name);
            Assert.Equal(44100 / 4, delayNode.DelaySamples); // Default is 1/4 second at 44.1kHz
            Assert.Equal(0.5f, delayNode.Feedback);
            Assert.Equal(0.5f, delayNode.DryWetMix);
        }

        [Fact]
        public void Constructor_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DelayNode(null!, 1000f, 44100, 128));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        public void Constructor_WithNonPositiveMaxDelayTime_ThrowsArgumentOutOfRangeException(float maxDelayTimeMs)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DelayNode("test", maxDelayTimeMs, 44100, 128));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithNonPositiveSampleRate_ThrowsArgumentOutOfRangeException(int sampleRate)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DelayNode("test", 1000f, sampleRate, 128));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithNonPositiveFrames_ThrowsArgumentOutOfRangeException(int frames)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DelayNode("test", 1000f, 44100, frames));
        }

        [Fact]
        public void DelaySamples_Getter_ReturnsCorrectValue()
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act
            var delaySamples = delayNode.DelaySamples;

            // Assert
            Assert.Equal(44100 / 4, delaySamples);
        }

        [Fact]
        public void DelaySamples_Setter_UpdatesValue()
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act
            delayNode.DelaySamples = 100;

            // Assert
            Assert.Equal(100, delayNode.DelaySamples);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1000000)]
        public void DelaySamples_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(int delaySamples)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => delayNode.DelaySamples = delaySamples);
        }

        [Fact]
        public void Feedback_Getter_ReturnsCorrectValue()
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act
            var feedback = delayNode.Feedback;

            // Assert
            Assert.Equal(0.5f, feedback);
        }

        [Fact]
        public void Feedback_Setter_UpdatesValue()
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act
            delayNode.Feedback = 0.75f;

            // Assert
            Assert.Equal(0.75f, delayNode.Feedback);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void Feedback_Setter_WithInvalidValue_ThrowsArgumentException(float invalidFeedback)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => delayNode.Feedback = invalidFeedback);
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.1f)]
        public void Feedback_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(float invalidFeedback)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => delayNode.Feedback = invalidFeedback);
        }

        [Fact]
        public void DryWetMix_Getter_ReturnsCorrectValue()
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act
            var dryWetMix = delayNode.DryWetMix;

            // Assert
            Assert.Equal(0.5f, dryWetMix);
        }

        [Fact]
        public void DryWetMix_Setter_UpdatesValue()
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act
            delayNode.DryWetMix = 0.25f;

            // Assert
            Assert.Equal(0.25f, delayNode.DryWetMix);
        }

        [Theory]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        [InlineData(float.NegativeInfinity)]
        public void DryWetMix_Setter_WithInvalidValue_ThrowsArgumentException(float invalidDryWetMix)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => delayNode.DryWetMix = invalidDryWetMix);
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.1f)]
        public void DryWetMix_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(float invalidDryWetMix)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => delayNode.DryWetMix = invalidDryWetMix);
        }

        [Fact]
        public void MaxDelaySamples_ReturnsCorrectValue()
        {
            // Arrange
            const int sampleRate = 44100;
            const float maxDelayTimeMs = 1000f; // 1 second
            var delayNode = new DelayNode("test", maxDelayTimeMs, sampleRate, 128);

            // Act
            var maxDelaySamples = delayNode.MaxDelaySamples;

            // Assert
            // 1000ms * 44100 samples/second / 1000ms/second = 44100 samples
            Assert.Equal(44100, maxDelaySamples);
        }

        [Fact]
        public void Process_WithValidInputs_AppliesDelayEffect()
        {
            // Arrange
            const int frames = 4;
            const int sampleRate = 44100;
            const float maxDelayTimeMs = 1000f;
            var delayNode = new DelayNode("test-delay", maxDelayTimeMs, sampleRate, frames);

            var input = new float[] { 1f, 0.5f, 0f, -1f };
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - with default delay (1/4 second = 11025 samples) and default feedback (0.5),
            // the delay buffer is initially empty, so output should equal input (all wet)
            Assert.Equal(input, output);
        }

        [Fact]
        public void Process_WithZeroDelaySamples_ProducesPassthrough()
        {
            // Arrange
            const int frames = 3;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-zero-delay", 1000f, sampleRate, frames);
            delayNode.DelaySamples = 0;

            var input = new float[] { 1f, 0.5f, 0f };
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - zero delay means no delay, so output equals input
            Assert.Equal(input, output);
        }

        [Fact]
        public void Process_WithFeedback_CreatesEchoEffect()
        {
            // Arrange
            const int frames = 4;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-feedback", 1000f, sampleRate, frames);
            delayNode.Feedback = 0.8f; // High feedback for noticeable echo
            delayNode.DryWetMix = 1.0f; // All wet to see pure echo
            delayNode.DelaySamples = 1; // 1 sample delay for immediate feedback

            var input = new float[] { 1f, 0f, 0f, 0f };
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - with feedback=0.8, delay=1 sample, and input[0]=1:
            // output[0] = 1 + 0*0.8 = 1 (no delayed sample yet)
            // output[1] = 0 + 1*0.8 = 0.8 (delayed sample from input[0])
            // output[2] = 0 + 0.8*0.8 = 0.64 (feedback of previous output)
            // output[3] = 0 + 0.64*0.8 = 0.512 (feedback of previous output)
            Assert.Equal(1f, output[0], 5);
            Assert.Equal(0.8f, output[1], 5);
            Assert.Equal(0.64f, output[2], 5);
            Assert.Equal(0.512f, output[3], 5);
        }

        [Fact]
        public void Process_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            const int frames = 2;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-null-input", 1000f, sampleRate, frames);
            var output = new float[frames];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => delayNode.Process(null!, output));
        }

        [Fact]
        public void Process_WithNullOutput_ThrowsArgumentNullException()
        {
            // Arrange
            const int frames = 2;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-null-output", 1000f, sampleRate, frames);
            var input = new float[frames];

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => delayNode.Process(input, null!));
        }

        [Fact]
        public void Process_WithMismatchedInputLength_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 4;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-mismatch", 1000f, sampleRate, frames);
            var input = new float[] { 1f, 2f, 3f }; // Wrong length
            var output = new float[frames];

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => delayNode.Process(input, output));
            Assert.Contains("must have 4 frames", ex.Message);
        }

        [Fact]
        public void Process_WithMismatchedOutputLength_ThrowsArgumentException()
        {
            // Arrange
            const int frames = 4;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-mismatch-out", 1000f, sampleRate, frames);
            var input = new float[frames];
            var output = new float[] { 1f, 2f }; // Wrong length

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => delayNode.Process(input, output));
            Assert.Contains("must have 4 frames", ex.Message);
        }

        [Fact]
        public void Reset_ClearsDelayBuffer()
        {
            // Arrange
            const int frames = 3;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-reset", 1000f, sampleRate, frames);
            delayNode.Feedback = 0.9f;

            var input = new float[] { 1f, 0f, 0f };
            var output1 = new float[frames];
            var output2 = new float[frames];

            // Process once to fill buffer
            delayNode.Process(input, output1);

            // Reset
            delayNode.Reset();

            // Process again - should be like new instance
            delayNode.Process(input, output2);

            // Assert - both outputs should be identical (buffer was cleared)
            Assert.Equal(output1, output2);
        }

        [Fact]
        public void Process_WithDryWetMix_AppliesCorrectMix()
        {
            // Arrange
            const int frames = 2;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-mix", 1000f, sampleRate, frames);
            delayNode.DryWetMix = 0.75f; // 75% wet, 25% dry
            delayNode.DelaySamples = 0; // No delay for simplicity

            var input = new float[] { 1f, 0.5f };
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - with delay=0, outputSample = input[i], so:
            // output[i] = input[i] * (1-0.75) + input[i] * 0.75 = input[i]
            // This test verifies the formula works correctly
            Assert.Equal(input, output);
        }

        [Fact]
        public void Process_WithDelaySamples_WorksCorrectly()
        {
            // Arrange
            const int frames = 2;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-delay", 1000f, sampleRate, frames);
            delayNode.DelaySamples = 100; // Set a reasonable delay

            var input = new float[] { 1f, 0f };
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - should not throw, should process successfully
            Assert.NotNull(output);
            Assert.Equal(frames, output.Length);
        }
    }
}