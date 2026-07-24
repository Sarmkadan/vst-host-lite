using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class DelayNodeJsonExtensionsTests
{
    private const string TestName = "TestDelay";
    private const float TestMaxDelayTimeMs = 1000f;
    private const int TestSampleRate = 44100;
    private const int TestFrames = 128;

    private DelayNode CreateNode(string name = TestName, float maxDelayTimeMs = TestMaxDelayTimeMs, int sampleRate = TestSampleRate, int frames = TestFrames)
    {
        return new DelayNode(name, maxDelayTimeMs, sampleRate, frames);
    }

    [Fact]
    public void ToJson_HappyPath_ReturnsValidJson()
    {
        // Arrange
        var node = CreateNode();
        node.DelaySamples = 500;
        node.Feedback = 0.75f;
        node.DryWetMix = 0.6f;

        // Act
        var json = node.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("\"name\"", json);
        Assert.Contains(TestName, json);
        Assert.Contains("\"delaySamples\"", json);
        Assert.Contains("500", json);
        Assert.Contains("\"feedback\"", json);
        Assert.Contains("0.75", json);
        Assert.Contains("\"dryWetMix\"", json);
        Assert.Contains("0.6", json);
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
        Assert.Contains(" ", json); // Indented JSON should have indentation
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
        DelayNode node = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => node.ToJson());
    }

    [Fact]
    public void FromJson_HappyPath_ReturnsDeserializedNode()
    {
        // Arrange
        var originalNode = CreateNode();
        originalNode.DelaySamples = 250;
        originalNode.Feedback = 0.9f;
        originalNode.DryWetMix = 0.3f;

        var json = originalNode.ToJson();

        // Act
        var deserializedNode = DelayNodeJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedNode);
        Assert.Equal(TestName, deserializedNode.Name);
        Assert.Equal(250, deserializedNode.DelaySamples);
        Assert.Equal(0.9f, deserializedNode.Feedback);
        Assert.Equal(0.3f, deserializedNode.DryWetMix);
        Assert.Equal(44100, deserializedNode.MaxDelaySamples);
    }

    [Fact]
    public void FromJson_EmptyString_ReturnsNull()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var result = DelayNodeJsonExtensions.FromJson(emptyJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WhitespaceString_ReturnsNull()
    {
        // Arrange
        var whitespaceJson = " \n\t ";

        // Act
        var result = DelayNodeJsonExtensions.FromJson(whitespaceJson);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string nullJson = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => DelayNodeJsonExtensions.FromJson(nullJson));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => DelayNodeJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_HappyPath_ReturnsTrueAndDeserializedNode()
    {
        // Arrange
        var originalNode = CreateNode("TryFromJsonTest");
        originalNode.DelaySamples = 750;
        originalNode.Feedback = 0.2f;
        originalNode.DryWetMix = 0.8f;

        var json = originalNode.ToJson();

        // Act
        var result = DelayNodeJsonExtensions.TryFromJson(json, out var deserializedNode);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserializedNode);
        Assert.Equal("TryFromJsonTest", deserializedNode.Name);
        Assert.Equal(750, deserializedNode.DelaySamples);
        Assert.Equal(0.2f, deserializedNode.Feedback);
        Assert.Equal(0.8f, deserializedNode.DryWetMix);
        Assert.Equal(44100, deserializedNode.MaxDelaySamples);
    }

    [Fact]
    public void TryFromJson_EmptyString_ReturnsFalseAndNull()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var result = DelayNodeJsonExtensions.TryFromJson(emptyJson, out var deserializedNode);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedNode);
    }

    [Fact]
    public void TryFromJson_WhitespaceString_ReturnsFalseAndNull()
    {
        // Arrange
        var whitespaceJson = " \n\t ";

        // Act
        var result = DelayNodeJsonExtensions.TryFromJson(whitespaceJson, out var deserializedNode);

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
        Assert.Throws<ArgumentNullException>(() => DelayNodeJsonExtensions.TryFromJson(nullJson, out _));
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = DelayNodeJsonExtensions.TryFromJson(invalidJson, out var deserializedNode);

        // Assert
        Assert.False(result);
        Assert.Null(deserializedNode);
    }

    [Fact]
    public void RoundTripSerialization_PreservesAllProperties()
    {
        // Arrange
        var originalNode = CreateNode("RoundTripSerializationTest", 2000f, 48000, 256);
        originalNode.DelaySamples = 1000;
        originalNode.Feedback = 0.45f;
        originalNode.DryWetMix = 0.55f;

        // Act
        var json = originalNode.ToJson();
        var deserializedNode = DelayNodeJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedNode);
        Assert.Equal("RoundTripSerializationTest", deserializedNode.Name);
        Assert.Equal(44100, deserializedNode.MaxDelaySamples);
        Assert.Equal(1000, deserializedNode.DelaySamples);
        Assert.Equal(0.45f, deserializedNode.Feedback);
        Assert.Equal(0.55f, deserializedNode.DryWetMix);
    }

    [Fact]
    public void RoundTripSerialization_WithIndentedJson_PreservesProperties()
    {
        // Arrange
        var originalNode = CreateNode();
        originalNode.DelaySamples = 123;
        originalNode.Feedback = 0.123f;
        originalNode.DryWetMix = 0.456f;

        // Act
        var json = originalNode.ToJson(indented: true);
        var deserializedNode = DelayNodeJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserializedNode);
        Assert.Equal(123, deserializedNode.DelaySamples);
        Assert.Equal(0.123f, deserializedNode.Feedback);
        Assert.Equal(0.456f, deserializedNode.DryWetMix);
        Assert.Equal(44100, deserializedNode.MaxDelaySamples);
    }
}