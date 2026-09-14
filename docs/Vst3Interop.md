# Vst3Interop

Provides core interoperability functionality for working with VST3 plugins. This class implements the necessary COM-style interop to enumerate VST3 plugin factories, manage interface reference counting, and handle error conditions.

VST3 uses a COM-like ABI where interfaces are represented as vtables (virtual function tables) and objects are reference counted through IUnknown-style AddRef/Release methods. Since VST3 interfaces are implemented in C++ and lack a stable C surface area, this class manually walks the vtable to access functionality.

This class does NOT:
- Create or manage actual VST3 plugin instances
- Handle audio processing or effect execution
- Provide direct access to VST3 processor interfaces

It focuses solely on factory enumeration and interface querying capabilities required to discover and inspect available VST3 plugins.

## Constants

| Name | Value | Description |
|------|-------|-------------|
| `kResultOk` | 0 | VST3 result code indicating a successful operation |
| `kResultFalse` | 1 | VST3 result code indicating a logical false result |
| `kInvalidArgument` | 2 | VST3 result code indicating an invalid argument was provided |
| `kNotImplemented` | 3 | VST3 result code indicating the requested operation is not implemented |
| `kInternalError` | 4 | VST3 result code indicating an internal error occurred |
| `kNotValid` | 5 | VST3 result code indicating the object or state is not valid for the operation |
| `kResultTrue` | -1 | VST3 result code indicating a logical true result |

## Methods

### `ThrowIfFailed(int result, string? context = null)`

Throws an appropriate exception based on the VST3 result code.

**Parameters:**
- `result`: The VST3 result code to check.
- `context`: Optional context string for error message.

**Exceptions:**
- `InvalidOperationException`: Thrown when the result indicates failure.

### `ComPtr<T>`

A SafeHandle-style wrapper for COM interface pointers that properly manages IUnknown reference counting through AddRef/Release calls.

**Type Parameters:**
- `T`: The interface type (must be unmanaged).

**Properties:**
- `Pointer`: Gets the raw interface pointer. Use with caution - the pointer is only valid while the ComPtr is in scope.
- `IsNull`: Gets whether the pointer is null (not a valid interface pointer).

**Methods:**
- `Dispose()`: Releases the COM interface pointer by calling Release() on the interface.
- `Invoke<TDelegate, TResult>(int slot, params object?[] args)`: Invokes a method on the COM interface through the vtable.
- `As<TOther>()`: Casts this ComPtr to another interface type by calling QueryInterface.

### `CountClasses(nint factory)`

Retrieves the number of classes supported by a VST3 factory.

**Parameters:**
- `factory`: Pointer to the VST3 factory instance.

**Returns:**
- The number of classes supported by the factory.

### `GetClassInfo(nint factory, int index)`

Retrieves information about a specific class in a VST3 factory.

**Parameters:**
- `factory`: Pointer to the VST3 factory instance.
- `index`: Zero-based index of the class to retrieve information for.

**Returns:**
- A `PluginClassInfo` object containing the class ID, category, and name.

### `FilterPluginClasses(IEnumerable<PluginClassInfo> infos, string? filter, string? category)`

Filters a collection of `PluginClassInfo` objects according to the provided name substring and/or exact category.

**Parameters:**
- `infos`: The source collection of plugin class infos.
- `filter`: Optional case-insensitive substring to match against `PluginClassInfo.Name`.
- `category`: Optional case-insensitive exact match to compare with `PluginClassInfo.Category`.

**Returns:**
- A list containing only the items that satisfy the filter criteria.

## PluginClassInfo

`public readonly record struct PluginClassInfo(string Cid, string Category, string Name)`

Represents information about a VST3 plugin class.

**Properties:**
- `Cid`: The class ID as a hexadecimal string.
- `Category`: The category of the plugin (e.g., "Audio Effect", "Instrument").
- `Name`: The human-readable name of the plugin.

## Equality

All instances of `Vst3Interop` are considered equal because the class has no instance state. The class implements `IEquatable<Vst3Interop>` with standard equality members (Equals, GetHashCode, ==, != operators).