using System;
using System.Text.Json;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class AudioBufferJsonExtensionsTests
{
    // Helper to create a simple AudioBuffer instance.
    // The actual AudioBuffer type may have many properties; we only need a
    // default instance that can be serialized/deserialized.
    private static AudioBuffer CreateSampleBuffer()
    {
        // Assuming AudioBuffer has a public parameterless constructor.
        // If it has required properties, they can be set here.
        return new AudioBuffer();
    }

    [Fact]
    public void ToJson_NullAudioBuffer_ThrowsArgumentNullException()
    {
        AudioBuffer? buffer = null;
        Assert.Throws<ArgumentNullException>(() => buffer!.ToJson());
    }

    [Fact]
    public void ToJson_DefaultBuffer_ReturnsJsonString()
    {
        var buffer = CreateSampleBuffer();

        var json = buffer.ToJson();

        // The result should be a non‑empty JSON object representation.
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
    }

    [Fact]
    public void ToJson_IndentedOption_ProducesIndentedJson()
    {
        var buffer = CreateSampleBuffer();

        var json = buffer.ToJson(indented: true);

        // Indented JSON contains line breaks; we check for at least one newline.
        Assert.Contains(Environment.NewLine, json);
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsAudioBuffer()
    {
        var buffer = CreateSampleBuffer();
        var json = buffer.ToJson();

        var result = AudioBufferJsonExtensions.FromJson(json);

        Assert.NotNull(result);
    }

    [Fact]
    public void FromJson_NullOrEmptyString_ReturnsNull()
    {
        Assert.Null(AudioBufferJsonExtensions.FromJson(null));
        Assert.Null(AudioBufferJsonExtensions.FromJson(string.Empty));
        Assert.Null(AudioBufferJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        var invalidJson = "this is not json";

        Assert.Throws<JsonException>(() => AudioBufferJsonExtensions.FromJson(invalidJson));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        var buffer = CreateSampleBuffer();
        var json = buffer.ToJson();

        var success = AudioBufferJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        var invalidJson = "invalid json";

        var success = AudioBufferJsonExtensions.TryFromJson(invalidJson, out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
