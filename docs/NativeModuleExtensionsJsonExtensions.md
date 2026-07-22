# NativeModuleExtensionsJsonExtensions

Provides JSON serialization and deserialization extensions for the `NativeModule` type, enabling conversion between `NativeModule` instances and their JSON representations. This facilitates persistence, configuration, and interoperability scenarios where native module metadata must be stored or transmitted in a structured format.

## API

### `public static string ToJson(this NativeModule module)`

Converts a `NativeModule` instance to its JSON string representation.

**Parameters:**
- `module` (`NativeModule`): The native module to serialize. Must not be `null`.

**Returns:**
- `string`: A JSON string representing the module's properties (`Path` and any additional metadata encapsulated in `NativeModuleJsonModel`).

**Throws:**
- `ArgumentNullException`: If `module` is `null`.
- `JsonException`: If serialization fails (e.g., due to unsupported types or circular references).

---

### `public static NativeModule? FromJson(string json)`

Deserializes a JSON string into a `NativeModule` instance.

**Parameters:**
- `json` (`string`): The JSON string to deserialize. Must not be `null` or empty.

**Returns:**
- `NativeModule?`: The deserialized `NativeModule` instance, or `null` if deserialization fails.

**Throws:**
- `ArgumentNullException`: If `json` is `null`.
- `JsonException`: If deserialization fails (e.g., due to malformed JSON or missing required fields).

---

### `public static bool TryFromJson(string json, out NativeModule? module)`

Attempts to deserialize a JSON string into a `NativeModule` instance without throwing exceptions.

**Parameters:**
- `json` (`string`): The JSON string to deserialize. Must not be `null` or empty.
- `module` (`out NativeModule?`): Output parameter containing the deserialized `NativeModule` if successful, otherwise `null`.

**Returns:**
- `bool`: `true` if deserialization succeeds; `false` otherwise.

**Throws:**
- `ArgumentNullException`: If `json` is `null`.

---

### `public string? Path { get; }`

Gets the file system path of the native module. This property is part of the `NativeModule` type but is documented here for context, as it is serialized/deserialized by the extension methods.

**Returns:**
- `string?`: The absolute or relative path to the module's binary, or `null` if not set.

---

### `public NativeModuleJsonModel NativeModuleJsonModel { get; }`

Provides access to the underlying JSON model used for serialization. This property is part of the `NativeModule` type but is included for clarity, as it defines the schema for JSON conversion.

**Returns:**
- `NativeModuleJsonModel`: A model containing the module's metadata (e.g., `Path` and other serialized fields).

---

### `public NativeModule ToNativeModule()`

Converts the JSON model (`NativeModuleJsonModel`) back into a `NativeModule` instance. This method is part of the `NativeModule` type but is documented here for context.

**Returns:**
- `NativeModule`: A new `NativeModule` instance populated from the JSON model.

**Throws:**
- `InvalidOperationException`: If the model is in an invalid state (e.g., missing required fields).

## Usage

### Serializing a NativeModule to JSON
