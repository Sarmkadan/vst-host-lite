using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace VstHostLite.Native;

/// <summary>
/// VST3 is a COM-like ABI: every interface is a vtable of function pointers and
/// objects are reference counted through IUnknown-style AddRef/Release. On .NET
/// we walk the vtable manually because the interfaces are C++ and there is no
/// stable C surface. This is only the bits we needed to enumerate a factory.
/// </summary>
public static class Vst3Interop
{
    // IPluginFactory::countClasses is at vtable slot 3 (after queryInterface,
    // addRef, release). Each entry is a function pointer we read by offset.
    private const int SlotCountClasses = 3;
    private const int SlotGetClassInfo = 4;

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
            if (hr != 0)
                throw new InvalidOperationException($"getClassInfo({index}) failed: 0x{hr:X8}");

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
                // Exact case‑insensitive match on the category.
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