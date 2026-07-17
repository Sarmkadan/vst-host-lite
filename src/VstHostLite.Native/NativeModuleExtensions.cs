using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace VstHostLite.Native;

/// <summary>
/// Extension methods for <see cref="NativeModule"/> that provide common operations
/// for working with VST3 native modules.
/// </summary>
public static class NativeModuleExtensions
{
    /// <summary>
    /// Gets the file name (without extension) of the native module.
    /// </summary>
    /// <param name="module">The native module instance.</param>
    /// <returns>The file name without extension, or <see langword="null"/> if the path is invalid or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/></exception>
    public static string GetFileNameWithoutExtension(this NativeModule module)
        => Path.GetFileNameWithoutExtension(module?.Path);

    /// <summary>
    /// Gets the directory containing the native module.
    /// </summary>
    /// <param name="module">The native module instance.</param>
    /// <returns>The directory path containing the module, or <see langword="null"/> if the path is invalid or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/></exception>
    public static string GetDirectory(this NativeModule module)
        => Path.GetDirectoryName(module?.Path);

    /// <summary>
    /// Determines whether the native module is a Windows DLL (has .dll extension).
    /// </summary>
    /// <param name="module">The native module instance.</param>
    /// <returns><see langword="true"/> if the module is a Windows DLL; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/></exception>
    public static bool IsWindowsDll(this NativeModule module)
        => string.Equals(Path.GetExtension(module?.Path), ".dll", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the file version information from the native module if available.
    /// </summary>
    /// <param name="module">The native module instance.</param>
    /// <returns>A dictionary containing file version information, or an empty dictionary if version info is not available.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/></exception>
    /// <exception cref="FileNotFoundException">Thrown when the module file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when file version information cannot be read due to access issues.</exception>
    public static IReadOnlyDictionary<string, string> GetFileVersionInfo(this NativeModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(module.Path))
        {
            return result;
        }

        try
        {
            var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(module.Path);

            if (versionInfo.FileVersion is not null)
            {
                result[nameof(versionInfo.FileVersion)] = versionInfo.FileVersion;
            }

            if (versionInfo.ProductVersion is not null)
            {
                result[nameof(versionInfo.ProductVersion)] = versionInfo.ProductVersion;
            }

            if (versionInfo.CompanyName is not null)
            {
                result[nameof(versionInfo.CompanyName)] = versionInfo.CompanyName;
            }

            if (versionInfo.ProductName is not null)
            {
                result[nameof(versionInfo.ProductName)] = versionInfo.ProductName;
            }

            if (versionInfo.FileDescription is not null)
            {
                result[nameof(versionInfo.FileDescription)] = versionInfo.FileDescription;
            }

            if (versionInfo.LegalCopyright is not null)
            {
                result[nameof(versionInfo.LegalCopyright)] = versionInfo.LegalCopyright;
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or UnauthorizedAccessException or IOException or NotSupportedException)
        {
            // Version info not available or not accessible
            throw new InvalidOperationException("Could not read file version information from the module", ex);
        }

        return result;
    }

    /// <summary>
    /// Gets the file size in bytes of the native module.
    /// </summary>
    /// <param name="module">The native module instance.</param>
    /// <returns>The size of the file in bytes.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/></exception>
    /// <exception cref="FileNotFoundException">Thrown when the module file does not exist.</exception>
    public static long GetFileSize(this NativeModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        if (!File.Exists(module.Path))
        {
            throw new FileNotFoundException("Module file not found", module.Path);
        }

        return new FileInfo(module.Path).Length;
    }

    /// <summary>
    /// Gets the last write time of the native module file.
    /// </summary>
    /// <param name="module">The native module instance.</param>
    /// <returns>The last write time of the module file in UTC.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="module"/> is <see langword="null"/></exception>
    /// <exception cref="FileNotFoundException">Thrown when the module file does not exist.</exception>
    public static DateTime LastWriteTimeUtc(this NativeModule module)
    {
        ArgumentNullException.ThrowIfNull(module);

        if (!File.Exists(module.Path))
        {
            throw new FileNotFoundException("Module file not found", module.Path);
        }

        return new FileInfo(module.Path).LastWriteTimeUtc;
    }
}