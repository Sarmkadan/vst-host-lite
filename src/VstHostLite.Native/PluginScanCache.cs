using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VstHostLite.Native;

/// <summary>
/// Provides caching for VST3 plugin scanning results.
/// Caches plugin path -> (file size, last write time, list of PluginClassInfo)
/// Invalidates entries when the plugin file's modification time or size changes.
/// Supports process-isolated scanning to prevent crashes from affecting the main process.
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

    public const string CacheFileExtension = ".vst3.cache.json";
    public const int CacheSchemaVersion = 2;

    /// <summary>
    /// Cache entry structure containing metadata for validation.
    /// </summary>
    private sealed class CacheEntry
    {
        /// <summary>
        /// Schema version for cache format compatibility.
        /// </summary>
        public int SchemaVersion { get; set; } = CacheSchemaVersion;

        /// <summary>
        /// File size of the plugin at the time of caching (bytes).
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Last write time of the plugin at the time of caching (UTC).
        /// </summary>
        public DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        /// List of plugin class information.
        /// </summary>
        public List<PluginClassInfo>? PluginClasses { get; set; }
    }

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
    /// <exception cref="ArgumentNullException"><paramref name="pluginPath"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="pluginPath"/> is empty or whitespace</exception>
    public static bool TryGetFresh(string pluginPath, out List<PluginClassInfo>? cachedInfo)
    {
        ArgumentNullException.ThrowIfNull(pluginPath);

        if (string.IsNullOrWhiteSpace(pluginPath))
        {
            throw new ArgumentException("Plugin path cannot be empty or whitespace.", nameof(pluginPath));
        }

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
            var pluginFileInfo = new FileInfo(pluginPath);
            var cacheEntry = JsonSerializer.Deserialize<CacheEntry>(File.ReadAllText(cacheFilePath), _jsonOptions);

            // Validate cache entry structure
            if (cacheEntry?.PluginClasses == null || cacheEntry.SchemaVersion != CacheSchemaVersion)
            {
                // Invalid schema version or missing data, treat as stale
                File.Delete(cacheFilePath);
                return false;
            }

            // Check if plugin file has changed since cache was created
            // Use both file size and last write time for more robust invalidation
            if (cacheEntry.FileSize != pluginFileInfo.Length ||
                cacheEntry.LastWriteTimeUtc != pluginFileInfo.LastWriteTimeUtc)
            {
                // Plugin file has changed, invalidate cache
                File.Delete(cacheFilePath);
                return false;
            }

            cachedInfo = cacheEntry.PluginClasses;
            return true;
        }
        catch
        {
            // If any error occurs reading/deserializing cache, treat as invalid
            return false;
        }
    }

    /// <summary>
    /// Scans a plugin using a child process for crash isolation.
    /// </summary>
    /// <param name="pluginPath">The path to the VST3 plugin.</param>
    /// <returns>List of plugin class information, or null if scanning failed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pluginPath"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="pluginPath"/> is empty or whitespace</exception>
    public static List<PluginClassInfo>? ScanWithIsolation(string pluginPath)
    {
        ArgumentNullException.ThrowIfNull(pluginPath);

        if (string.IsNullOrWhiteSpace(pluginPath))
        {
            throw new ArgumentException("Plugin path cannot be empty or whitespace.", nameof(pluginPath));
        }

        if (!File.Exists(pluginPath))
        {
            return null;
        }

        try
        {
            // Use process isolation to scan the plugin
            // This prevents crashes in the plugin from affecting the main process
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Environment.ProcessPath ?? throw new InvalidOperationException("Could not determine current process path."),
                    Arguments = $"scan-one \"{pluginPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                }
            };

            process.Start();

            // Wait for completion with timeout to prevent hanging
            if (!process.WaitForExit(10000)) // 10 second timeout
            {
                process.Kill();
                return null;
            }

            if (process.ExitCode != 0)
            {
                // Non-zero exit code indicates failure (e.g., plugin crashed during scan)
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            if (!string.IsNullOrWhiteSpace(error))
            {
                // Log errors to stderr but continue with empty result
                Console.Error.WriteLine(error);
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            // Parse JSON output from child process
            var infos = JsonSerializer.Deserialize<List<PluginClassInfo>>(output, _jsonOptions);
            return infos;
        }
        catch
        {
            // If anything goes wrong with process isolation, return null
            // This allows the caller to fall back to non-isolated scanning
            return null;
        }
    }

    /// <summary>
    /// Stores plugin class info in the cache.
    /// </summary>
    /// <param name="pluginPath">The path to the VST3 plugin.</param>
    /// <param name="pluginClassInfos">List of plugin class info to cache.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pluginPath"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="pluginPath"/> is empty or whitespace</exception>
    /// <exception cref="ArgumentNullException"><paramref name="pluginClassInfos"/> is <see langword="null"/></exception>
    public static void Save(string pluginPath, List<PluginClassInfo> pluginClassInfos)
    {
        ArgumentNullException.ThrowIfNull(pluginPath);
        ArgumentNullException.ThrowIfNull(pluginClassInfos);

        if (string.IsNullOrWhiteSpace(pluginPath))
        {
            throw new ArgumentException("Plugin path cannot be empty or whitespace.", nameof(pluginPath));
        }

        if (pluginClassInfos.Count == 0)
        {
            // Don't cache empty results
            return;
        }

        var cacheFilePath = GetCacheFilePath(pluginPath);
        var pluginFileInfo = new FileInfo(pluginPath);

        var entry = new CacheEntry
        {
            SchemaVersion = CacheSchemaVersion,
            FileSize = pluginFileInfo.Length,
            LastWriteTimeUtc = pluginFileInfo.LastWriteTimeUtc,
            PluginClasses = pluginClassInfos
        };

        var json = JsonSerializer.Serialize(entry, _jsonOptions);
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
