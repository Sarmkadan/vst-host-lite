# Plugin Scan Cache

The `PluginScanCache` provides a persistent cache for VST3 plugin scanning results to improve performance when scanning the same plugin multiple times.

## Features

- **Automatic caching**: Scanned plugin information is automatically cached to disk
- **Cache invalidation**: Cache entries are automatically invalidated when the plugin file is modified
- **Fast lookups**: Subsequent scans of the same plugin use cached data instead of re-scanning
- **Simple API**: Easy-to-use methods for saving, loading, and clearing cache entries

## Usage

### Basic Usage

```csharp
using var module = NativeModule.Load(pluginPath);
var infos = module.ScanPluginClasses(); // Uses cache automatically
```

### Manual Cache Operations

```csharp
// Check if cache exists and is fresh
if (PluginScanCache.TryGetFresh(pluginPath, out var cachedInfo))
{
    // Use cached info
    Console.WriteLine($"Found {cachedInfo.Count} plugin classes in cache");
}

// Save to cache
PluginScanCache.Save(pluginPath, pluginClassInfos);

// Clear cache for specific plugin
PluginScanCache.Clear(pluginPath);

// Clear all caches
PluginScanCache.ClearAll();
```

## Implementation Details

### Cache Format

Cache files are stored as JSON files with the naming convention:
```
<plugin-path>.vst3.cache.json
```

Example:
```json
[
  {
    "cid": "ABCDEF1234567890",
    "category": "Audio Module Class",
    "name": "My Awesome Plugin"
  }
]
```

### Cache Invalidation

The cache automatically invalidates entries when:
1. The plugin file doesn't exist
2. The plugin file's last write time is newer than the cache file
3. The cache file cannot be read or deserialized

### Thread Safety

The cache is designed for single-threaded usage. If multiple threads need to access the cache simultaneously, external synchronization should be used.

## Performance Benefits

- **First scan**: Reads plugin metadata from the VST3 module (slow)
- **Subsequent scans**: Reads from JSON cache file (fast)
- **Cache hit**: ~1-5ms vs ~50-200ms for plugin scan

## Integration Points

The cache integrates with the `NativeModule` class through extension methods:

- `NativeModule.ScanPluginClasses()` - Scans plugin and uses cache
- `NativeModule.ClearPluginCache()` - Clears cache for specific plugin

## Cache File Location

Cache files are stored in the same directory as the assembly:
```
<assembly-location>/<plugin-name>.vst3.cache.json
```

Example:
```
/bin/Debug/net8.0/VST3Plugin1.vst3.cache.json
```

## Error Handling

The cache gracefully handles errors:
- Invalid JSON: Cache is treated as stale
- Missing files: Cache returns false/null
- Permission issues: Cache operations fail silently

This ensures that cache failures never break plugin scanning - they just fall back to fresh scanning.
