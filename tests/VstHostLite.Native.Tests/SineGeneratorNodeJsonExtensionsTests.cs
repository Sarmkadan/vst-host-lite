using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class SineGeneratorNodeJsonExtensionsTests
{
    private const string TestName = "TestSineGenerator";
    private const float TestSampleRate = 48000f;
    private const int TestFrames = 512;

    private SineGeneratorNode CreateNode(string name = TestName, float sampleRate = TestSampleRate, int frames = TestFrames)
    {
        return new SineGeneratorNode(name, sampleRate, frames);
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        // Arrange
        var node = CreateNode();
        node.Frequency = 440f;
        node.Amplitude = 0.75f;

        // Act
        var json = node.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("\"name\"", json);
        Assert.Contains(TestName, json);
        Assert.Contains("\"frequency\"", json);
        Assert.Contains("440", json);
        Assert.Contains("\"amplitude\"", json);
        Assert.Contains("0.75", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var node = CreateNode();

        // Act
        var json = node.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\n", json); // Indented JSON should have newlines
        Assert.Contains("  ", json); // Indented JSON should have indentation
    }

    [Fact]
    public void ToJson_WithIndentedFalse_ReturnsCompactJson()
    {
        // Arrange
        var node = CreateNode();

        // Act
        var json = node.ToJson(indented: false);

        // Assert
        Assert.NotNull(json);
        Assert.DoesNotContain("\n", json); // Compact JSON should not have newlines
    }

    [Fact]
    public void ToJson_NullNode_ThrowsArgumentNullException()
    {
        // Arrange
        SineGeneratorNode node = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => node.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedNode()
    {
        // Arrange
        var originalNode = CreateNode();
        originalNode.Frequency = 880f;
        originalNode.Amplitude = 0.5f;

        var json = originalNode.ToJson();

        // Act
        var deserializedNode = SineGeneratorNodeJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedNode);
        Assert.Equal(TestName, deserializedNode.Name);
        Assert.Equal(880f, deserializedNode.Frequency);
        Assert.Equal(0.5f, deserializedNode.Amplitude);
        Assert.Equal(TestSampleRate, deserializedNode.SampleRate);
        Assert.Equal(TestFrames, deserializedNode.Frames);
    }

    [Fact]
    public void FromJson_EmptyString_ReturnsNull()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var result = SineGeneratorNodeJsonExtensions.FromJson(emptyJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WhitespaceString_ReturnsNull()
    {
        // Arrange
        var whitespaceJson = "   \n\t  ";

        // Act
        var result = SineGeneratorNodeJsonExtensions.FromJson(whitespaceJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SineGeneratorNodeJsonExtensions.FromJson(nullJson));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => SineGeneratorNodeJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedNode()
    {
        // Arrange
        var originalNode = CreateNode();
        originalNode.Frequency = 1000f;
        originalNode.Amplitude = 0.25f;

        var json = originalNode.ToJson();

        // Act
        var result = SineGeneratorNodeJsonExtensions.TryFromJson(json, out var deserializedNode);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserializedNode);
        Assert.Equal(TestName, deserializedNode.Name);
        Assert.Equal(1000f, deserializedNode.Frequency);
        Assert.Equal(0.25f, deserializedNode.Amplitude);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsFalseAndNull()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var result = SineGeneratorNodeJsonExtensions.TryFromJson(emptyJson, out var deserializedNode);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedNode);
    }

    [Fact]
    public void TryFromJson_WhitespaceString_ReturnsFalseAndNull()
    {
        // Arrange
        var whitespaceJson = "   \n\t  ";

        // Act
        var result = SineGeneratorNodeJsonExtensions.TryFromJson(whitespaceJson, out var deserializedNode);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedNode);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SineGeneratorNodeJsonExtensions.TryFromJson(nullJson, out _));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = SineGeneratorNodeJsonExtensions.TryFromJson(invalidJson, out var deserializedNode);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedNode);
    }

    [Fact]
    public void RoundTripSerialization_PreservesAllProperties()
    {
        // Arrange
        var originalNode = CreateNode("RoundTripTest", 44100f, 256);
        originalNode.Frequency = 1234.56f;
        originalNode.Amplitude = 0.8f;

        // Act
        var json = originalNode.ToJson();
        var deserializedNode = SineGeneratorNodeJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedNode);
        Assert.Equal("RoundTripTest", deserializedNode.Name);
        Assert.Equal(44100f, deserializedNode.SampleRate);
        Assert.Equal(256, deserializedNode.Frames);
    }
}