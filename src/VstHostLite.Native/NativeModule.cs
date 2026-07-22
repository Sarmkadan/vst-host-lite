using System.Runtime.InteropServices;

namespace VstHostLite.Native;

/// <summary>
/// Thin wrapper around a loaded VST3 bundle. On Windows a .vst3 is really a
/// DLL; on Linux/macOS it is a bundle directory with the shared object inside.
/// We only ever got Windows-style single-file modules to load reliably.
/// </summary>
public sealed class NativeModule : IDisposable
{
    private nint _handle;
    private bool _entered;
    private bool _disposed;
    private ExitDelegate? _exitDelegate;

    public string Path { get; }

    private NativeModule(string path, nint handle, ExitDelegate? exitDelegate)
    {
        Path = path;
        _handle = handle;
        _exitDelegate = exitDelegate;
    }

    public static NativeModule Load(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!File.Exists(path))
            throw new FileNotFoundException("VST3 module not found", path);

        if (!NativeLibrary.TryLoad(path, out var handle))
            throw new DllNotFoundException($"could not load native module: {path}");

        ExitDelegate? exitDelegate = null;
        var module = new NativeModule(path, handle, exitDelegate);
        module.Enter();
        return module;
    }

    // VST3 requires the module entry point to be called once before any
    // factory calls, and the exit point on unload. Names differ per platform.
    private void Enter()
    {
        var entry = OperatingSystem.IsWindows() ? "InitDll"
            : OperatingSystem.IsMacOS() ? "bundleEntry"
            : "ModuleEntry";

        if (NativeLibrary.TryGetExport(_handle, entry, out var fn))
        {
            var init = Marshal.GetDelegateForFunctionPointer<EntryDelegate>(fn);
            // On Linux ModuleEntry expects the bundle handle; passing the
            // module handle is what the SDK reference host does too.
            _entered = init(_handle);
        }
        else
        {
            // Some older modules have no explicit entry point.
            _entered = true;
        }

        // Store the exit delegate for guaranteed cleanup
        StoreExitDelegate();
    }

    private void StoreExitDelegate()
    {
        var exit = OperatingSystem.IsWindows() ? "ExitDll"
            : OperatingSystem.IsMacOS() ? "bundleExit"
            : "ModuleExit";

        if (NativeLibrary.TryGetExport(_handle, exit, out var fn))
        {
            _exitDelegate = Marshal.GetDelegateForFunctionPointer<ExitDelegate>(fn);
        }
    }

    /// <summary>
    /// Gets the VST3 plugin factory interface.
    /// </summary>
    /// <returns>A pointer to the plugin factory.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the module has been disposed.</exception>
    public nint GetFactory()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!NativeLibrary.TryGetExport(_handle, "GetPluginFactory", out var fn))
            throw new EntryPointNotFoundException("GetPluginFactory missing - not a valid VST3 module");

        var getFactory = Marshal.GetDelegateForFunctionPointer<GetFactoryDelegate>(fn);
        return getFactory();
    }

    /// <summary>
    /// Finalizer to ensure the native module is properly disposed if the object is not explicitly disposed.
    /// </summary>
    ~NativeModule()
    {
        Dispose(false);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if called after the module has been disposed.</exception>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Dispose managed state (managed objects referenced from fields)
        }

        // Always call exit delegate if we entered the module
        if (_entered && _exitDelegate != null)
        {
            _exitDelegate();
        }

        NativeLibrary.Free(_handle);
        _handle = 0;
        _disposed = true;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool EntryDelegate(nint handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ExitDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint GetFactoryDelegate();
}