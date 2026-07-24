using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests
{
    public class ParameterSmootherTests
    {
        [Fact]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            // Arrange
            var sampleRate = 44100f;
            var timeConstant = 0.1f;
            var initial = 0.5f;

            // Act
            var smoother = new ParameterSmoother(sampleRate, timeConstant, initial);

            // Assert
            Assert.NotNull(smoother);
            Assert.Equal(initial, smoother.Current, 5);
            Assert.Equal(initial, smoother.Target, 5);
        }

        [Fact]
        public void Constructor_InvalidSampleRate_Throws()
        {
            // Arrange
            var timeConstant = 0.1f;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(0f, timeConstant));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(-1f, timeConstant));
        }

        [Fact]
        public void Constructor_InvalidTimeConstant_Throws()
        {
            // Arrange
            var sampleRate = 44100f;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(sampleRate, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(sampleRate, -0.5f));
        }

        [Fact]
        public void SnapToTarget_UpdatesCurrent()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0.2f);
            smoother.Target = 0.8f;

            // Act
            smoother.SnapToTarget();

            // Assert
            Assert.Equal(0.8f, smoother.Current, 5);
        }

        [Fact]
        public void NextValue_UpdatesCurrent()
        {
            // Arrange
            var sampleRate = 1000f;
            var timeConstant = 1.0f;
            var smoother = new ParameterSmoother(sampleRate, timeConstant, 0f);
            smoother.Target = 1f;

            // Compute expected alpha
            var alpha = 1f - (float)Math.Exp(-1.0 / (sampleRate * timeConstant));

            // Act
            var result = smoother.NextValue();

            // Assert
            Assert.Equal(alpha, result, 5);
            Assert.Equal(alpha, smoother.Current, 5);
        }

        [Fact]
        public void Process_WritesSmoothedValues()
        {
            // Arrange
            var sampleRate = 1000f;
            var timeConstant = 1.0f;
            var smoother = new ParameterSmoother(sampleRate, timeConstant, 0f);
            smoother.Target = 1f;
            var length = 5;
            var destination = new float[length];

            // Compute expected values
            var expected = new float[length];
            var alpha = 1f - (float)Math.Exp(-1.0 / (sampleRate * timeConstant));
            var current = 0f;
            for (int i = 0; i < length; i++)
            {
                current += (1f - current) * alpha;
                expected[i] = current;
            }

            // Act
            smoother.Process(destination);

            // Assert
            Assert.Equal(expected, destination);
        }

        [Fact]
        public void Process_NullDestination_Throws()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0f);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => smoother.Process(null));
        }

        [Fact]
        public void Process_EmptyArray_DoesNothing()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0f);
            var empty = new float[0];

            // Act & Assert
            var ex = Record.Exception(() => smoother.Process(empty));
            Assert.Null(ex);
        }

        [Fact]
        public void TargetProperty_SetAndGet()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0f);

            // Act
            smoother.Target = 0.75f;

            // Assert
            Assert.Equal(0.75f, smoother.Target, 5);
        }
    }
}
