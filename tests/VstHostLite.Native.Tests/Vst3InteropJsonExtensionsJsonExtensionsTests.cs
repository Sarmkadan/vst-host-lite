using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class Vst3InteropJsonExtensionsJsonExtensionsTests
{
    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        // Arrange
        const bool indented = false;

        // Act
        var json = Vst3InteropJsonExtensionsJsonExtensions.ToJson(indented);

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("\"type\"", json);
        Assert.Contains("Vst3InteropJsonExtensions", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        const bool indented = true;

        // Act
        var json = Vst3InteropJsonExtensionsJsonExtensions.ToJson(indented);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\n", json); // Indented JSON should have newlines
        Assert.Contains(" ", json); // Indented JSON should have indentation
        Assert.Contains("\"type\"", json);
        Assert.Contains("Vst3InteropJsonExtensions", json);
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        const bool indented = false;

        // Act
        var json = Vst3InteropJsonExtensionsJsonExtensions.ToJson(indented);

        // Assert
        Assert.NotNull(json);
        Assert.DoesNotContain("\n", json); // Compact JSON should not have newlines
        Assert.Contains("\"type\"", json);
        Assert.Contains("Vst3InteropJsonExtensions", json);
    }

    [Fact]
    public void FromJson_HappyPath_WithValidJson_ReturnsMarkerObject()
    {
        // Arrange
        var validJson = Vst3InteropJsonExtensionsJsonExtensions.ToJson(false);

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.FromJson(validJson);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_HappyPath_WithTypeMarker_ReturnsMarkerObject()
    {
        // Arrange
        var json = "{\"type\":\"Vst3InteropJsonExtensions\"}";

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Vst3InteropJsonExtensionsJsonExtensions.FromJson(emptyJson));
    }

    [Fact]
    public void FromJson_WhitespaceString_ThrowsJsonException()
    {
        // Arrange
        var whitespaceJson = " \n\t ";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => Vst3InteropJsonExtensionsJsonExtensions.FromJson(whitespaceJson));
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Vst3InteropJsonExtensionsJsonExtensions.FromJson(nullJson));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => Vst3InteropJsonExtensionsJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void FromJson_WrongType_ReturnsNull()
    {
        // Arrange
        var wrongTypeJson = "{\"type\":\"WrongType\"}";

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.FromJson(wrongTypeJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_HappyPath_WithValidJson_ReturnsTrueAndMarkerObject()
    {
        // Arrange
        var validJson = Vst3InteropJsonExtensionsJsonExtensions.ToJson(false);

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(validJson, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
    }

    [Fact]
    public void TryFromJson_HappyPath_WithTypeMarker_ReturnsTrueAndMarkerObject()
    {
        // Arrange
        var json = "{\"type\":\"Vst3InteropJsonExtensions\"}";

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(json, out var value);

        // Assert
        Assert.True(result);
        Assert.NotNull(value);
    }

    [Fact]
    public void TryFromJson_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(emptyJson, out _));
    }

    [Fact]
    public void TryFromJson_WhitespaceString_ReturnsFalseAndNull()
    {
        // Arrange
        var whitespaceJson = " \n\t ";

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(whitespaceJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(nullJson, out _));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(invalidJson, out var value);

        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_WrongType_ReturnsTrueAndNull()
    {
        // Arrange
        var wrongTypeJson = "{\"type\":\"WrongType\"}";

        // Act
        var result = Vst3InteropJsonExtensionsJsonExtensions.TryFromJson(wrongTypeJson, out var value);

        // Assert
        Assert.True(result);
        Assert.Null(value);
    }
}