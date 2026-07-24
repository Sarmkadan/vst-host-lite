using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests
{
    public class ClipDetectorTests
    {
        // ---------- float[] overload ----------

        [Fact]
        public void Detect_FloatArray_HappyPath_ReturnsCorrectResult()
        {
            // Arrange
            float[] buffer = { 0.5f, 1.2f, -1.5f, 0.9f }; // two samples exceed default threshold 1.0f

            // Act
            ClipDetectionResult result = ClipDetector.Detect(buffer);

            // Assert
            Assert.Equal(2, result.ClippedSampleCount);
            Assert.Equal(1, result.FirstClipIndex); // first clipped sample is at index 1 (value 1.2f)
            Assert.Equal(1.5f, result.MaxAbsoluteValue);
        }

        [Fact]
        public void Detect_FloatArray_NoClipping_ReturnsZeroCounts()
        {
            // Arrange
            float[] buffer = { -0.8f, 0.3f, 0.99f };

            // Act
            ClipDetectionResult result = ClipDetector.Detect(buffer);

            // Assert
            Assert.Equal(0, result.ClippedSampleCount);
            Assert.Equal(-1, result.FirstClipIndex);
            Assert.Equal(0.99f, result.MaxAbsoluteValue);
        }

        [Fact]
        public void Detect_FloatArray_EmptyArray_ReturnsZeroCounts()
        {
            // Arrange
            float[] buffer = Array.Empty<float>();

            // Act
            ClipDetectionResult result = ClipDetector.Detect(buffer);

            // Assert
            Assert.Equal(0, result.ClippedSampleCount);
            Assert.Equal(-1, result.FirstClipIndex);
            Assert.Equal(0.0f, result.MaxAbsoluteValue);
        }

        [Fact]
        public void Detect_FloatArray_Null_ThrowsArgumentNullException()
        {
            // Arrange
            float[]? buffer = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ClipDetector.Detect(buffer!));
        }

        // ---------- AudioBuffer overload ----------

        [Fact]
        public void Detect_AudioBuffer_HappyPath_ReturnsCorrectResult()
        {
            // Arrange: 2 channels, 2 frames
            var audioBuffer = new AudioBuffer(2, 2);
            // Fill samples:
            // Channel 0, Frame 0 = 0.5 (no clip)
            // Channel 0, Frame 1 = 1.2 (clip)
            // Channel 1, Frame 0 = -1.5 (clip, first in linear order)
            // Channel 1, Frame 1 = 0.9 (no clip)
            audioBuffer[0, 0] = 0.5f;
            audioBuffer[0, 1] = 1.2f;
            audioBuffer[1, 0] = -1.5f;
            audioBuffer[1, 1] = 0.9f;

            // Act
            ClipDetectionResult result = ClipDetector.Detect(audioBuffer);

            // Assert
            // Two clipped samples (1.2 and -1.5)
            Assert.Equal(2, result.ClippedSampleCount);
            // First clipped sample is at frame 0, channel 1 => index = 0 * 2 + 1 = 1
            Assert.Equal(1, result.FirstClipIndex);
            // Max absolute value is 1.5
            Assert.Equal(1.5f, result.MaxAbsoluteValue);
        }

        [Fact]
        public void Detect_AudioBuffer_NoClipping_ReturnsZeroCounts()
        {
            // Arrange: 1 channel, 3 frames
            var audioBuffer = new AudioBuffer(1, 3);
            audioBuffer[0, 0] = 0.2f;
            audioBuffer[0, 1] = -0.7f;
            audioBuffer[0, 2] = 0.99f; // below default threshold 1.0f

            // Act
            ClipDetectionResult result = ClipDetector.Detect(audioBuffer);

            // Assert
            Assert.Equal(0, result.ClippedSampleCount);
            Assert.Equal(-1, result.FirstClipIndex);
            Assert.Equal(0.99f, result.MaxAbsoluteValue);
        }

        [Fact]
        public void Detect_AudioBuffer_Null_ThrowsArgumentNullException()
        {
            // Arrange
            AudioBuffer? buffer = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ClipDetector.Detect(buffer!));
        }

        // ---------- ClipDetectionResult ----------

        [Fact]
        public void ClipDetectionResult_ToString_ContainsAllValues()
        {
            // Arrange
            var result = new ClipDetectionResult
            {
                ClippedSampleCount = 3,
                MaxAbsoluteValue = 2.3456f,
                FirstClipIndex = 5
            };

            // Act
            string text = result.ToString();

            // Assert
            Assert.Contains("Clipped: 3", text);
            Assert.Contains("Max: 2.345600", text); // default ToString uses F6 format
            Assert.Contains("First: 5", text);
        }
    }
}
