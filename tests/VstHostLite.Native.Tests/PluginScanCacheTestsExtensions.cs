using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Extension methods that provide reusable helpers for <see cref="PluginScanCacheTests"/>.
/// </summary>
public static class PluginScanCacheTestsExtensions
{
    /// <summary>
    /// Creates a temporary plugin file with the optional <paramref name="content"/>.
    /// The file name ends with <c>.vst3</c> so that <see cref="PluginScanCache"/> treats it as a plugin.
    /// </summary>
    /// <param name="test">The test instance (used only for argument validation).</param>
    /// <param name="content">Optional text to write into the file.</param>
    /// <returns>The full path of the created temporary plugin file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static string CreateTempPluginFile(this PluginScanCacheTests test, string? content = null)
    {
        ArgumentNullException.ThrowIfNull(test);
        var path = Path.GetTempFileName() + ".vst3";
        if (content is not null)
            File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Generates a read‑only list of <see cref="PluginClassInfo"/> objects from the supplied tuples.
    /// </summary>
    /// <param name="test">The test instance (used only for argument validation).</param>
    /// <param name="entries">
    /// A collection of tuples where each tuple contains the component ID, category and name of a plugin class.
    /// </param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of <see cref="PluginClassInfo"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    public static IReadOnlyList<PluginClassInfo> CreateSamplePluginInfos(
        this PluginScanCacheTests test,
        params (string cid, string category, string name)[] entries)
    {
        ArgumentNullException.ThrowIfNull(test);
        var list = new List<PluginClassInfo>();
        foreach (var (cid, category, name) in entries)
            list.Add(new PluginClassInfo(cid, category, name));
        return list;
    }

    /// <summary>
    /// Asserts that the cache file for <paramref name="pluginPath"/> exists.
    /// </summary>
    /// <param name="test">The test instance (used only for argument validation).</param>
    /// <param name="pluginPath">The path of the plugin file whose cache should exist.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pluginPath"/> is <c>null</c> or empty.</exception>
    public static void AssertCacheFileExists(this PluginScanCacheTests test, string pluginPath)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(pluginPath);
        var cachePath = pluginPath + PluginScanCache.CacheFileExtension;
        Assert.True(File.Exists(cachePath), $"Cache file '{cachePath}' should exist.");
    }

    /// <summary>
    /// Deletes the cache file for <paramref name="pluginPath"/> if it exists.
    /// </summary>
    /// <param name="test">The test instance (used only for argument validation).</param>
    /// <param name="pluginPath">The path of the plugin file whose cache should be removed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="test"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pluginPath"/> is <c>null</c> or empty.</exception>
    public static void DeleteCacheFileIfExists(this PluginScanCacheTests test, string pluginPath)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentException.ThrowIfNullOrEmpty(pluginPath);
        var cachePath = pluginPath + PluginScanCache.CacheFileExtension;
        if (File.Exists(cachePath))
            File.Delete(cachePath);
    }
}
