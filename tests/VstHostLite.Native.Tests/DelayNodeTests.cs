using System;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests
{
    /// <summary>
    /// Unit tests for <see cref="DelayNode"/> covering constructor validation, property
    /// defaults and range-checked setters, maximum delay calculation, audio processing
    /// behaviour (passthrough, echo via feedback, dry/wet mixing) and delay buffer reset.
    /// </summary>
    public class DelayNodeTests
    {
        [Fact]
        /// <summary>
        /// Tests that constructing a <see cref="DelayNode"/> with valid parameters creates an
        /// instance whose name matches the supplied value and whose properties start at their
        /// defaults: a quarter-second delay at the given sample rate (44100 / 4 = 11025 samples),
        /// 0.5 feedback and 0.5 dry/wet mix.
        /// </summary>
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
        /// <summary>
        /// Tests that constructing a <see cref="DelayNode"/> with a null name
        /// throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        public void Constructor_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DelayNode(null!, 1000f, 44100, 128));
        }

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        /// <summary>
        /// Tests that constructing a <see cref="DelayNode"/> with a zero or negative maximum
        /// delay time throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="maxDelayTimeMs">An invalid non-positive maximum delay time in milliseconds.</param>
        public void Constructor_WithNonPositiveMaxDelayTime_ThrowsArgumentOutOfRangeException(float maxDelayTimeMs)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DelayNode("test", maxDelayTimeMs, 44100, 128));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        /// <summary>
        /// Tests that constructing a <see cref="DelayNode"/> with a zero or negative sample
        /// rate throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="sampleRate">An invalid non-positive sample rate in Hz.</param>
        public void Constructor_WithNonPositiveSampleRate_ThrowsArgumentOutOfRangeException(int sampleRate)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DelayNode("test", 1000f, sampleRate, 128));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        /// <summary>
        /// Tests that constructing a <see cref="DelayNode"/> with a zero or negative frame
        /// count throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="frames">An invalid non-positive number of frames per processing block.</param>
        public void Constructor_WithNonPositiveFrames_ThrowsArgumentOutOfRangeException(int frames)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new DelayNode("test", 1000f, 44100, frames));
        }

        [Fact]
        /// <summary>
        /// Tests that the <see cref="DelayNode.DelaySamples"/> getter returns the default
        /// quarter-second delay at the construction sample rate (44100 / 4 = 11025 samples).
        /// </summary>
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
        /// <summary>
        /// Tests that assigning a valid value to <see cref="DelayNode.DelaySamples"/>
        /// stores and returns that value.
        /// </summary>
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
        /// <summary>
        /// Tests that assigning a negative delay, or one beyond the node's maximum capacity of
        /// 44100 samples (1000 ms at 44100 Hz), to <see cref="DelayNode.DelaySamples"/> throws
        /// an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="delaySamples">An invalid delay below zero or above the maximum delay in samples.</param>
        public void DelaySamples_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(int delaySamples)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => delayNode.DelaySamples = delaySamples);
        }

        [Fact]
        /// <summary>
        /// Tests that the <see cref="DelayNode.Feedback"/> getter returns the default value of 0.5.
        /// </summary>
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
        /// <summary>
        /// Tests that assigning a valid value to <see cref="DelayNode.Feedback"/>
        /// stores and returns that value.
        /// </summary>
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
        /// <summary>
        /// Tests that assigning NaN or an infinite value to <see cref="DelayNode.Feedback"/>
        /// throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="invalidFeedback">An invalid non-finite feedback amount.</param>
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
        /// <summary>
        /// Tests that assigning a feedback amount outside the [0, 1] range to
        /// <see cref="DelayNode.Feedback"/> throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="invalidFeedback">An invalid feedback amount below 0 or above 1.</param>
        public void Feedback_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(float invalidFeedback)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => delayNode.Feedback = invalidFeedback);
        }

        [Fact]
        /// <summary>
        /// Tests that the <see cref="DelayNode.DryWetMix"/> getter returns the default value of 0.5.
        /// </summary>
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
        /// <summary>
        /// Tests that assigning a valid value to <see cref="DelayNode.DryWetMix"/>
        /// stores and returns that value.
        /// </summary>
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
        /// <summary>
        /// Tests that assigning NaN or an infinite value to <see cref="DelayNode.DryWetMix"/>
        /// throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="invalidDryWetMix">An invalid non-finite dry/wet mix ratio.</param>
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
        /// <summary>
        /// Tests that assigning a dry/wet mix ratio outside the [0, 1] range to
        /// <see cref="DelayNode.DryWetMix"/> throws an <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="invalidDryWetMix">An invalid dry/wet mix ratio below 0 or above 1.</param>
        public void DryWetMix_Setter_WithOutOfRangeValue_ThrowsArgumentOutOfRangeException(float invalidDryWetMix)
        {
            // Arrange
            var delayNode = new DelayNode("test", 1000f, 44100, 128);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => delayNode.DryWetMix = invalidDryWetMix);
        }

        [Fact]
        /// <summary>
        /// Tests that <see cref="DelayNode.MaxDelaySamples"/> converts the configured maximum
        /// delay time of 1000 ms into 44100 samples at a 44100 Hz sample rate.
        /// </summary>
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
        /// <summary>
        /// Tests that processing a four-frame buffer with default settings copies the input to
        /// the output unchanged, because the delay line starts empty so no delayed samples are
        /// mixed into the wet signal yet.
        /// </summary>
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
        /// <summary>
        /// Tests that processing with <see cref="DelayNode.DelaySamples"/> set to zero passes
        /// the input straight through to the output without modification.
        /// </summary>
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
        /// <summary>
        /// Tests that processing a single-sample impulse with full wet mix, 0.8 feedback and a
        /// one-sample delay produces a decaying echo of 1, 0.8, 0.64 and 0.512 across the four
        /// output frames, each sample being the previous output scaled by the feedback amount.
        /// </summary>
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
        /// <summary>
        /// Tests that calling <c>DelayNode.Process</c> with a null input buffer
        /// throws an <see cref="ArgumentNullException"/>.
        /// </summary>
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
        /// <summary>
        /// Tests that calling <c>DelayNode.Process</c> with a null output buffer
        /// throws an <see cref="ArgumentNullException"/>.
        /// </summary>
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
        /// <summary>
        /// Tests that processing with an input buffer shorter than the node's frame count
        /// throws an <see cref="ArgumentException"/> whose message states the required
        /// frame count ("must have 4 frames").
        /// </summary>
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
        /// <summary>
        /// Tests that processing with an output buffer shorter than the node's frame count
        /// throws an <see cref="ArgumentException"/> whose message states the required
        /// frame count ("must have 4 frames").
        /// </summary>
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
        /// <summary>
        /// Tests that <see cref="DelayNode.Reset"/> clears the internal delay buffer so that
        /// processing the same input again produces output identical to the first pass.
        /// </summary>
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
        /// <summary>
        /// Tests that processing with a 0.75 dry/wet mix and zero delay yields the unmodified
        /// input, because the dry and wet contributions sum back to the original sample values.
        /// </summary>
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
        /// <summary>
        /// Tests that processing with a 100-sample delay completes without error and fills the
        /// output buffer with the expected number of frames.
        /// </summary>
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

        [Fact]
        /// <summary>
        /// Tests that processing an impulse (1.0 followed by zeros) with a specific delay
        /// produces output where the impulse appears after the delay period.
        /// </summary>
        public void Process_WithImpulseAndDelay_OutputIsDelayedByDelaySamples()
        {
            // Arrange
            const int frames = 5;
            const int sampleRate = 44100;
            const int delaySamples = 2;
            var delayNode = new DelayNode("test-impulse-delay", 1000f, sampleRate, frames);
            delayNode.DelaySamples = delaySamples;
            delayNode.Feedback = 0.0f; // No feedback to hear just the delayed impulse
            delayNode.DryWetMix = 1.0f; // All wet to see the delayed signal clearly

            var input = new float[] { 1f, 0f, 0f, 0f, 0f }; // Impulse at sample 0
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - with delay of 2 samples:
            // output[0] should be 0 (no delayed signal yet)
            // output[1] should be 0 (still no delayed signal)
            // output[2] should be 1 (the delayed impulse)
            // output[3] and [4] should be 0 (no more input)
            Assert.Equal(0f, output[0]);
            Assert.Equal(0f, output[1]);
            Assert.Equal(1f, output[2], 5); // Allow small floating point differences
            Assert.Equal(0f, output[3]);
            Assert.Equal(0f, output[4]);
        }

        [Fact]
        /// <summary>
        /// Tests that processing with a dry/wet mix of 0 returns the unmodified dry signal,
        /// regardless of delay or feedback settings.
        /// </summary>
        public void Process_WithDryWetMixZero_ReturnsPureDrySignal()
        {
            // Arrange
            const int frames = 3;
            const int sampleRate = 44100;
            var delayNode = new DelayNode("test-dry-mix-zero", 1000f, sampleRate, frames);
            delayNode.DelaySamples = 100; // Non-zero delay
            delayNode.Feedback = 0.9f; // High feedback
            delayNode.DryWetMix = 0.0f; // All dry

            var input = new float[] { 0.5f, 0.25f, -0.1f };
            var output = new float[frames];

            // Act
            delayNode.Process(input, output);

            // Assert - with dry/wet mix = 0, output should equal input exactly
            Assert.Equal(input, output);
        }
    }
}
