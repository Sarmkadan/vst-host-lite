using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace VstHostLite.Native.Tests;

public class AudioGraphValidationJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidList_ReturnsJson()
    {
        var list = new List<string> { "error1", "error2" };
        var json = list.ToJson();

        Assert.Contains("error1", json);
        Assert.Contains("error2", json);
    }

    [Fact]
    public void ToJson_NullList_ThrowsArgumentNullException()
    {
        IReadOnlyList<string>? list = null;
        Assert.Throws<ArgumentNullException>(() => list!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsList()
    {
        var json = "[\"error1\",\"error2\"]";
        var result = AudioGraphValidationJsonExtensions.FromJson(json);

        Assert.Equal(2, result.Count);
        Assert.Equal("error1", result[0]);
        Assert.Equal("error2", result[1]);
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        var json = "invalid json";
        Assert.Throws<JsonException>(() => AudioGraphValidationJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => AudioGraphValidationJsonExtensions.FromJson(""));
        Assert.Throws<ArgumentException>(() => AudioGraphValidationJsonExtensions.FromJson("   "));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndValue()
    {
        var json = "[\"error1\"]";
        var success = AudioGraphValidationJsonExtensions.TryFromJson(json, out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal("error1", result![0]);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        var json = "invalid";
        var success = AudioGraphValidationJsonExtensions.TryFromJson(json, out var result);

        Assert.False(success);
        Assert.Null(result);
    }
}
