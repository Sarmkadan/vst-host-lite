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

    public string Path { get; }

    private NativeModule(string path, nint handle)
    {
        Path = path;
        _handle = handle;
    }

    public static NativeModule Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("VST3 module not found", path);

        if (!NativeLibrary.TryLoad(path, out var handle))
            throw new DllNotFoundException($"could not load native module: {path}");

        var module = new NativeModule(path, handle);
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
    }

    public nint GetFactory()
    {
        if (!NativeLibrary.TryGetExport(_handle, "GetPluginFactory", out var fn))
            throw new EntryPointNotFoundException("GetPluginFactory missing - not a valid VST3 module");

        var getFactory = Marshal.GetDelegateForFunctionPointer<GetFactoryDelegate>(fn);
        return getFactory();
    }

    public void Dispose()
    {
        if (_handle == 0)
            return;

        if (_entered)
        {
            var exit = OperatingSystem.IsWindows() ? "ExitDll"
                     : OperatingSystem.IsMacOS() ? "bundleExit"
                     : "ModuleExit";
            if (NativeLibrary.TryGetExport(_handle, exit, out var fn))
            {
                var exitFn = Marshal.GetDelegateForFunctionPointer<ExitDelegate>(fn);
                exitFn();
            }
        }

        NativeLibrary.Free(_handle);
        _handle = 0;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool EntryDelegate(nint handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ExitDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint GetFactoryDelegate();
}
