using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace VstHostLite.Native.Tests;

public class NativeModuleExtensionsTests
{
    private const string TestPath = @"C:\Program Files\Test\testmodule.dll";
    private const string TestPathNoExt = @"C:\Program Files\Test\testmodule";
    private const string TestPathExe = @"C:\Program Files\Test\testapp.exe";
    private const string TestPathSo = @"/usr/lib/testmodule.so";
    private const string EmptyPath = "";

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
    public void GetFileNameWithoutExtension_WithValidPath_ReturnsFileNameWithoutExtension()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.GetFileNameWithoutExtension();

        // Assert
        Assert.Equal("testmodule", result);
    }

    [Fact]
    public void GetFileNameWithoutExtension_WithPathWithoutExtension_ReturnsFileName()
    {
        // Arrange
        var module = CreateTestModule(TestPathNoExt);

        // Act
        var result = module.GetFileNameWithoutExtension();

        // Assert
        Assert.Equal("testmodule", result);
    }

    [Fact]
    public void GetFileNameWithoutExtension_WithEmptyPath_ReturnsEmptyString()
    {
        // Arrange
        var module = CreateTestModule(EmptyPath);

        // Act
        var result = module.GetFileNameWithoutExtension();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetFileNameWithoutExtension_WithNullPath_ReturnsNull()
    {
        // Arrange
        NativeModule? module = null;

        // Act
        var result = module?.GetFileNameWithoutExtension();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDirectory_WithValidPath_ReturnsDirectory()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.GetDirectory();

        // Assert
        Assert.Equal(@"C:\Program Files\Test", result);
    }

    [Fact]
    public void GetDirectory_WithFileNameOnly_ReturnsNull()
    {
        // Arrange
        var module = CreateTestModule(Path.GetFileName(TestPath));

        // Act
        var result = module.GetDirectory();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDirectory_WithEmptyPath_ReturnsNull()
    {
        // Arrange
        var module = CreateTestModule(EmptyPath);

        // Act
        var result = module.GetDirectory();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetDirectory_WithNullPath_ReturnsNull()
    {
        // Arrange
        NativeModule? module = null;

        // Act
        var result = module?.GetDirectory();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void IsWindowsDll_WithDllExtension_ReturnsTrue()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.IsWindowsDll();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWindowsDll_WithExeExtension_ReturnsFalse()
    {
        // Arrange
        var module = CreateTestModule(TestPathExe);

        // Act
        var result = module.IsWindowsDll();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWindowsDll_WithSoExtension_ReturnsFalse()
    {
        // Arrange
        var module = CreateTestModule(TestPathSo);

        // Act
        var result = module.IsWindowsDll();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWindowsDll_WithUpperCaseDllExtension_ReturnsTrue()
    {
        // Arrange
        var module = CreateTestModule(@"C:\Test\TestModule.DLL");

        // Act
        var result = module.IsWindowsDll();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWindowsDll_WithNullPath_ReturnsFalse()
    {
        // Arrange
        NativeModule? module = null;

        // Act
        var result = module?.IsWindowsDll();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFileVersionInfo_WithValidModule_ReturnsVersionInfo()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.GetFileVersionInfo();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyDictionary<string, string>>(result);
    }

    [Fact]
    public void GetFileVersionInfo_WithEmptyPath_ReturnsEmptyDictionary()
    {
        // Arrange
        var module = CreateTestModule(EmptyPath);

        // Act
        var result = module.GetFileVersionInfo();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetFileVersionInfo_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        NativeModule? module = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => module!.GetFileVersionInfo());
    }

    [Fact]
    public void GetFileVersionInfo_ReturnsExpectedKeys()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.GetFileVersionInfo();

        // Assert
        var keys = result.Keys.ToList();
        Assert.Contains(keys, key => key.Equals(nameof(System.Diagnostics.FileVersionInfo.FileVersion), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keys, key => key.Equals(nameof(System.Diagnostics.FileVersionInfo.ProductVersion), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keys, key => key.Equals(nameof(System.Diagnostics.FileVersionInfo.CompanyName), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keys, key => key.Equals(nameof(System.Diagnostics.FileVersionInfo.ProductName), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keys, key => key.Equals(nameof(System.Diagnostics.FileVersionInfo.FileDescription), StringComparison.OrdinalIgnoreCase));
        Assert.Contains(keys, key => key.Equals(nameof(System.Diagnostics.FileVersionInfo.LegalCopyright), StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetFileSize_WithValidModule_ReturnsFileSize()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.GetFileSize();

        // Assert
        Assert.True(result > 0);
    }

    [Fact]
    public void GetFileSize_WithEmptyPath_ThrowsFileNotFoundException()
    {
        // Arrange
        var module = CreateTestModule(EmptyPath);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => module.GetFileSize());
    }

    [Fact]
    public void GetFileSize_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        NativeModule? module = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => module!.GetFileSize());
    }

    [Fact]
    public void GetFileSize_ReturnsCorrectSize()
    {
        // Arrange
        var module = CreateTestModule(TestPath);
        var expectedSize = new FileInfo(TestPath).Length;

        // Act
        var result = module.GetFileSize();

        // Assert
        Assert.Equal(expectedSize, result);
    }

    [Fact]
    public void LastWriteTimeUtc_WithValidModule_ReturnsLastWriteTime()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.LastWriteTimeUtc();

        // Assert
        Assert.True(result <= DateTime.UtcNow);
        Assert.True(result >= DateTime.UtcNow.AddDays(-365));
    }

    [Fact]
    public void LastWriteTimeUtc_WithEmptyPath_ThrowsFileNotFoundException()
    {
        // Arrange
        var module = CreateTestModule(EmptyPath);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => module.LastWriteTimeUtc());
    }

    [Fact]
    public void LastWriteTimeUtc_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        NativeModule? module = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => module!.LastWriteTimeUtc());
    }

    [Fact]
    public void LastWriteTimeUtc_ReturnsUtcTime()
    {
        // Arrange
        var module = CreateTestModule(TestPath);

        // Act
        var result = module.LastWriteTimeUtc();

        // Assert
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    [Fact]
    public void AllMethods_WithNullModule_ThrowArgumentNullException()
    {
        // Arrange
        NativeModule? module = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => module!.GetFileNameWithoutExtension());
        Assert.Throws<ArgumentNullException>(() => module!.GetDirectory());
        Assert.Throws<ArgumentNullException>(() => module!.IsWindowsDll());
        Assert.Throws<ArgumentNullException>(() => module!.GetFileVersionInfo());
        Assert.Throws<ArgumentNullException>(() => module!.GetFileSize());
        Assert.Throws<ArgumentNullException>(() => module!.LastWriteTimeUtc());
    }
}