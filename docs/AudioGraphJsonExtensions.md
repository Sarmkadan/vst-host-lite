# AudioGraphJsonExtensions

Provides JSON serialization and deserialization helpers for the `AudioGraph` type, along with a set of required properties that represent the serializable state of an audio node.

## API

### ToJson
```csharp
public static string ToJson(AudioGraph graph)
```
Serializes the supplied `AudioGraph` instance to a JSON string.  
- **Parameters**  
  - `graph`: The `AudioGraph` to serialize. Must not be `null`.  
- **Return value**  
  - A UTF‑8 encoded JSON string representing the graph.  
- **Exceptions**  
  - `ArgumentNullException` if `graph` is `null`.  
  - `JsonSerializationException` if an error occurs during serialization (e.g., unsupported type, circular reference).

### FromJson
```csharp
public static AudioGraph? FromJson(string json)
```
Deserializes a JSON string into an `AudioGraph` instance.  
- **Parameters**  
  - `json`: The JSON text to parse. Must not be `null`.  
- **Return value**  
  - The deserialized `AudioGraph`, or `null` if `json` is `null`, empty, or does not contain a valid graph representation.  
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonException` if the JSON is malformed or missing required fields.

### TryFromJson
```csharp
public static bool TryFromJson(string json, out AudioGraph? graph)
```
Attempts to deserialize a JSON string into an `AudioGraph`, indicating success via the return value.  
- **Parameters**  
  - `json`: The JSON text to parse. Must not be `null`.  
  - `graph`: Receives the deserialized `AudioGraph` when the method returns `true`; otherwise receives `null`.  
- **Return value**  
  - `true` if deserialization succeeded and a non‑null graph was produced; `false` otherwise.  
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.

### Name
```csharp
public required string Name { get; init; }
```
Gets or sets the human‑readable name of the audio component represented by this JSON extension.  
- **Remarks**  
  - This property is *required*; it must be assigned before the instance is used for serialization.  
  - The value is stored as a JSON string property named `"name"`.

### Component
```csharp
public required nint Component { get; init; }
```
Gets or sets the native component handle associated with the audio graph.  
- **Remarks**  
  - This property is *required*; it must be assigned before serialization.  
  - The value is stored as a JSON number (or string, depending on the serializer) representing a platform‑specific pointer.

### NextIndex
```csharp
public int NextIndex { get; init; } = 0;
```
Gets or sets the ordering index used when multiple components are present in the graph.  
- **Remarks**  
  - Defaults to `0` if not explicitly set.  
  - The value is stored as a JSON integer property named `"nextIndex"`.

## Usage

### Serializing an AudioGraph
```csharp
AudioGraph graph = BuildAudioGraph(); // Assume this method creates a populated graph
string json = AudioGraphJsonExtensions.ToJson(graph);
// Persist or transmit the JSON as needed
File.WriteAllText("audiograph.json", json);
```

### Deserializing with error handling
```csharp
string json = File.ReadAllText("audiograph.json");
if (AudioGraphJsonExtensions.TryFromJson(json, out AudioGraph? graph) && graph != null)
{
    // Use the restored graph
    ProcessGraph(graph);
}
else
{
    // Handle invalid or missing data
    Logger.Warning("Failed to load audio graph from JSON.");
}
```

## Notes

- **Edge cases**  
  - Supplying `null` to any of the static methods throws `ArgumentNullException`.  
  - If the JSON lacks the required `"name"` or `"component"` fields, `FromJson` returns `null` and `TryFromJson` returns `false`.  
  - Version mismatches (e.g., future properties added to the graph) are tolerated; unknown properties are ignored during deserialization.  
  - The `NextIndex` property does not affect serialization correctness but may be used by consumers to determine processing order.

- **Thread‑safety**  
  - The static methods (`ToJson`, `FromJson`, `TryFromJson`) are thread‑safe because they operate only on their input parameters and do not mutate shared state.  
  - Instance properties (`Name`, `Component`, `NextIndex`) are not thread‑safe; concurrent read/write access from multiple threads without external synchronization may result in race conditions. It is recommended to treat instances as immutable after initialization or to guard access with appropriate locking mechanisms.
