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
    /// Scans a VST3 plugin and returns its class information with filtering support, using cache when available.
    /// </summary>
    /// <param name="module">The loaded native module.</param>
    /// <param name="filter">Substring to filter by plugin name (case-insensitive).</param>
    /// <param name="category">Exact category to filter by (case-insensitive).</param>
    /// <returns>List of plugin class information matching filters.</returns>
    public static List<PluginClassInfo> ScanPluginClasses(this NativeModule module, string? filter, string? category)
    {
        var infos = ScanPluginClasses(module);
        return ApplyFilters(infos, filter, category);
    }

    /// <summary>
    /// Clears the cache for a specific plugin.
    /// </summary>
    /// <param name="module">The loaded native module.</param>
    public static void ClearPluginCache(this NativeModule module)
    {
        PluginScanCache.Clear(module.Path);
    }

    /// <summary>
    /// Applies filters to plugin class information.
    /// </summary>
    /// <param name="infos">List of plugin class information.</param>
    /// <param name="filter">Substring to filter by plugin name (case-insensitive).</param>
    /// <param name="category">Exact category to filter by (case-insensitive).</param>
    /// <returns>Filtered list of plugin class information.</returns>
    private static List<PluginClassInfo> ApplyFilters(List<PluginClassInfo> infos, string? filter, string? category)
    {
        if (filter == null && category == null)
        {
            return infos;
        }

        var result = new List<PluginClassInfo>();

        foreach (var info in infos)
        {
            bool matches = true;

            if (filter != null)
            {
                // Case-insensitive substring match on name
                matches &= info.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (category != null)
            {
                // Exact case-insensitive match on category
                matches &= string.Equals(info.Category, category, StringComparison.OrdinalIgnoreCase);
            }

            if (matches)
            {
                result.Add(info);
            }
        }

        return result;
    }
}
