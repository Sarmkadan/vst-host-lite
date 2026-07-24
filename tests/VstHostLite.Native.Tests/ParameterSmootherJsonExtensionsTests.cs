using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests
{
    public class ParameterSmootherJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidParameterSmoother_ReturnsValidJson()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0.5f);
            smoother.Target = 0.8f;

            // Act
            var json = smoother.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("current", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("target", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("sampleRate", json, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("timeConstantSeconds", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsPrettyPrintedJson()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0.5f);

            // Act
            var json = smoother.ToJson(indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\n", json); // Indented JSON should have newlines
        }

        [Fact]
        public void ToJson_WithIndentedFalse_ReturnsCompactJson()
        {
            // Arrange
            var smoother = new ParameterSmoother(44100f, 0.1f, 0.5f);

            // Act
            var json = smoother.ToJson(indented: false);

            // Assert
            Assert.NotNull(json);
            Assert.DoesNotContain("\n", json); // Compact JSON should not have newlines
        }

        [Fact]
        public void ToJson_WithNullParameter_ThrowsArgumentNullException()
        {
            // Arrange
            ParameterSmoother? smoother = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => smoother!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsParameterSmoother()
        {
            // Arrange
            var original = new ParameterSmoother(48000f, 0.25f, 0.25f);
            original.Target = 0.75f;
            var json = original.ToJson();

            // Act
            var deserialized = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Current, deserialized.Current, 5);
            Assert.Equal(original.Target, deserialized.Target, 5);
        }

        [Fact]
        public void FromJson_WithNullJson_ReturnsNull()
        {
            // Arrange
            string? json = null;

            // Act
            var result = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_WithEmptyString_ReturnsNull()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_WithWhitespaceString_ReturnsNull()
        {
            // Arrange
            var json = "   \t\n  ";

            // Act
            var result = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "{ invalid json }";

            // Act & Assert
            Assert.Throws<System.Text.Json.JsonException>(() => ParameterSmootherJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndDeserializedObject()
        {
            // Arrange
            var original = new ParameterSmoother(41000f, 0.5f, 0.33f);
            original.Target = 0.66f;
            var json = original.ToJson();

            // Act
            var result = ParameterSmootherJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(result);
            Assert.NotNull(deserialized);
            Assert.Equal(original.Current, deserialized.Current, 5);
            Assert.Equal(original.Target, deserialized.Target, 5);
        }

        [Fact]
        public void TryFromJson_WithNullJson_ReturnsFalseAndNull()
        {
            // Arrange
            string? json = null;

            // Act
            var result = ParameterSmootherJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.False(result);
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_WithEmptyString_ReturnsFalseAndNull()
        {
            // Arrange
            var json = string.Empty;

            // Act
            var result = ParameterSmootherJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.False(result);
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_WithWhitespaceString_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "  \r\n\t  ";

            // Act
            var result = ParameterSmootherJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.False(result);
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "not a valid json";

            // Act
            var result = ParameterSmootherJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.False(result);
            Assert.Null(deserialized);
        }

        [Fact]
        public void Roundtrip_WithAllProperties_RoundtripsCorrectly()
        {
            // Arrange
            var original = new ParameterSmoother(48000f, 0.125f, 0.125f);
            original.Target = 0.875f;

            // Act
            var json = original.ToJson();
            var deserialized = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Current, deserialized.Current, 5);
            Assert.Equal(original.Target, deserialized.Target, 5);
        }

        [Fact]
        public void Roundtrip_WithZeroValues_RoundtripsCorrectly()
        {
            // Arrange
            var original = new ParameterSmoother(1f, 1f, 0f);
            original.Target = 0f;

            // Act
            var json = original.ToJson();
            var deserialized = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(0f, deserialized.Current, 5);
            Assert.Equal(0f, deserialized.Target, 5);
        }

        [Fact]
        public void Roundtrip_WithHighPrecisionValues_RoundtripsCorrectly()
        {
            // Arrange
            var original = new ParameterSmoother(96000f, 0.001f, 0.123456789f);
            original.Target = 0.987654321f;

            // Act
            var json = original.ToJson();
            var deserialized = ParameterSmootherJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Current, deserialized.Current, 8);
            Assert.Equal(original.Target, deserialized.Target, 8);
        }
    }
}