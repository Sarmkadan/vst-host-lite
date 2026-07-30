using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VstHostLite.Native
{
    /// <summary>
    /// Provides extension methods for handling VST3 bundle paths.
    /// </summary>
    public static class Vst3PathExtensions
    {
        /// <summary>
        /// Determines whether the specified path points to a VST3 bundle directory.
        /// </summary>
        /// <param name="path">The path to examine.</param>
        /// <returns><c>true</c> if the path ends with the <c>.vst3</c> extension (case‑insensitive); otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <c>null</c>.</exception>
        public static bool IsVst3Path(this string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            return Path.GetExtension(path).Equals(".vst3", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Resolves the full path to the platform‑specific binary inside a VST3 bundle.
        /// </summary>
        /// <param name="bundlePath">The path to the VST3 bundle directory (must end with <c>.vst3</c>).</param>
        /// <returns>The absolute path to the binary file inside the bundle.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bundlePath"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="bundlePath"/> does not represent a VST3 bundle.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown when the current OS platform is not supported.</exception>
        public static string ResolveBundleBinaryPath(this string bundlePath)
        {
            ArgumentNullException.ThrowIfNull(bundlePath);

            if (!bundlePath.IsVst3Path())
                throw new ArgumentException("Path does not represent a VST3 bundle directory.", nameof(bundlePath));

            // The plugin name is the bundle folder name without the .vst3 extension.
            var pluginName = bundlePath.GetPluginName();

            string platformFolder;
            string binaryFileName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platformFolder = "x86_64-win";
                binaryFileName = $"{pluginName}.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platformFolder = "x86_64-linux";
                binaryFileName = $"{pluginName}.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platformFolder = "x86_64-mac";
                binaryFileName = $"{pluginName}.vst3";
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS platform for VST3 bundle resolution.");
            }

            // Typical VST3 bundle layout: <bundle>.vst3/Contents/<platformFolder>/<binary>
            return Path.Combine(bundlePath, "Contents", platformFolder, binaryFileName);
        }

        /// <summary>
        /// Gets the plugin name from a VST3 bundle path (the file name without the <c>.vst3</c> extension).
        /// </summary>
        /// <param name="path">The VST3 bundle path.</param>
        /// <returns>The plugin name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <c>null</c>.</exception>
        public static string GetPluginName(this string path)
        {
            ArgumentNullException.ThrowIfNull(path);
            return Path.GetFileNameWithoutExtension(path);
        }
    }
}
