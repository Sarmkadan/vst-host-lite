# AudioGraphExtensionsJsonExtensions

Provides JSON serialization and deserialization extensions for `AudioGraph` and related types, enabling round-trip persistence and inter-process communication of audio graph structures.

## API

### `ToJson`

Serializes an `AudioGraph` instance to a JSON string.

- **Parameters**
  - `graph` (`AudioGraph`): The graph to serialize.
- **Return value**
  - `string`: A JSON representation of the graph.
- **Exceptions**
  - Throws `ArgumentNullException` if `graph` is `null`.

### `FromJson`

Deserializes a JSON string into an `AudioGraph` instance.

- **Parameters**
  - `json` (`string`): The JSON string to deserialize.
- **Return value**
  - `AudioGraph`: The deserialized graph.
- **Exceptions**
  - Throws `ArgumentNullException` if `json` is `null`.
  - Throws `JsonException` if the JSON is malformed or incompatible with the expected schema.

### `TryFromJson`

Attempts to deserialize a JSON string into an `AudioGraph` instance without throwing exceptions.

- **Parameters**
  - `json` (`string`): The JSON string to deserialize.
  - `graph` (`out AudioGraph`): Receives the deserialized graph on success.
- **Return value**
  - `bool`: `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - None. Errors are reported via the return value.

### `Name` (required)

The name of the audio graph node or component.

- **Type**
  - `string` (required)
- **Remarks**
  - Must be non-empty and unique within the graph context.

### `Component` (required)

A handle or identifier for the underlying audio component.

- **Type**
  - `nint` (required)
- **Remarks**
  - Typically a pointer or handle to a native audio component.

### `NextIndex`

The next available index for new nodes in the graph.

- **Type**
  - `int`
- **Default**
  - `0`

### `Read`

Reads and parses a JSON stream into the current instance.

- **Parameters**
  - `reader` (`Utf8JsonReader*`): The JSON reader.
- **Exceptions**
  - Throws `JsonException` if the JSON is invalid or incompatible.

### `Write`

Writes the current instance to a JSON stream.

- **Parameters**
  - `writer` (`Utf8JsonReader*`): The JSON writer.
- **Exceptions**
  - None. Errors are handled by the JSON writer.

## Usage

### Serializing a graph to JSON
