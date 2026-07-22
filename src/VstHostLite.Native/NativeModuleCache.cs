using System.Collections.Concurrent;

namespace VstHostLite.Native;

/// <summary>
/// Thread-safe cache for NativeModule instances that provides reference counting.
/// Multiple callers can acquire the same module by path, and it will only be
/// loaded once. The module is disposed when the last reference is released.
/// </summary>
public static class NativeModuleCache
{
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private static readonly object _cleanupLock = new();

    /// <summary>
    /// Cache entry that holds a NativeModule and its reference count.
    /// </summary>
    private sealed class CacheEntry : IDisposable
    {
        private readonly object _lock = new();
        private int _refCount;
        private NativeModule? _module;
        private bool _disposed;

        public string Path { get; }
        public NativeModule Module => _module ?? throw new ObjectDisposedException(nameof(CacheEntry));
        public int RefCount => _refCount;

        public CacheEntry(string path, NativeModule module)
        {
            Path = path;
            _module = module;
            _refCount = 1;
        }

        public bool TryAcquire(out NativeModule module)
        {
            lock (_lock)
            {
                if (_disposed || _module == null)
                {
                    module = null!;
                    return false;
                }

                _refCount++;
                module = _module;
                return true;
            }
        }

        public void Release()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _refCount--;

                if (_refCount <= 0)
                {
                    Dispose();
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _module?.Dispose();
                _module = null;
            }
        }
    }

    /// <summary>
    /// Acquires a NativeModule for the given path. If the module is already loaded,
    /// increments the reference count and returns the existing instance. Otherwise,
    /// loads the module and adds it to the cache.
    /// </summary>
    /// <param name="path">Full path to the native module.</param>
    /// <returns>A NativeModule instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the module file does not exist.</exception>
    /// <exception cref="DllNotFoundException">Thrown when the module cannot be loaded.</exception>
    public static NativeModule Acquire(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        // Fast path: try to get existing entry
        if (_cache.TryGetValue(path, out var entry))
        {
            if (entry.TryAcquire(out var module))
            {
                return module;
            }

            // Entry exists but is disposed, remove it and fall through to load
            _cache.TryRemove(path, out _);
        }

        // Slow path: need to load the module
        lock (_cache)
        {
            // Double-check after acquiring lock
            if (_cache.TryGetValue(path, out entry))
            {
                if (entry.TryAcquire(out var module))
                {
                    return module;
                }
                _cache.TryRemove(path, out _);
            }

            var loadedModule = NativeModule.Load(path);
            entry = new CacheEntry(path, loadedModule);
            _cache[path] = entry;
            return loadedModule;
        }
    }

    /// <summary>
    /// Releases a NativeModule acquired via Acquire().
    /// </summary>
    /// <param name="path">Full path to the native module.</param>
    /// <param name="module">The module instance to release.</param>
    public static void Release(string path, NativeModule module)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (_cache.TryGetValue(path, out var entry))
        {
            entry.Release();
        }
        else
        {
            // Module wasn't cached, dispose it directly
            module.Dispose();
        }
    }

    /// <summary>
    /// Gets the current reference count for a cached module.
    /// Returns -1 if the module is not cached.
    /// </summary>
    /// <param name="path">Full path to the native module.</param>
    /// <returns>Reference count, or -1 if not cached.</returns>
    public static int GetRefCount(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (_cache.TryGetValue(path, out var entry))
        {
            lock (entry)
            {
                return ((CacheEntry)entry).RefCount;
            }
        }

        return -1;
    }

    /// <summary>
    /// Clears all cached modules, disposing them and releasing all references.
    /// </summary>
    public static void Clear()
    {
        CacheEntry[] entries;
        lock (_cleanupLock)
        {
            entries = _cache.Values.ToArray();
            _cache.Clear();
        }

        foreach (var entry in entries)
        {
            entry.Dispose();
        }
    }

    /// <summary>
    /// Gets the number of modules currently cached.
    /// </summary>
    public static int Count => _cache.Count;
}
