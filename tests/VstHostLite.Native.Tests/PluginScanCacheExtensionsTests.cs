using System;
using System.Reflection;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests
{
    public class PluginScanCacheExtensionsTests
    {
        [Fact]
        public void ScanPluginClasses_NullModule_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                PluginScanCacheExtensions.ScanPluginClasses(null!));
        }

        [Fact]
        public void ScanPluginClasses_WithFilter_NullModule_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                PluginScanCacheExtensions.ScanPluginClasses(null!, "filter", "category"));
        }

        [Fact]
        public void ClearPluginCache_NullModule_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                PluginScanCacheExtensions.ClearPluginCache(null!));
        }

        [Fact]
        public void ClearPluginCache_ValidModule_DoesNotThrow()
        {
            // Arrange: create a NativeModule instance via its non‑public constructor.
            // The constructor signature is (IntPtr handle, string path) in the current codebase.
            var ctor = typeof(NativeModule).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                new[] { typeof(IntPtr), typeof(string) },
                modifiers: null);

            Assert.NotNull(ctor); // Ensure the constructor exists; otherwise the test will fail early.

            var dummyModule = (NativeModule)ctor!.Invoke(new object[] { IntPtr.Zero, "dummy_path" });

            // Act
            var exception = Record.Exception(() => dummyModule.ClearPluginCache());

            // Assert
            Assert.Null(exception);
        }
    }
}
