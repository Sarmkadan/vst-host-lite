// Copyright (c) 2024.
// Licensed under the MIT license.

using System;
using System.Text.Json;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests;

public class ClipDetectorJsonExtensionsTests
{
    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        ClipDetectionResult? nullResult = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullResult!.ToJson());
    }

    [Fact]
    public void ToJson_Indented_ContainsNewLine()
    {
        // Arrange
        var result = new ClipDetectionResult();

        // Act
        var json = result.ToJson(indented: true);

        // Assert
        // Indented JSON should contain at least one newline character.
        Assert.Contains('\n', json);
    }

    [Fact]
    public void FromJson_NullOrEmpty_ReturnsNull()
    {
        // Null input
        Assert.Null(ClipDetectorJsonExtensions.FromJson(null));

        // Empty string
        Assert.Null(ClipDetectorJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_ValidJson_RoundTripProducesEquivalentObject()
    {
        // Arrange
        var original = new ClipDetectionResult();

        // Serialize using the extension method (ensures same settings are used)
        var json = original.ToJson();

        // Act
        var deserialized = ClipDetectorJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        // For a simple round‑trip we compare the JSON representations again.
        // This works even if ClipDetectionResult does not implement Equals.
        var reserialized = deserialized!.ToJson();
        Assert.Equal(json, reserialized);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndResult()
    {
        // Arrange
        var original = new ClipDetectionResult();
        var json = original.ToJson();

        // Act
        var success = ClipDetectorJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        // Verify that the deserialized instance round‑trips back to the same JSON.
        Assert.Equal(json, result!.ToJson());
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange: malformed JSON
        const string malformedJson = "{ this is not valid json }";

        // Act
        var success = ClipDetectorJsonExtensions.TryFromJson(malformedJson, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }
}
