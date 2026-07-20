using System.IO;
using Xunit;

namespace VstHostLite.Native.Tests;

public class PluginScanCacheTests
{
    [Fact]
    public void TryGetFresh_ReturnsFalse_WhenPluginDoesNotExist()
    {
        // Arrange
        var nonExistentPath = "/path/that/does/not/exist.vst3";

        // Act
        var result = PluginScanCache.TryGetFresh(nonExistentPath, out var cachedInfo);

        // Assert
        Assert.False(result);
        Assert.Null(cachedInfo);
    }

    [Fact]
    public void SaveAndTryGetFresh_RoundtripWorks()
    {
        // Arrange
        var testPluginPath = Path.GetTempFileName() + ".vst3";
        var cacheFilePath = testPluginPath + PluginScanCache.CacheFileExtension;

        try
        {
            var testInfos = new List<PluginClassInfo>
            {
                new PluginClassInfo("test-cid-1", "Audio Module", "Test Plugin 1"),
                new PluginClassInfo("test-cid-2", "Fx", "Test Plugin 2")
            };

            // Act - save to cache
            PluginScanCache.Save(testPluginPath, testInfos);

            // Act - retrieve from cache
            var result = PluginScanCache.TryGetFresh(testPluginPath, out var cachedInfo);

            // Assert
            Assert.True(result);
            Assert.NotNull(cachedInfo);
            Assert.Equal(2, cachedInfo?.Count);
            Assert.Equal("Test Plugin 1", cachedInfo?[0].Name);
            Assert.Equal("Test Plugin 2", cachedInfo?[1].Name);
        }
        finally
        {
            // Cleanup
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }
            if (File.Exists(testPluginPath))
            {
                File.Delete(testPluginPath);
            }
        }
    }

    [Fact]
    public void TryGetFresh_ReturnsFalse_WhenCacheIsStale()
    {
        // Arrange
        var testPluginPath = Path.GetTempFileName() + ".vst3";
        var cacheFilePath = testPluginPath + PluginScanCache.CacheFileExtension;

        try
        {
            // Create a test plugin file
            File.WriteAllText(testPluginPath, "dummy");

            var testInfos = new List<PluginClassInfo>
            {
                new PluginClassInfo("test-cid", "Test", "Test Plugin")
            };

            // Save to cache
            PluginScanCache.Save(testPluginPath, testInfos);

            // Wait a bit to ensure timestamp difference
            Thread.Sleep(10);

            // Modify the plugin file (making cache stale)
            File.WriteAllText(testPluginPath, "modified");

            // Act - try to get stale cache
            var result = PluginScanCache.TryGetFresh(testPluginPath, out var cachedInfo);

            // Assert - should return false because cache is stale
            Assert.False(result);
            Assert.Null(cachedInfo);
        }
        finally
        {
            // Cleanup
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }
            if (File.Exists(testPluginPath))
            {
                File.Delete(testPluginPath);
            }
        }
    }

    [Fact]
    public void Clear_RemovesCacheFile()
    {
        // Arrange
        var testPluginPath = Path.GetTempFileName() + ".vst3";
        var cacheFilePath = testPluginPath + PluginScanCache.CacheFileExtension;

        try
        {
            var testInfos = new List<PluginClassInfo>
            {
                new PluginClassInfo("test-cid", "Test", "Test Plugin")
            };

            // Save to cache
            PluginScanCache.Save(testPluginPath, testInfos);

            // Verify cache exists
            Assert.True(File.Exists(cacheFilePath));

            // Act - clear cache
            PluginScanCache.Clear(testPluginPath);

            // Assert - cache should be removed
            Assert.False(File.Exists(cacheFilePath));
        }
        finally
        {
            // Cleanup
            if (File.Exists(testPluginPath))
            {
                File.Delete(testPluginPath);
            }
        }
    }

    [Fact]
    public void ClearAll_RemovesAllCacheFiles()
    {
        // Arrange
        var testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        try
        {
            var plugin1Path = Path.Combine(testDir, "plugin1.vst3");
            var plugin2Path = Path.Combine(testDir, "plugin2.vst3");

            var testInfos1 = new List<PluginClassInfo> { new PluginClassInfo("cid1", "Cat1", "Plugin1") };
            var testInfos2 = new List<PluginClassInfo> { new PluginClassInfo("cid2", "Cat2", "Plugin2") };

            PluginScanCache.Save(plugin1Path, testInfos1);
            PluginScanCache.Save(plugin2Path, testInfos2);

            // Verify both caches exist
            Assert.True(File.Exists(plugin1Path + PluginScanCache.CacheFileExtension));
            Assert.True(File.Exists(plugin2Path + PluginScanCache.CacheFileExtension));

            // Act - clear all caches
            PluginScanCache.ClearAll();

            // Assert - both caches should be removed
            Assert.False(File.Exists(plugin1Path + PluginScanCache.CacheFileExtension));
            Assert.False(File.Exists(plugin2Path + PluginScanCache.CacheFileExtension));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
        }
    }
}
