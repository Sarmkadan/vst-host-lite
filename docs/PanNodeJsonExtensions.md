# PanNodeJsonExtensions

The `PanNodeJsonExtensions` class provides JSON serialization and deserialization support for `PanNode` objects. It exposes static methods to convert a `PanNode` to and from a JSON string, as well as instance properties that mirror the fields of a `PanNode` and a method to materialize the underlying `PanNode` instance. This class is intended for use in scenarios where a `PanNode` must be persisted or transmitted as JSON, and the instance members allow inspection or modification of the serializable data before conversion.

## API

### Static Members

#### `public static string ToJson(PanNode? node)`

Serializes a `PanNode` to its JSON representation.

- **Parameters**  
  `node`: The `PanNode` to serialize. May be `null`.
- **Returns**  
  A JSON string representing the `PanNode`. If `node` is `null`, the method returns the JSON representation of a default `PanNode` (or an empty object, depending on implementation).
- **Throws**  
  No exceptions are thrown under normal circumstances. Serialization failures (e.g., circular references) are not expected because `PanNode` is a simple data type.

#### `public static PanNode? FromJson(string? json)`

Deserializes a JSON string into a `PanNode`.

- **Parameters**  
  `json`: The JSON string to deserialize. May be `null` or empty.
- **Returns**  
  A `PanNode` instance if deserialization succeeds; otherwise `null`. A `null` or empty `json` argument returns `null`.
- **Throws**  
  Throws a `JsonException` if the JSON string is malformed or cannot be deserialized into a `PanNode`.

#### `public static bool TryFromJson(string? json, out PanNode? result)`

Attempts to deserialize a JSON string into a `PanNode` without throwing an exception.

- **Parameters**  
  `json`: The JSON string to deserialize. May be `null` or empty.  
  `result`: When this method returns, contains the deserialized `PanNode` if successful, or `null` if deserialization failed.
- **Returns**  
  `true` if deserialization succeeded; `false` otherwise. A `null` or empty `json` argument returns `false` and sets `result` to `null`.
- **Throws**  
  Never throws. All deserialization errors are caught internally.

### Instance Members

#### `public string? Name { get; set; }`

Gets or sets the name of the pan node. This property corresponds to the `Name` field of the underlying `PanNode`.

#### `public float Pan { get; set; }`

Gets or sets the pan value. This property corresponds to the `Pan` field of the underlying `PanNode`. The value typically ranges from -1.0 (full left) to 1.0 (full right), but the class does not enforce constraints.

#### `public int Frames { get; set; }`

Gets or sets the number of frames associated with the pan node. This property corresponds to the `Frames` field of the underlying `PanNode`.

#### `public PanNode ToPanNode()`

Creates a new `PanNode` instance from the current property values.

- **Returns**  
  A `PanNode` with `Name`, `Pan`, and `Frames` set to the current values of the corresponding instance properties.
- **Throws**  
  No exceptions are thrown.

## Usage

### Example 1: Serialize a PanNode to JSON and deserialize it back

```csharp
using VstHostLite; // Namespace assumed

// Create a PanNode
var originalNode = new PanNode
{
    Name = "LeftChannel",
    Pan = -0.5f,
    Frames = 128
};

// Serialize to JSON
string json = PanNodeJsonExtensions.ToJson(originalNode);
Console.WriteLine(json);
// Output: {"Name":"LeftChannel","Pan":-0.5,"Frames":128}

// Deserialize back
PanNode? restoredNode = PanNodeJsonExtensions.FromJson(json);
if (restoredNode != null)
{
    Console.WriteLine($"Restored: {restoredNode.Name}, Pan={restoredNode.Pan}, Frames={restoredNode.Frames}");
}
```

### Example 2: Use TryFromJson with invalid input and inspect instance properties

```csharp
using VstHostLite;

// Attempt to deserialize malformed JSON
string badJson = "{ invalid }";
if (PanNodeJsonExtensions.TryFromJson(badJson, out PanNode? result))
{
    Console.WriteLine("Deserialization succeeded.");
}
else
{
    Console.WriteLine("Deserialization failed. Creating default node.");
    // Create a PanNodeJsonExtensions instance manually
    var wrapper = new PanNodeJsonExtensions
    {
        Name = "Default",
        Pan = 0.0f,
        Frames = 256
    };
    PanNode defaultNode = wrapper.ToPanNode();
    Console.WriteLine($"Default node: {defaultNode.Name}, Pan={defaultNode.Pan}, Frames={defaultNode.Frames}");
}
```

## Notes

- **Null handling**: The static methods `FromJson` and `TryFromJson` accept `null` or empty strings and treat them as failure cases (returning `null` or `false`). The `ToJson` method serializes a `null` node to a JSON representation of a default `PanNode` (e.g., `{"Name":null,"Pan":0,"Frames":0}`). Callers should validate inputs when necessary.
- **Property constraints**: The `Pan` property is a `float` and is not validated. Values outside the typical -1.0 to 1.0 range are allowed and will be serialized/deserialized as-is. The `Frames` property is an `int` and may be negative; no validation is performed.
- **Thread safety**: The static methods (`ToJson`, `FromJson`, `TryFromJson`) are thread-safe because they do not rely on any shared mutable state. Instance members (`Name`, `Pan`, `Frames`, `ToPanNode`) are not thread-safe; concurrent reads and writes to the same instance may produce inconsistent results. If multiple threads access an instance, external synchronization is required.
- **Serialization format**: The JSON output uses camelCase property names by default (e.g., `"name"`, `"pan"`, `"frames"`). The exact casing depends on the underlying serializer configuration; the examples above assume PascalCase for clarity.
