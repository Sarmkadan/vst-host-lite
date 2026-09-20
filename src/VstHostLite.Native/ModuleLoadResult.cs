using System;

namespace VstHostLite.Native;

/// <summary>
/// Result of attempting to load a native module.
/// </summary>
/// <param name="Success">Whether the module was loaded successfully.</param>
/// <param name="Module">The loaded module, if successful; otherwise null.</param>
/// <param name="ErrorMessage">Error message if loading failed.</param>
public record ModuleLoadResult(bool Success, NativeModule? Module, string? ErrorMessage);