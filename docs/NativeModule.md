# NativeModule Class

Thin wrapper around a loaded VST3 bundle. On Windows a .vst3 is really a DLL; on Linux/macOS it is a bundle directory with the shared object inside. We only ever got Windows-style single-file modules to load reliably.

## Namespace
`VstHostLite.Native`

## Implements
- `System.IDisposable`
- `System.IEquatable<NativeModule>`

## Public Properties

### Path
```csharp
public string Path { get; }
```
Gets the file system path to the loaded VST3 module.
- **Value**: The absolute or relative path to the VST3 module file.

## Public Static Methods

### Load(string path)
```csharp
public static NativeModule Load(string path)
```
Loads a VST3 module from the specified file path.

#### Parameters
- `path`: The file system path to the VST3 module.

#### Returns
A new `NativeModule` instance representing the loaded module.

#### Exceptions
- `System.ArgumentNullException`: If `path` is null.
- `System.IO.FileNotFoundException`: If the VST3 module file does not exist at the specified path.
- `System.DllNotFoundException`: If the native module could not be loaded.

#### Remarks
This method performs the following steps:
1. Validates the input path.
2. Attempts to load the native library using `NativeLibrary.TryLoad`.
3. Creates a new `NativeModule` instance.
4. Calls the module's entry point (`InitDll` on Windows, `bundleEntry` on macOS, `ModuleEntry` on Linux).
5. Stores the exit delegate for guaranteed cleanup.
6. Returns the initialized module.

## Public Instance Methods

### GetFactory()
```csharp
public nint GetFactory()
```
Gets the VST3 plugin factory interface.

#### Returns
A pointer to the plugin factory.

#### Exceptions
- `System.ObjectDisposedException`: If the module has been disposed.
- `System.EntryPointNotFoundException`: If the `GetPluginFactory` export is not found (indicating the module is not a valid VST3 module).

### Dispose()
```csharp
public void Dispose()
```
Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.

#### Exceptions
- `System.ObjectDisposedException`: If called after the module has been disposed.

#### Remarks
This method:
1. Disposes managed state (if any).
2. Calls the exit delegate (`ExitDll` on Windows, `bundleExit` on macOS, `ModuleExit` on Linux) if the module was entered.
3. Frees the native library handle.
4. Suppresses finalization.

## Overrides

### Equals(NativeModule? other)
```csharp
public bool Equals(NativeModule? other)
```
Determines whether the specified `NativeModule` is equal to the current instance.

#### Parameters
- `other`: The `NativeModule` to compare with the current instance.

#### Returns
`true` if the specified `NativeModule` is equal to the current instance; otherwise, `false`.

#### Remarks
Two `NativeModule` instances are considered equal if their `Path` properties are equal.

### Equals(object? obj)
```csharp
public override bool Equals(object? obj)
```
Determines whether the specified object is equal to the current instance.

#### Parameters
- `obj`: The object to compare with the current instance.

#### Returns
`true` if the specified object is a `NativeModule` and is equal to the current instance; otherwise, `false`.

### GetHashCode()
```csharp
public override int GetHashCode()
```
Serves as the default hash function.

#### Returns
A hash code for the current instance.

## Operators

### operator ==(NativeModule? left, NativeModule? right)
```csharp
public static bool operator ==(NativeModule? left, NativeModule? right)
```
Determines whether two specified `NativeModule` instances are equal.

#### Parameters
- `left`: The first `NativeModule` to compare, or null.
- `right`: The second `NativeModule` to compare, or null.

#### Returns
`true` if the value of `left` is the same as the value of `right`; otherwise, `false`.

### operator !=(NativeModule? left, NativeModule? right)
```csharp
public static bool operator !=(NativeModule? left, NativeModule? right)
```
Determines whether two specified `NativeModule` instances are not equal.

#### Parameters
- `left`: The first `NativeModule` to compare, or null.
- `right`: The second `NativeModule` to compare, or null.

#### Returns
`true` if the value of `left` is different from the value of `right`; otherwise, `false`.

## Thread Safety
This class is not thread-safe. Instances should only be accessed from a single thread.

## Platform Notes
- **Windows**: Uses `InitDll` for entry point and `ExitDll` for exit point.
- **macOS**: Uses `bundleEntry` for entry point and `bundleExit` for exit point.
- **Linux**: Uses `ModuleEntry` for entry point and `ModuleExit` for exit point.