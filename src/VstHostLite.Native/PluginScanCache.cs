using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides caching for VST3 plugin scanning results.
/// Caches plugin path -> (last write time, list of PluginClassInfo)
/// Invalidates entries when the plugin file's modification time changes.
/// </summary>
public static class PluginScanCache
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private const string CacheFileExtension = ".vst3.cache.json";

    /// <summary>
    /// Gets the cache file path for a plugin.
    /// </summary>
    /// <param name="pluginPath">The path to the VST3 plugin.</param>
    /// <returns>The path to the cache file.</returns>
    private static string GetCacheFilePath(string pluginPath)
    {
        return pluginPath + CacheFileExtension;
    }

    /// <summary>
    /// Attempts to get cached plugin class info for a plugin path.
    /// Returns null if cache is invalid or doesn't exist.
    /// </summary>
    /// <param name="pluginPath">The path to the VST3 plugin.</param>
    /// <param name="cachedInfo">Output: cached plugin class info if valid.</param>
    /// <returns>True if valid cached data exists; false otherwise.</returns>
    public static bool TryGetFresh(string pluginPath, out List<PluginClassInfo>? cachedInfo)
    {
        cachedInfo = null;

        if (!File.Exists(pluginPath))
        {
            return false;
        }

        var cacheFilePath = GetCacheFilePath(pluginPath);

        // Check if cache file exists
        if (!File.Exists(cacheFilePath))
        {
            return false;
        }

        try
        {
            var cacheFileInfo = new FileInfo(cacheFilePath);
            var pluginFileInfo = new FileInfo(pluginPath);

            // Check if plugin file has been modified since cache was created
            if (cacheFileInfo.LastWriteTimeUtc < pluginFileInfo.LastWriteTimeUtc)
            {
                // Plugin file is newer than cache, invalidate cache
                File.Delete(cacheFilePath);
                return false;
            }

            // Read and deserialize cache
            var json = File.ReadAllText(cacheFilePath);
            cachedInfo = JsonSerializer.Deserialize<List<PluginClassInfo>>(json, _jsonOptions);

            return cachedInfo != null;
        }
        catch
        {
            // If any error occurs reading/deserializing cache, treat as invalid
            return false;
        }
    }

    /// <summary>
    /// Stores plugin class info in the cache.
    /// </summary>
    /// <param name="pluginPath">The path to the VST3 plugin.</param>
    /// <param name="pluginClassInfos">List of plugin class info to cache.</param>
    public static void Save(string pluginPath, List<PluginClassInfo> pluginClassInfos)
    {
        if (pluginClassInfos == null || pluginClassInfos.Count == 0)
        {
            // Don't cache empty results
            return;
        }

        var cacheFilePath = GetCacheFilePath(pluginPath);
        var json = JsonSerializer.Serialize(pluginClassInfos, _jsonOptions);
        File.WriteAllText(cacheFilePath, json);
    }

    /// <summary>
    /// Clears the cache for a specific plugin.
    /// </summary>
    /// <param name="pluginPath">The path to the VST3 plugin.</param>
    public static void Clear(string pluginPath)
    {
        var cacheFilePath = GetCacheFilePath(pluginPath);
        if (File.Exists(cacheFilePath))
        {
            File.Delete(cacheFilePath);
        }
    }

    /// <summary>
    /// Clears all plugin scan caches.
    /// </summary>
    public static void ClearAll()
    {
        var directory = Path.GetDirectoryName(typeof(PluginScanCache).Assembly.Location);
        if (directory != null)
        {
            var cacheFiles = Directory.GetFiles(directory, "*" + CacheFileExtension);
            foreach (var cacheFile in cacheFiles)
            {
                try
                {
                    File.Delete(cacheFile);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
    }
}
