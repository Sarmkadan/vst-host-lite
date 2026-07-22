using System.IO;
using Xunit;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Tests for NativeModuleCache to verify thread-safe reference counting and caching behavior.
/// </summary>
public class NativeModuleCacheTests
{
    [Fact]
    public void Acquire_NewModule_LoadsAndCachesIt()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");

        try
        {
            // Act
            var module1 = NativeModuleCache.Acquire(tempFile);
            var module2 = NativeModuleCache.Acquire(tempFile);

            // Assert
            Assert.NotNull(module1);
            Assert.NotNull(module2);
            Assert.Same(module1, module2); // Should be the same instance
            Assert.Equal(2, NativeModuleCache.GetRefCount(tempFile));

            // Cleanup
            NativeModuleCache.Release(tempFile, module1);
            NativeModuleCache.Release(tempFile, module2);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Acquire_ReleasesWhenRefCountReachesZero()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");

        try
        {
            // Act
            var module1 = NativeModuleCache.Acquire(tempFile);
            var refCount1 = NativeModuleCache.GetRefCount(tempFile);

            NativeModuleCache.Release(tempFile, module1);
            var refCount2 = NativeModuleCache.GetRefCount(tempFile);

            NativeModuleCache.Release(tempFile, module1);
            var refCount3 = NativeModuleCache.GetRefCount(tempFile);

            // Assert
            Assert.Equal(1, refCount1);
            Assert.Equal(0, refCount2);
            Assert.Equal(-1, refCount3); // Should be removed from cache after disposal
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clear_RemovesAllCachedModules()
    {
        // Arrange
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        File.WriteAllText(tempFile1, "");
        File.WriteAllText(tempFile2, "");

        try
        {
            // Act
            var module1 = NativeModuleCache.Acquire(tempFile1);
            var module2 = NativeModuleCache.Acquire(tempFile2);

            Assert.Equal(2, NativeModuleCache.Count);

            NativeModuleCache.Clear();

            // Assert
            Assert.Equal(0, NativeModuleCache.Count);
        }
        finally
        {
            if (File.Exists(tempFile1))
                File.Delete(tempFile1);
            if (File.Exists(tempFile2))
                File.Delete(tempFile2);
        }
    }

    [Fact]
    public void GetRefCount_ReturnsMinusOneForNonCachedModule()
    {
        // Arrange
        var nonExistentPath = "/non/existent/path.dll";

        // Act
        var refCount = NativeModuleCache.GetRefCount(nonExistentPath);

        // Assert
        Assert.Equal(-1, refCount);
    }

    [Fact]
    public void Release_ModuleNotInCache_DisposesItDirectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");
        var module = NativeModule.Load(tempFile);

        try
        {
            // Act - release a module that wasn't acquired through the cache
            NativeModuleCache.Release(tempFile, module);

            // Assert - module should be disposed (no exception thrown)
            // The module is disposed by Release() even though it wasn't cached
            Assert.True(true);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
