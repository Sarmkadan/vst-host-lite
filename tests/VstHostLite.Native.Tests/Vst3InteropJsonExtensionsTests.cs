using System;
using System.Text.Json;
using Xunit;

namespace VstHostLite.Native.Tests;

public class Vst3InteropJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidPluginClassInfo_ReturnsJsonString()
    {
        // Arrange
        var pluginInfo = new PluginClassInfo(
            Cid: "ABCDEF1234567890ABCDEF1234567890",
            Category: "Audio Module Class",
            Name: "Test Plugin"
        );

        // Act
        var json = pluginInfo.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.NotEmpty(json);
        Assert.Contains("abcdef1234567890abcdef1234567890", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("audio module class", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("test plugin", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var pluginInfo = new PluginClassInfo(
            Cid: "1234567890ABCDEF1234567890ABCDEF",
            Category: "Effect",
            Name: "My Effect"
        );

        // Act
        var json = pluginInfo.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.StartsWith("{\n", json);
        Assert.Contains("  ", json); // Should have indentation
    }

    [Fact]
    public void ToJson_WithValidPluginClassInfo_WorksCorrectly()
    {
        // Arrange
        var pluginInfo = new PluginClassInfo(
            Cid: "ABCD",
            Category: "Test",
            Name: "Plugin"
        );

        // Act
        var json = pluginInfo.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("abcD", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FromJson_WithValidJson_ReturnsPluginClassInfo()
    {
        // Arrange - Create valid JSON for PluginClassInfo
        var json = "{\"cid\":\"ABCDEF1234567890ABCDEF1234567890\",\"category\":\"Audio Module Class\",\"name\":\"Test Plugin\"}";

        // Act
        var pluginInfo = Vst3InteropJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(pluginInfo);
        Assert.Equal("ABCDEF1234567890ABCDEF1234567890", pluginInfo.Value.Cid, ignoreCase: true);
        Assert.Equal("Audio Module Class", pluginInfo.Value.Category, ignoreCase: true);
        Assert.Equal("Test Plugin", pluginInfo.Value.Name, ignoreCase: true);
    }

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => Vst3InteropJsonExtensions.FromJson(json!)
        );
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Vst3InteropJsonExtensions.FromJson(json)
        );
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsJsonException()
    {
        // Arrange
        var json = " \n\t ";

        // Act & Assert
        Assert.Throws<JsonException>(
            () => Vst3InteropJsonExtensions.FromJson(json)
        );
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json";

        // Act & Assert
        Assert.Throws<JsonException>(
            () => Vst3InteropJsonExtensions.FromJson(json)
        );
    }

    [Fact]
    public void FromJson_WithEmptyObject_ReturnsPluginClassInfoWithNullProperties()
    {
        // Arrange
        var json = "{}";

        // Act
        var pluginInfo = Vst3InteropJsonExtensions.FromJson(json);

        // Assert - Empty object deserializes to PluginClassInfo with null/empty properties
        Assert.NotNull(pluginInfo);
        Assert.Null(pluginInfo.Value.Cid);
        Assert.Null(pluginInfo.Value.Category);
        Assert.Null(pluginInfo.Value.Name);
    }

    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndPluginClassInfo()
    {
        // Arrange
        var json = "{\"cid\":\"1234567890ABCDEF1234567890ABCDEF\",\"category\":\"Effect\",\"name\":\"My Effect\"}";

        // Act
        var result = Vst3InteropJsonExtensions.TryFromJson(json, out var pluginInfo);

        // Assert
        Assert.True(result);
        Assert.NotNull(pluginInfo);
        Assert.Equal("1234567890ABCDEF1234567890ABCDEF", pluginInfo.Value.Cid, ignoreCase: true);
        Assert.Equal("Effect", pluginInfo.Value.Category, ignoreCase: true);
        Assert.Equal("My Effect", pluginInfo.Value.Name, ignoreCase: true);
    }

    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var result = Vst3InteropJsonExtensions.TryFromJson(json, out var pluginInfo);

        // Assert
        Assert.False(result);
        Assert.Null(pluginInfo);
    }

    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;
        PluginClassInfo? pluginInfo = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => Vst3InteropJsonExtensions.TryFromJson(json!, out pluginInfo)
        );
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void TryFromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => Vst3InteropJsonExtensions.TryFromJson(json, out _)
        );
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void Roundtrip_SerializationDeserialization_PreservesAllProperties()
    {
        // Arrange
        var original = new PluginClassInfo(
            Cid: "AABBCCDDEEFF00112233445566778899",
            Category: "Instrument",
            Name: "Test Instrument"
        );

        // Act - Serialize then deserialize
        var json = original.ToJson();
        var deserialized = Vst3InteropJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Cid, deserialized.Value.Cid, ignoreCase: true);
        Assert.Equal(original.Category, deserialized.Value.Category, ignoreCase: true);
        Assert.Equal(original.Name, deserialized.Value.Name, ignoreCase: true);
    }

    [Fact]
    public void Roundtrip_WithIndentedSerialization_PreservesAllProperties()
    {
        // Arrange
        var original = new PluginClassInfo(
            Cid: "FFEEDDCCBBAA99887766554433221100",
            Category: "Fx",
            Name: "Reverb"
        );

        // Act
        var json = original.ToJson(indented: true);
        var deserialized = Vst3InteropJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(original.Cid, deserialized.Value.Cid, ignoreCase: true);
        Assert.Equal(original.Category, deserialized.Value.Category, ignoreCase: true);
        Assert.Equal(original.Name, deserialized.Value.Name, ignoreCase: true);
    }

    [Fact]
    public void Roundtrip_TryFromJson_PreservesAllProperties()
    {
        // Arrange
        var original = new PluginClassInfo(
            Cid: "11223344556677889900AABBCCDDEEFF",
            Category: "Mixer",
            Name: "Audio Mixer"
        );

        // Act
        var json = original.ToJson();
        var result = Vst3InteropJsonExtensions.TryFromJson(json, out var deserialized);

        // Assert
        Assert.True(result);
        Assert.NotNull(deserialized);
        Assert.Equal(original.Cid, deserialized.Value.Cid, ignoreCase: true);
        Assert.Equal(original.Category, deserialized.Value.Category, ignoreCase: true);
        Assert.Equal(original.Name, deserialized.Value.Name, ignoreCase: true);
    }

    [Fact]
    public void ToJson_ProducesCamelCaseProperties()
    {
        // Arrange
        var pluginInfo = new PluginClassInfo(
            Cid: "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            Category: "TestCategory",
            Name: "TestName"
        );

        // Act
        var json = pluginInfo.ToJson();

        // Assert - Verify camelCase formatting
        Assert.Contains("\"cid\"", json);
        Assert.Contains("\"category\"", json);
        Assert.Contains("\"name\"", json);
        Assert.DoesNotContain("\"Cid\"", json);
        Assert.DoesNotContain("\"Category\"", json);
        Assert.DoesNotContain("\"Name\"", json);
    }

    [Fact]
    public void FromJson_WithWhitespaceTrimmed_ReturnsPluginClassInfo()
    {
        // Arrange - JSON with leading/trailing whitespace
        var json = "  {\"cid\":\"ABCD\",\"category\":\"cat\",\"name\":\"name\"}  \n";

        // Act
        var pluginInfo = Vst3InteropJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(pluginInfo);
        Assert.Equal("ABCD", pluginInfo.Value.Cid, ignoreCase: true);
    }

    [Fact]
    public void FromJson_WithEmptyStringProperties_ReturnsPluginClassInfoWithEmptyStrings()
    {
        // Arrange - JSON with empty string values
        var json = "{\"cid\":\"\",\"category\":\"\",\"name\":\"\"}";

        // Act
        var pluginInfo = Vst3InteropJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(pluginInfo);
        Assert.Empty(pluginInfo.Value.Cid);
        Assert.Empty(pluginInfo.Value.Category);
        Assert.Empty(pluginInfo.Value.Name);
    }
}