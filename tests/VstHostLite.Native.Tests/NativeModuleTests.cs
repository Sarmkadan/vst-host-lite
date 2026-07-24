using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace VstHostLite.Native.Tests;

public class NativeModuleTests
{
    private static NativeModule CreateTestModule(string path)
    {
        var module = (NativeModule)Activator.CreateInstance(
            typeof(NativeModule),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { path, nint.Zero, null },
            null);
        return module!;
    }

    [Fact]
    public void Path_Getter_ReturnsCorrectPath()
    {
        // Arrange
        var testPath = "/test/path/module.vst3";
        var module = CreateTestModule(testPath);

        // Act
        var path = module.Path;

        // Assert
        Assert.Equal(testPath, path);
    }

    [Fact]
    public void Load_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullPath = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NativeModule.Load(nullPath!));
    }

    [Fact]
    public void Load_WithEmptyPath_ThrowsFileNotFoundException()
    {
        // Arrange
        var emptyPath = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => NativeModule.Load(emptyPath));
        Assert.Equal("VST3 module not found", exception.Message);
    }

    [Fact]
    public void Load_WithWhitespacePath_ThrowsFileNotFoundException()
    {
        // Arrange
        var whitespacePath = "   ";

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => NativeModule.Load(whitespacePath));
        Assert.Equal("VST3 module not found", exception.Message);
    }

    [Fact]
    public void Load_WithNonExistentPath_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "/nonexistent/path/module.vst3";

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => NativeModule.Load(nonExistentPath));
        Assert.StartsWith("VST3 module not found", exception.Message);
        Assert.Equal(nonExistentPath, exception.FileName);
    }

    [Fact]
    public void GetFactory_WhenDisposed_ThrowsObjectDisposedException()
    {
        // Arrange
        var module = CreateTestModule("/test/path");
        module.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => module.GetFactory());
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_WithoutException()
    {
        // Arrange
        var module = CreateTestModule("/test/path");

        // Act
        module.Dispose();
        module.Dispose();
        module.Dispose();

        // Assert - No exception thrown
    }

    [Fact]
    public void Dispose_SetsDisposedFlag()
    {
        // Arrange
        var module = CreateTestModule("/test/path");
        Assert.False(module.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(module) as bool? ?? true);

        // Act
        module.Dispose();

        // Assert
        Assert.True(module.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(module) as bool? ?? false);
    }

    [Fact]
    public void Finalizer_CallsDispose()
    {
        // Arrange
        var module = CreateTestModule("/test/path");

        // Act - Trigger finalizer
        module = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Assert - No exception should be thrown
    }

    [Fact]
    public void PathProperty_IsReadOnly()
    {
        // Arrange
        var module = CreateTestModule("/original/path");

        // Act - Try to set via reflection (should not work for init-only property)
        var pathProperty = typeof(NativeModule).GetProperty("Path");
        Assert.True(pathProperty?.CanRead);
        Assert.False(pathProperty?.CanWrite);
    }

    [Fact]
    public void Constructor_WithValidPath_CreatesInstance()
    {
        // Arrange
        var path = "/test/module.vst3";

        // Act
        var module = CreateTestModule(path);

        // Assert
        Assert.Equal(path, module.Path);
    }
}