# NativeModuleCache

The `NativeModuleCache` class provides a thread-safe mechanism for managing the lifecycle of native VST3 modules within the `vst-host-lite` runtime. It acts as a reference-counted registry that prevents premature unloading of shared native libraries while allowing multiple consumers to safely acquire and release module handles. By tracking acquisition counts per file path, this cache ensures that native resources are only unloaded from memory when no active references remain, mitigating common crash scenarios associated with dynamic library management in audio plugin hosting.

## API

### `Path`
```csharp
public string Path { get; }
```
Gets the absolute file system path to the native module (e.g., the `.vst3` bundle or `.dll`/`.so` file) associated with this cache instance. This property is read-only and is established upon the creation of the cache entry.

### `CacheEntry`
```csharp
public CacheEntry { get; }
```
Retrieves the underlying `CacheEntry` object that holds the internal state for this specific module. This includes the loaded `NativeModule` instance and the current reference count logic. Access is provided for inspection or advanced integration scenarios where direct access to the cached handle is required without invoking the acquisition methods.

### `TryAcquire`
```csharp
public bool TryAcquire(out NativeModule module)
```
Attempts to increment the reference count and retrieve a valid `NativeModule` handle.
*   **Parameters**: None.
*   **Returns**: `true` if the module was successfully acquired and the `module` output parameter is populated; `false` if the cache has been disposed or the underlying module failed to load.
*   **Throws**: Does not throw exceptions; failures are indicated by the boolean return value.

### `Release`
```csharp
public void Release()
```
Decrements the reference count for the module associated with this instance. If the reference count reaches zero, the underlying native module is unloaded from memory and resources are freed.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: May throw an `ObjectDisposedException` if called after the cache instance itself has been disposed.

### `Dispose`
```csharp
public void Dispose()
```
Releases all resources managed by this `NativeModuleCache` instance. This method forcibly decrements the reference count (potentially unloading the module if this was the last reference) and marks the instance as unusable for future acquisitions.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: Implements standard `IDisposable` behavior; should be safe to call multiple times, though subsequent calls to other members will likely throw `ObjectDisposedException`.

### `Acquire` (Static)
```csharp
public static NativeModule Acquire(string path)
```
Static factory method to locate or create a cache entry for the specified path and immediately acquire a reference.
*   **Parameters**: `path` (string) – The absolute path to the native module.
*   **Returns**: A valid `NativeModule` handle.
*   **Throws**: Throws `FileNotFoundException` or `BadImageFormatException` (or similar loading errors) if the module cannot be loaded. Throws `ArgumentException` if the path is invalid.

### `Release` (Static)
```csharp
public static void Release(string path)
```
Static helper to decrement the global reference count for a module identified by its path. This is typically used when the caller does not hold an instance of `NativeModuleCache` but needs to balance a previous static `Acquire` call.
*   **Parameters**: `path` (string) – The absolute path to the native module.
*   **Returns**: `void`.
*   **Throws**: May throw if the path is not found in the global cache or if internal state is corrupted.

### `GetRefCount` (Static)
```csharp
public static int GetRefCount(string path)
```
Retrieves the current number of active references held for the module at the specified path.
*   **Parameters**: `path` (string) – The absolute path to the native module.
*   **Returns**: An integer representing the current reference count. Returns 0 if the path is not currently cached.
*   **Throws**: Generally does not throw; returns 0 for unknown paths.

### `Clear` (Static)
```csharp
public static void Clear()
```
Forcibly clears the entire global cache, releasing all loaded native modules regardless of their current reference counts.
*   **Parameters**: None.
*   **Returns**: `void`.
*   **Throws**: May cause instability if called while plugins are actively processing audio, as it unloads modules currently in use. Use with extreme caution, primarily intended for testing or application shutdown sequences.

## Usage

### Example 1: Standard Plugin Loading Lifecycle
This example demonstrates the recommended pattern for loading a VST3 plugin, using the instance-based `TryAcquire` and `Release` methods to ensure proper reference counting.

```csharp
using VstHostLite;

public class PluginLoader
{
    public void LoadAndRun(string pluginPath)
    {
        // Create a cache instance for the specific plugin path
        using var cache = new NativeModuleCache(pluginPath);

        // Attempt to acquire the native module
        if (cache.TryAcquire(out var module))
        {
            try
            {
                // Use the module to create a plugin factory or instance
                var factory = module.CreateFactory();
                // ... perform audio processing or scanning ...
            }
            finally
            {
                // Always release the reference when done
                cache.Release();
            }
        }
        else
        {
            Console.WriteLine($"Failed to acquire module at {pluginPath}");
        }
        
        // The 'using' statement calls cache.Dispose(), ensuring cleanup
    }
}
```

### Example 2: Global Reference Inspection and Static Acquisition
This example illustrates using the static API to check if a module is already loaded elsewhere in the application and acquiring a handle without maintaining a long-lived `NativeModuleCache` instance.

```csharp
using VstHostLite;

public class ModuleInspector
{
    public void CheckAndLoad(string pluginPath)
    {
        // Check how many references currently exist for this path
        int currentRefs = NativeModuleCache.GetRefCount(pluginPath);
        Console.WriteLine($"Current references for {pluginPath}: {currentRefs}");

        if (currentRefs == 0)
        {
            Console.WriteLine("Module not loaded. Acquiring new instance.");
        }

        // Statically acquire the module (increments global ref count)
        NativeModule module = NativeModuleCache.Acquire(pluginPath);

        try
        {
            // Perform operations...
            ValidateModule(module);
        }
        finally
        {
            // Must balance the static Acquire with a static Release
            NativeModuleCache.Release(pluginPath);
        }
    }

    private void ValidateModule(NativeModule module) 
    {
        // Validation logic here
    }
}
```

## Notes

*   **Thread Safety**: The `NativeModuleCache` is designed to be thread-safe. Static methods (`Acquire`, `Release`, `GetRefCount`, `Clear`) utilize internal locking to manage the global dictionary of cached modules. Instance methods (`TryAcquire`, `Release`) are safe to call from multiple threads, provided the instance itself has not been disposed.
*   **Reference Counting Integrity**: Every successful call to `Acquire` (static or via `TryAcquire`) must be matched by a corresponding `Release` call. Failing to release references will result in native modules remaining loaded in memory indefinitely, potentially causing file lock issues on Windows or memory leaks.
*   **Disposal Behavior**: Calling `Dispose()` on a `NativeModuleCache` instance effectively acts as a `Release()` call for the reference held by that instance. Once disposed, the instance cannot be reused; calling `TryAcquire` on a disposed instance will return `false`.
*   **Clear Risks**: The static `Clear()` method bypasses reference counting checks. Invoking this while any part of the application holds an active `NativeModule` handle will likely result in access violations or crashes when those handles are subsequently used. This method should strictly be reserved for application teardown or isolated test environments.
*   **Path Normalization**: The cache uses the provided string path as a key. Ensure that paths are normalized (e.g., consistent casing on case-insensitive file systems, resolved absolute paths) before passing them to `Acquire` or `GetRefCount` to avoid duplicate cache entries for the same physical file.
