# PluginScanCache

Provides caching for VST3 plugin scanning results.

## Purpose

The `PluginScanCache` class caches VST3 plugin scanning results to avoid rescanning plugins on every application start. It caches the plugin path along with the file size, last write time, and list of `PluginClassInfo` entries. The cache is invalidated when the plugin file's modification time or size changes, ensuring that updated plugins are rescanned.

The class supports process-isolated scanning to prevent crashes in third-party VST plugins from affecting the main host process.

## Public API

### Fields

- `CacheFileExtension`: The file extension used for cache files (`.vst3.cache.json`).
- `CacheSchemaVersion`: The current schema version of the cache format (`2`).

### Properties

- `Log`: An optional callback for receiving cache diagnostic messages. Set to a method that accepts a string to enable logging of cache events (hits, misses, errors, saves, clears).

### Methods

#### `TryGetFresh`

```csharp
public static bool TryGetFresh(string? pluginPath, out List<PluginClassInfo>? cachedInfo)
```

Attempts to retrieve cached plugin class information for a given plugin path.

**Parameters**
- `pluginPath`: The path to the VST3 plugin file.
- `cachedInfo`: Output parameter containing the cached plugin class information if valid cache exists; otherwise `null`.

**Returns**
- `true` if valid cached data exists and is fresh; `false` if cache is missing, stale, or an error occurred.

**Exceptions**
- `ArgumentNullException`: If `pluginPath` is `null`.
- `ArgumentException`: If `pluginPath` is empty or whitespace.

**Behavior**
- Returns `false` if the plugin file does not exist.
- Returns `false` if the cache file does not exist.
- Deserializes the cache entry and validates the schema version.
- Compares the cached file size and last write time with the current plugin file's attributes.
- If the file has changed, the cache is deleted and the method returns `false`.
- On success, sets `cachedInfo` to the cached list of `PluginClassInfo` and returns `true`.
- Logs cache hit/miss/error events via the `Log` callback if set.

#### `ScanWithIsolation`

```csharp
public static List<PluginClassInfo>? ScanWithIsolation(string? pluginPath)
```

Scans a plugin using a child process for crash isolation.

**Parameters**
- `pluginPath`: The path to the VST3 plugin file.

**Returns**
- A list of `PluginClassInfo` objects if scanning succeeded; `null` if scanning failed or the plugin file does not exist.

**Exceptions**
- `ArgumentNullException`: If `pluginPath` is `null`.
- `ArgumentException`: If `pluginPath` is empty or whitespace.

**Behavior**
- Returns `null` if the plugin file does not exist.
- Launches the current process with a scan command argument and the plugin path in a child process.
- Uses a 10-second timeout to prevent hanging.
- If the child process exits with a non-zero code, returns `null` (indicating failure, e.g., plugin crash).
- Captures standard output and error; non-empty error is written to the console's error stream.
- Deserializes the JSON output from the child process into a list of `PluginClassInfo`.
- Returns `null` if any exception occurs during process isolation, allowing the caller to fall back to non-isolated scanning.

#### `Save`

```csharp
public static void Save(string pluginPath, List<PluginClassInfo> pluginClassInfos)
```

Stores plugin class information in the cache.

**Parameters**
- `pluginPath`: The path to the VST3 plugin file.
- `pluginClassInfos`: The list of plugin class information to cache.

**Exceptions**
- `ArgumentNullException`: If `pluginPath` or `pluginClassInfos` is `null`.
- `ArgumentException`: If `pluginPath` is empty or whitespace.

**Behavior**
- Does nothing if `pluginClassInfos` is empty (avoids caching empty results).
- Creates a cache entry containing the current schema version, plugin file size, last write time (UTC), and the plugin class information.
- Serializes the entry to JSON and writes it to the cache file (plugin path + `CacheFileExtension`).
- Logs a cache save event via the `Log` callback if set.

#### `Clear`

```csharp
public static void Clear(string pluginPath)
```

Clears the cache for a specific plugin.

**Parameters**
- `pluginPath`: The path to the VST3 plugin file.

**Behavior**
- Deletes the cache file associated with the plugin path if it exists.
- Logs a cache clear event via the `Log` callback if set.

#### `ClearAll`

```csharp
public static void ClearAll()
```

Clears all plugin scan caches in the directory containing the `PluginScanCache` assembly.

**Behavior**
- Enumerates all files with the `CacheFileExtension` in the assembly's directory.
- Attempts to delete each cache file, ignoring any errors during deletion.
- No logging is performed for individual file deletions in this method.

## Cache Format

The cache is stored as a JSON file with the following structure:

```jsonc
{
  "schemaVersion": 2,
  "fileSize": 123456,
  "lastWriteTimeUtc": "2026-09-14T12:34:56.789Z",
  "pluginClasses": [
    {
      // PluginClassInfo properties (see PluginClassInfo documentation)
    }
  ]
}
```

- `schemaVersion`: Must match `CacheSchemaVersion` (currently 2) for the cache to be considered valid.
- `fileSize`: The size of the plugin file in bytes at the time of caching.
- `lastWriteTimeUtc`: The last write time of the plugin file in UTC at the time of caching.
- `pluginClasses`: An array of `PluginClassInfo` objects representing the discovered plugin classes.

## Process-Isolated Scanning

The `ScanWithIsolation` method launches a child process to perform the actual plugin scanning. This isolates the host process from crashes that may occur in poorly behaved VST plugins during the scanning process.

The child process is invoked with the same executable as the host process, passing a scan command argument and the plugin path. The child process outputs the scan results as JSON to standard output, which the parent process then deserializes.

If the child process hangs (exceeds 10 seconds), it is terminated and the scan is considered failed.

## Thread Safety

The static methods of `PluginScanCache` are thread-safe for concurrent calls to different plugin paths. However, concurrent operations on the exact same plugin path may result in race conditions (e.g., one thread reading while another writes). In practice, such conflicts are rare and typically resolve themselves on the next scan attempt.

## Logging

When the `Log` property is set to a delegate, the class logs the following events:

- `event=cache_hit`: Successful cache retrieval.
- `event=cache_miss`: Cache miss with reason (plugin file missing, cache file missing, stale).
- `event=cache_error`: Error reading or deserializing cache.
- `event=cache_save`: Cache entry saved.
- `event=cache_clear`: Cache cleared for a specific plugin.

Log messages are formatted as key-value pairs for easy parsing, with values escaped to prevent injection of extra spaces or quotes.