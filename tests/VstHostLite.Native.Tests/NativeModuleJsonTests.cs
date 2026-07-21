using System;
using System.IO;
using System.Text.Json;
using Xunit;

namespace VstHostLite.Native.Tests;

public class NativeModuleJsonTests
{
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

    [Fact]
    public void FromJson_WithNullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => NativeModuleJsonExtensions.FromJson(json!));
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void FromJson_WithEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => NativeModuleJsonExtensions.FromJson(json));
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void FromJson_WithWhitespaceJson_ThrowsArgumentException()
    {
        // Arrange
        var json = "   \n\t  ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => NativeModuleJsonExtensions.FromJson(json));
        Assert.Equal("json", exception.ParamName);
    }

    [Fact]
    public void FromJson_WithInvalidJson_ThrowsJsonException()
    {
        // Arrange
        var json = "invalid json";

        // Act & Assert
        Assert.Throws<JsonException>(() => NativeModuleJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_WithMissingPathProperty_ThrowsJsonException()
    {
        // Arrange
        var json = "{}";

        // Act & Assert
        Assert.Throws<JsonException>(() => NativeModuleJsonExtensions.FromJson(json));
    }

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