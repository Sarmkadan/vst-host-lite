using System;
using System.Text.Json;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests
{
    public class NoiseGeneratorNodeJsonExtensionsTests
    {
        private const string TestName = "noise";
        private const int TestFrames = 64;
        private const float TestAmplitude = 0.5f;

        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var node = new NoiseGeneratorNode(TestName, TestFrames);

            // Act
            var json = node.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.IsType<string>(json);
            // The JSON should contain the name and frames
            Assert.Contains(TestName, json);
            Assert.Contains(TestFrames.ToString(), json);
            // Amplitude default is 1.0
            Assert.Contains("1.0", json);
        }

        [Fact]
        public void ToJson_NullNode_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NoiseGeneratorNodeJsonExtensions.ToJson(null!));
        }

        [Fact]
        public void ToJson_IndentedTrue_ReturnsIndentedJson()
        {
            // Arrange
            var node = new NoiseGeneratorNode(TestName, TestFrames);

            // Act
            var json = node.ToJson(indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains(Environment.NewLine, json); // Indented JSON should have newlines
        }

        [Fact]
        public void ToJson_WithCustomAmplitude_IncludesAmplitude()
        {
            // Arrange
            var node = new NoiseGeneratorNode(TestName, TestFrames) { Amplitude = TestAmplitude };

            // Act
            var json = node.ToJson();

            // Assert
            Assert.Contains(TestAmplitude.ToString(), json);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NoiseGeneratorNodeJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyOrWhiteSpaceJson_ReturnsNull()
        {
            // Act
            var result1 = NoiseGeneratorNodeJsonExtensions.FromJson(string.Empty);
            var result2 = NoiseGeneratorNodeJsonExtensions.FromJson("   ");
            var result3 = NoiseGeneratorNodeJsonExtensions.FromJson("\t\n\r");

            // Assert
            Assert.Null(result1);
            Assert.Null(result2);
            Assert.Null(result3);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var node = new NoiseGeneratorNode(TestName, TestFrames) { Amplitude = TestAmplitude };
            var json = node.ToJson();

            // Act
            var result = NoiseGeneratorNodeJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(node.Name, result!.Name);
            Assert.Equal(node.Frames, result.Frames);
            Assert.Equal(node.Amplitude, result.Amplitude);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "{ invalid json";

            // Act & Assert
            Assert.Throws<JsonException>(() => NoiseGeneratorNodeJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => NoiseGeneratorNodeJsonExtensions.TryFromJson(null!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyOrWhiteSpaceJson_ReturnsFalse()
        {
            // Act
            var success1 = NoiseGeneratorNodeJsonExtensions.TryFromJson(string.Empty, out var result1);
            var success2 = NoiseGeneratorNodeJsonExtensions.TryFromJson("   ", out var result2);

            // Assert
            Assert.False(success1);
            Assert.Null(result1);
            Assert.False(success2);
            Assert.Null(result2);
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndDeserializedObject()
        {
            // Arrange
            var node = new NoiseGeneratorNode(TestName, TestFrames) { Amplitude = TestAmplitude };
            var json = node.ToJson();

            // Act
            var success = NoiseGeneratorNodeJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(node.Name, result!.Name);
            Assert.Equal(node.Frames, result.Frames);
            Assert.Equal(node.Amplitude, result.Amplitude);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var invalidJson = "{ invalid json";

            // Act
            var success = NoiseGeneratorNodeJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}