using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VstHostLite.Native;

/// <summary>
/// VST3 is a COM-like ABI: every interface is a vtable of function pointers and
/// objects are reference counted through IUnknown-style AddRef/Release. On .NET 10
/// we walk the vtable manually because the interfaces are C++ and there is no
/// stable C surface. This is only the bits we needed to enumerate a factory.
/// </summary>
public static class Vst3Interop
{
    // VST3 result codes
    public const int kResultOk = 0;
    public const int kResultFalse = 1;
    public const int kInvalidArgument = 2;
    public const int kNotImplemented = 3;
    public const int kInternalError = 4;
    public const int kNotValid = 5;
    public const int kResultTrue = -1; // Typically -1 for success in VST3

    // IPluginFactory::countClasses is at vtable slot 3 (after queryInterface,
    // addRef, release). Each entry is a function pointer we read by offset.
    private const int SlotCountClasses = 3;
    private const int SlotGetClassInfo = 4;

    /// <summary>
    /// Throws an appropriate exception based on the VST3 result code.
    /// </summary>
    /// <param name="result">The VST3 result code to check.</param>
    /// <param name="context">Optional context string for error message.</param>
    /// <exception cref="InvalidOperationException">Thrown when the result indicates failure.</exception>
    public static void ThrowIfFailed(int result, string? context = null)
    {
        if (result == kResultOk || result == kResultTrue)
            return;

        string message = context == null
            ? $"VST3 operation failed with result code: 0x{result:X8}"
            : $"VST3 operation failed with result code 0x{result:X8} ({context}) - ";

        message += result switch
        {
            kResultFalse => "Operation returned false/kResultFalse",
            kInvalidArgument => "Invalid argument provided",
            kNotImplemented => "Not implemented",
            kInternalError => "Internal error occurred",
            kNotValid => "Object or state not valid for operation",
            _ => $"Unknown error code: 0x{result:X8}"
        };

        throw new InvalidOperationException(message);
    }

    /// <summary>
    /// A SafeHandle-style wrapper for COM interface pointers that properly manages
    /// IUnknown reference counting through AddRef/Release calls.
    /// </summary>
    /// <typeparam name="T">The interface type.</typeparam>
    public readonly struct ComPtr<T> : IDisposable where T : unmanaged
    {
        private readonly nint _pointer;

        /// <summary>
        /// Gets the raw interface pointer. Use with caution - the pointer
        /// is only valid while the ComPtr is in scope.
        /// </summary>
        public nint Pointer => _pointer;

        /// <summary>
        /// Gets whether the pointer is null (not a valid interface pointer).
        /// </summary>
        public bool IsNull => _pointer == 0;

        /// <summary>
        /// Initializes a new <see cref="ComPtr{T}"/> wrapping an existing COM interface pointer.
        /// </summary>
        /// <param name="pointer">The raw COM interface pointer.</param>
        /// <exception cref="ArgumentException">Thrown if the pointer is not aligned on a pointer boundary.</exception>
        public ComPtr(nint pointer)
        {
            if (pointer != 0 && (pointer & (nint.Size - 1)) != 0)
                throw new ArgumentException("COM interface pointer must be aligned on a pointer boundary", nameof(pointer));

            _pointer = pointer;
        }

        /// <summary>
        /// Releases the COM interface pointer by calling Release() on the interface.
        /// </summary>
        public void Dispose()
        {
            if (_pointer != 0)
            {
                try
                {
                    // Call Release() on the interface (vtable slot 2)
                    var vtable = Marshal.ReadIntPtr(_pointer);
                    var releaseSlot = Marshal.ReadIntPtr(vtable, 2 * nint.Size);
                    var release = Marshal.GetDelegateForFunctionPointer<ReleaseDelegate>(releaseSlot);
                    release(_pointer);
                }
                catch
                {
                    // Best effort - don't let exceptions escape Dispose
                }
            }
        }

        /// <summary>
        /// Invokes a method on the COM interface through the vtable.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type for the method.</typeparam>
        /// <typeparam name="TResult">The return type of the method.</typeparam>
        /// <param name="slot">The vtable slot index.</param>
        /// <param name="args">The arguments to pass.</param>
        /// <returns>The result of the method call.</returns>
        public TResult Invoke<TDelegate, TResult>(int slot, params object?[] args) where TDelegate : Delegate
        {
            if (IsNull)
                throw new InvalidOperationException("Cannot invoke method on null COM interface pointer");

            var vtable = Marshal.ReadIntPtr(_pointer);
            var methodPtr = Marshal.ReadIntPtr(vtable, slot * nint.Size);
            var method = Marshal.GetDelegateForFunctionPointer<TDelegate>(methodPtr);
            return method.DynamicInvoke(args) is TResult result ? result : throw new InvalidOperationException("Method returned null or wrong type");
        }

        /// <summary>
        /// Casts this ComPtr to another interface type by calling QueryInterface.
        /// </summary>
        /// <typeparam name="TOther">The target interface type.</typeparam>
        /// <returns>A new ComPtr wrapping the queried interface.</returns>
        public ComPtr<TOther> As<TOther>() where TOther : unmanaged
        {
            if (IsNull)
                return new ComPtr<TOther>(0);

            // Call QueryInterface (vtable slot 0)
            var vtable = Marshal.ReadIntPtr(_pointer);
            var queryInterfaceSlot = Marshal.ReadIntPtr(vtable, 0 * nint.Size);
            var queryInterface = Marshal.GetDelegateForFunctionPointer<QueryInterfaceDelegate>(queryInterfaceSlot);

            var iid = typeof(TOther).GUID;
            var result = queryInterface(_pointer, ref iid, out var otherPtr);
            ThrowIfFailed(result, $"QueryInterface for {typeof(TOther).Name}");

            return new ComPtr<TOther>(otherPtr);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int QueryInterfaceDelegate(nint self, ref Guid iid, out nint ptr);

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int AddRefDelegate(nint self);

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int ReleaseDelegate(nint self);

    public static int CountClasses(nint factory)
    {
        if (factory == 0)
            return 0;

        try
        {
            var vtable = Marshal.ReadIntPtr(factory);
            var slot = Marshal.ReadIntPtr(vtable, SlotCountClasses * nint.Size);
            var count = Marshal.GetDelegateForFunctionPointer<CountDelegate>(slot);
            return count(factory);
        }
        catch
        {
            throw;
        }
    }

    public static PluginClassInfo GetClassInfo(nint factory, int index)
    {
        try
        {
            var vtable = Marshal.ReadIntPtr(factory);
            var slot = Marshal.ReadIntPtr(vtable, SlotGetClassInfo * nint.Size);
            var getInfo = Marshal.GetDelegateForFunctionPointer<GetClassInfoDelegate>(slot);

            var raw = new PClassInfoRaw();
            var hr = getInfo(factory, index, ref raw);
            ThrowIfFailed(hr, $"getClassInfo({index})");

            return new PluginClassInfo(
                Cid: Convert.ToHexString(raw.Cid),
                Category: FromAscii(raw.Category),
                Name: FromAscii(raw.Name));
        }
        catch
        {
            throw;
        }
    }

    /// <summary>
    /// Filters a collection of <see cref="PluginClassInfo"/> objects according to
    /// the provided name substring (<paramref name="filter"/>) and/or exact
    /// category (<paramref name="category"/>). Matching is case‑insensitive.
    /// </summary>
    /// <param name="infos">The source collection of plugin class infos.</param>
    /// <param name="filter">
    /// Optional case‑insensitive substring to match against <see cref="PluginClassInfo.Name"/>.
    /// </param>
    /// <param name="category">
    /// Optional case‑insensitive exact match to compare with <see cref="PluginClassInfo.Category"/>.
    /// </param>
    /// <returns>A list containing only the items that satisfy the filter criteria.</returns>
    public static List<PluginClassInfo> FilterPluginClasses(
        IEnumerable<PluginClassInfo> infos,
        string? filter,
        string? category)
    {
        // If no filters are supplied, return the full list.
        if (filter == null && category == null)
        {
            return new List<PluginClassInfo>(infos);
        }

        var result = new List<PluginClassInfo>();

        foreach (var info in infos)
        {
            bool matches = true;

            if (filter != null)
            {
                // Case‑insensitive substring match on the plugin name.
                matches &= info.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (category != null)
            {
                // Exact case-insensitive match on the category.
                matches &= string.Equals(info.Category, category, StringComparison.OrdinalIgnoreCase);
            }

            if (matches)
            {
                result.Add(info);
            }
        }

        return result;
    }

    private static string FromAscii(byte[] buffer)
    {
        var end = Array.IndexOf(buffer, (byte)0);
        return System.Text.Encoding.ASCII.GetString(buffer, 0, end < 0 ? buffer.Length : end);
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int CountDelegate(nint self);

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    private delegate int GetClassInfoDelegate(nint self, int index, ref PClassInfoRaw info);

    // Matches Steinberg::PClassInfo layout.
    [StructLayout(LayoutKind.Sequential)]
    private struct PClassInfoRaw
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Cid;
        public int Cardinality;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Category;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public byte[] Name;
    }
}

public readonly record struct PluginClassInfo(string Cid, string Category, string Name);