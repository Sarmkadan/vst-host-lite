namespace VstHostLite.Native;

/// <summary>
/// Extension methods for scanning VST3 plugins with caching support.
/// </summary>
public static class PluginScanCacheExtensions
{
    /// <summary>
    /// Scans a VST3 plugin and returns its class information, using cache when available.
    /// </summary>
    /// <param name="module">The loaded native module.</param>
    /// <returns>List of plugin class information.</returns>
    public static List<PluginClassInfo> ScanPluginClasses(this NativeModule module)
    {
        // Try to get cached info first
        if (PluginScanCache.TryGetFresh(module.Path, out var cachedInfo))
        {
            return cachedInfo!;
        }

        // Scan fresh if cache is invalid or doesn't exist
        var factory = module.GetFactory();
        var count = Vst3Interop.CountClasses(factory);
        var infos = new List<PluginClassInfo>(count);

        for (var i = 0; i < count; i++)
        {
            var info = Vst3Interop.GetClassInfo(factory, i);
            infos.Add(info);
        }

        // Save to cache for future use
        PluginScanCache.Save(module.Path, infos);

        return infos;
    }

    /// <summary>
    /// Clears the cache for a specific plugin.
    /// </summary>
    /// <param name="module">The loaded native module.</param>
    public static void ClearPluginCache(this NativeModule module)
    {
        PluginScanCache.Clear(module.Path);
    }
}
