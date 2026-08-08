using System;
using System.Collections.Generic;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests
{
    public class SineGeneratorNodeTests
    {
        [Fact]
        public void Constructor_ValidParameters_SetsProperties()
        {
            // Arrange
            const string name = "test";
            const float sampleRate = 44100f;
            const int frames = 512;

            // Act
            var node = new SineGeneratorNode(name, sampleRate, frames);

            // Assert
            Assert.Equal(name, node.Name);
            Assert.Equal(sampleRate, node.SampleRate);
            Assert.Equal(frames, node.Frames);
            Assert.InRange(node.Frequency, 0f, float.MaxValue); // Default frequency set
            Assert.InRange(node.Amplitude, 0f, 1f); // Default amplitude set
        }

        [Fact]
        public void Constructor_NullName_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new SineGeneratorNode(null, 44100f, 512));
        }

        [Fact]
        public void Constructor_InvalidSampleRate_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new SineGeneratorNode("test", 0f, 512));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SineGeneratorNode("test", -1f, 512));
        }

        [Fact]
        public void Constructor_InvalidFrames_ThrowsArgumentOutOfRangeException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new SineGeneratorNode("test", 44100f, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SineGeneratorNode("test", 44100f, -1));
        }

        [Fact]
        public void Generate_NullBuffer_ThrowsArgumentNullException()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => node.Generate(null));
        }

        [Fact]
        public void Generate_WrongLengthBuffer_ThrowsArgumentException()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512);
            var buffer = new float[256]; // Wrong length

            // Act & Assert
            Assert.Throws<ArgumentException>(() => node.Generate(buffer));
        }

        [Fact]
        public void Generate_ZeroAmplitude_ProducesZeroBuffer()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512) { Amplitude = 0f };
            var buffer = new float[512];

            // Act
            node.Generate(buffer);

            // Assert
            Assert.All(buffer, sample => Assert.Equal(0f, sample));
        }

        [Fact]
        public void Generate_NonZeroAmplitude_ProducesNonZeroValues()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512) { Amplitude = 0.5f, Frequency = 440f };
            var buffer = new float[512];

            // Act
            node.Generate(buffer);

            // Assert
            // At least one sample should be non-zero (since sine wave varies)
            Assert.Contains(buffer, sample => Math.Abs(sample) > 0.001f);
            // All samples should be within amplitude range
            Assert.All(buffer, sample => Assert.InRange(Math.Abs(sample), 0f, 0.5f + 0.0001f));
        }

        [Fact]
        public void Reset_SetsPhaseToZero()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512) { Frequency = 440f };
            var buffer = new float[512];
            // Generate some samples to advance phase
            node.Generate(buffer);
            float phaseAfterGenerate = GetPrivatePhase(node); // We'll need to access private field via reflection or add a test-only accessor? Since we can't modify production code, we'll test indirectly: after Reset, generating again should start from zero phase.

            // Act
            node.Reset();

            // Assert: After reset, generating should produce same first sample as from fresh node
            var freshNode = new SineGeneratorNode("test", 44100f, 512) { Frequency = 440f };
            var freshBuffer = new float[512];
            freshNode.Generate(freshBuffer);

            var resetBuffer = new float[512];
            node.Generate(resetBuffer);

            Assert.Equal(freshBuffer[0], resetBuffer[0], 5); // Allow small tolerance
        }

        [Fact]
        public void Frequency_Setter_UpdatesPhaseIncrement()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512);
            float originalIncrement = GetPrivatePhaseIncrement(node);

            // Act
            node.Frequency = 880f; // Double frequency

            // Assert
            float newIncrement = GetPrivatePhaseIncrement(node);
            Assert.Equal(2f * originalIncrement, newIncrement, 5);
        }

        [Fact]
        public void Amplitude_Setter_ValidatesAndClamps()
        {
            // Arrange
            var node = new SineGeneratorNode("test", 44100f, 512);

            // Act & Assert: Valid range
            node.Amplitude = 0f;
            Assert.Equal(0f, node.Amplitude);
            node.Amplitude = 1f;
            Assert.Equal(1f, node.Amplitude);

            // Act & Assert: Out of low throws
            Assert.Throws<ArgumentOutOfRangeException>(() => node.Amplitude = -0.1f);
            // Act & Assert: Out of high throws
            Assert.Throws<ArgumentOutOfRangeException>(() => node.Amplitude = 1.1f);

            // Act & Assert: NaN throws
            Assert.Throws<ArgumentException>(() => node.Amplitude = float.NaN);
            // Act & Assert: Infinity throws
            Assert.Throws<ArgumentException>(() => node.Amplitude = float.PositiveInfinity);
        }

        // Helper to access private _phaseIncrement field via reflection
        private static float GetPrivatePhaseIncrement(SineGeneratorNode node)
        {
            var field = typeof(SineGeneratorNode).GetField("_phaseIncrement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)field.GetValue(node);
        }

        // Helper to access private _phase field via reflection
        private static float GetPrivatePhase(SineGeneratorNode node)
        {
            var field = typeof(SineGeneratorNode).GetField("_phase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (float)field.GetValue(node);
        }
    }
}
