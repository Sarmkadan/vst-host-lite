# MixerNodeJsonExtensions
The `MixerNodeJsonExtensions` type provides a set of methods for serializing and deserializing `MixerNode` objects to and from JSON, allowing for easy storage and transmission of mixer node configurations. This enables developers to work with mixer nodes in a more flexible and interoperable way, facilitating tasks such as saving and loading project settings or exchanging data between different parts of an application.

## API
* `public static string ToJson(MixerNodeJsonModel model)`: Serializes a `MixerNodeJsonModel` object into a JSON string. This method takes a `MixerNodeJsonModel` instance as input and returns a JSON string representation of the model. It does not throw any exceptions.
* `public static MixerNode? FromJson(string json)`: Deserializes a JSON string into a `MixerNode` object. This method takes a JSON string as input and returns a `MixerNode` instance if the deserialization is successful, or `null` if the input string is invalid or cannot be deserialized. It may throw exceptions if the input string is malformed or if there are errors during deserialization.
* `public static bool TryFromJson(string json, out MixerNode? node)`: Attempts to deserialize a JSON string into a `MixerNode` object. This method takes a JSON string as input and returns a boolean indicating whether the deserialization was successful. If successful, the deserialized `MixerNode` instance is stored in the `out` parameter `node`. It does not throw any exceptions.
* `public string Name { get; }`: Gets the name of the mixer node. This property returns a string representing the name of the mixer node.
* `public int InputCount { get; }`: Gets the number of inputs of the mixer node. This property returns an integer representing the number of inputs.
* `public int Frames { get; }`: Gets the number of frames of the mixer node. This property returns an integer representing the number of frames.
* `public float[] Gains { get; }`: Gets the gains of the mixer node. This property returns an array of floats representing the gains.
* `public MixerNodeJsonModel { get; }`: Gets the JSON model of the mixer node. This property returns a `MixerNodeJsonModel` instance representing the JSON model.
* `public MixerNode ToMixerNode { get; }`: Gets the mixer node. This property returns a `MixerNode` instance representing the mixer node.

## Usage
```csharp
// Example 1: Serializing a MixerNode to JSON
MixerNodeJsonModel model = new MixerNodeJsonModel("MyMixerNode", 2, 1024, new float[] { 0.5f, 0.7f });
string json = MixerNodeJsonExtensions.ToJson(model);
Console.WriteLine(json);

// Example 2: Deserializing JSON to a MixerNode
string json = "{\"Name\":\"MyMixerNode\",\"InputCount\":2,\"Frames\":1024,\"Gains\":[0.5,0.7]}";
MixerNode? node = MixerNodeJsonExtensions.FromJson(json);
if (node != null)
{
    Console.WriteLine(node.Name);
}
```

## Notes
When working with `MixerNodeJsonExtensions`, it is essential to consider edge cases such as invalid or malformed JSON input, which may result in exceptions or `null` values. Additionally, the thread-safety of this type depends on the underlying implementation of the `MixerNode` and `MixerNodeJsonModel` classes. If these classes are not thread-safe, using `MixerNodeJsonExtensions` in a multi-threaded environment may lead to unexpected behavior or errors. It is recommended to review the documentation and implementation of these classes to ensure safe and correct usage.
