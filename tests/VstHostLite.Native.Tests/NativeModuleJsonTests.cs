using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace VstHostLite.Native.Tests;

    /// <summary>
    /// Tests for the NativeModuleJsonExtensions class.
    /// </summary>
public class NativeModuleJsonTests
{
    /// <summary>
    /// Tests that FromJson returns a NativeModule when given valid JSON with an existing path.
    /// </summary>
    [Fact]
    public void FromJson_WithValidJson_ReturnsNativeModule()
    {
        // Arrange - Create a valid JSON string with a path that exists
        var testDllPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var json = $"{{\"path\": \"{testDllPath}\"}}";

        // Act
        var module = NativeModuleJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(module);
        Assert.Equal(testDllPath, module.Path);
        module.Dispose();
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentNullException when json is null.
    /// </summary>
    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => NativeModuleJsonExtensions.FromJson(json!));
        Assert.Equal("json", exception.ParamName);
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentException when json is empty.
    /// </summary>
    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => NativeModuleJsonExtensions.FromJson(json));
        Assert.Equal("json", exception.ParamName);
    }

    /// <summary>
    /// Tests that FromJson throws ArgumentException when json is only whitespace.
    /// </summary>
    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => NativeModuleJsonExtensions.FromJson(json));
        Assert.Equal("json", exception.ParamName);
    }

    /// <summary>
    /// Tests that FromJson throws JsonException when json is invalid.
    /// </summary>
    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json";

        // Act & Assert
        Assert.Throws<JsonException>(() => NativeModuleJsonExtensions.FromJson(json));
    }

    /// <summary>
    /// Tests that FromJson throws JsonException when the JSON is missing the "path" property.
    /// </summary>
    [Fact]
    public void FromJson_WithMissingPathProperty_ThrowsJsonException()
    {
        // Arrange
        var json = "{}";

        // Act & Assert
        Assert.Throws<JsonException>(() => NativeModuleJsonExtensions.FromJson(json));
    }

    /// <summary>
    /// Tests that TryFromJson returns true and a module when given valid JSON.
    /// </summary>
    [Fact]
    public void TryFromJson_WithValidJson_ReturnsTrueAndModule()
    {
        // Arrange
        var testDllPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var json = $"{{\"path\": \"{testDllPath}\"}}";

        // Act
        var result = NativeModuleJsonExtensions.TryFromJson(json, out var module);

        // Assert
        Assert.True(result);
        Assert.NotNull(module);
        Assert.Equal(testDllPath, module.Path);
        module?.Dispose();
    }

    /// <summary>
    /// Tests that TryFromJson returns false and null when given invalid JSON.
    /// </summary>
    [Fact]
    public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        var json = "invalid json";

        // Act
        var result = NativeModuleJsonExtensions.TryFromJson(json, out var module);

        // Assert
        Assert.False(result);
        Assert.Null(module);
    }

    /// <summary>
    /// Tests that TryFromJson throws ArgumentNullException when json is null.
    /// </summary>
    [Fact]
    public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;
        NativeModule? module = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => NativeModuleJsonExtensions.TryFromJson(json!, out module));
        Assert.Equal("json", exception.ParamName);
    }

    /// <summary>
    /// Tests that serializing and then deserializing preserves the path.
    /// </summary>
    [Fact]
    public void Roundtrip_SerializationDeserialization_PreservesPath()
    {
        // Arrange - Test the roundtrip by manually creating JSON that matches the expected format
        var originalPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var json = $"{{\"path\": \"{originalPath}\"}}";

        // Act - deserialize then serialize back
        var module = NativeModuleJsonExtensions.FromJson(json);
        var jsonBack = NativeModuleJsonExtensions.ToJson(module);
        var moduleBack = NativeModuleJsonExtensions.FromJson(jsonBack);

        // Assert
        Assert.Equal(originalPath, module.Path);
        Assert.Equal(originalPath, moduleBack.Path);
        module.Dispose();
        moduleBack.Dispose();
    }

    /// <summary>
    /// Tests that serializing and then deserializing with indented JSON preserves the path.
    /// </summary>
    [Fact]
    public void Roundtrip_WithIndentedSerialization_PreservesPath()
    {
        // Arrange
        var originalPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var json = $"{{\"path\": \"{originalPath}\"}}";

        // Act
        var module = NativeModuleJsonExtensions.FromJson(json);
        var jsonBack = NativeModuleJsonExtensions.ToJson(module, indented: true);
        var moduleBack = NativeModuleJsonExtensions.FromJson(jsonBack);

        // Assert
        Assert.Equal(originalPath, module.Path);
        Assert.Equal(originalPath, moduleBack.Path);
        module.Dispose();
        moduleBack.Dispose();
    }

    /// <summary>
    /// Tests that ToJson produces camelCase property names.
    /// </summary>
    [Fact]
    public void ToJson_ProducesCamelCaseProperties()
    {
        // Arrange
        var testDllPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var module = NativeModule.Load(testDllPath);

        // Act - explicitly call NativeModuleJsonExtensions.ToJson
        var json = NativeModuleJsonExtensions.ToJson(module);

        // Assert
        Assert.Contains("path", json); // Should be camelCase, not PascalCase
        Assert.DoesNotContain("Path", json); // Should not contain PascalCase
        module.Dispose();
    }

    /// <summary>
    /// Tests that ToJson with indented true returns formatted JSON (starts with "{\n").
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
    {
        // Arrange
        var testDllPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var module = NativeModule.Load(testDllPath);

        // Act - explicitly call NativeModuleJsonExtensions.ToJson
        var json = NativeModuleJsonExtensions.ToJson(module, indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.StartsWith("{\n", json);
        Assert.Contains("path", json);
        module.Dispose();
    }

    /// <summary>
    /// Tests that FromJson throws FileNotFoundException when the path does not exist.
    /// </summary>
    [Fact]
    public void FromJson_WithNonExistentPath_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "/non/existent/path.dll";
        var json = $"{{\"path\": \"{nonExistentPath}\"}}";

        // Act & Assert
        // This should throw FileNotFoundException because the path doesn't exist
        Assert.Throws<FileNotFoundException>(() => NativeModuleJsonExtensions.FromJson(json));
    }

    /// <summary>
    /// Tests that TryFromJson returns false when the path does not exist.
    /// </summary>
    [Fact]
    public void TryFromJson_WithNonExistentPath_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = "/non/existent/path.dll";
        var json = $"{{\"path\": \"{nonExistentPath}\"}}";

        // Act
        var result = NativeModuleJsonExtensions.TryFromJson(json, out var module);

        // Assert
        Assert.False(result);
        Assert.Null(module);
    }

    /// <summary>
    /// Tests that the JSON produced by ToJson matches the expected format (starts and ends with braces, contains "path" and the testDllPath).
    /// </summary>
    [Fact]
    public void DtoSerializationShape_MatchesExpectedFormat()
    {
        // Arrange
        var testDllPath = Path.Combine(AppContext.BaseDirectory, "VstHostLite.Native.dll");
        var module = NativeModule.Load(testDllPath);

        // Act - explicitly call NativeModuleJsonExtensions.ToJson
        var json = NativeModuleJsonExtensions.ToJson(module);

        // Assert - Verify the JSON has the expected structure
        Assert.StartsWith("{", json);
        Assert.EndsWith("}", json);
        Assert.Contains("path", json);
        Assert.Contains(testDllPath, json);
        module.Dispose();
    }

    /// <summary>
    /// Tests that ToJson throws ArgumentNullException when the module is null.
    /// </summary>
    [Fact]
    public void ToJson_WithNullModule_ThrowsArgumentNullException()
    {
        // Arrange
        NativeModule? module = null;

        // Act & Assert - explicitly call NativeModuleJsonExtensions.ToJson
        var exception = Assert.Throws<ArgumentNullException>(() => NativeModuleJsonExtensions.ToJson(module!));
        Assert.Equal("value", exception.ParamName);
    }
}