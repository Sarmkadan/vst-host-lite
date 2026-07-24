using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace VstHostLite.Native;

/// <summary>
/// Provides JSON (de)serialization helpers for <see cref="MidiEventQueue"/>.
/// </summary>
public static class MidiEventQueueJsonExtensions
{
    /// <summary>
    /// Cached JSON serializer options using camel‑case naming.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Serializes the <see cref="MidiEventQueue"/> to a JSON string.
    /// </summary>
    /// <param name="value">The queue to serialize.</param>
    /// <param name="indented">
    /// If <c>true</c>, the output will be formatted with indentation; otherwise it will be compact.
    /// </param>
    /// <returns>A JSON representation of the queue.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this MidiEventQueue value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // Retrieve the private fields via reflection.
        var eventsField = typeof(MidiEventQueue).GetField("_events", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate '_events' field.");
        var overflowField = typeof(MidiEventQueue).GetField("_overflowPolicy", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Unable to locate '_overflowPolicy' field.");

        var events = (List<MidiEvent>)eventsField.GetValue(value)!;
        var overflow = (MidiEventQueue.OverflowPolicy)overflowField.GetValue(value)!;

        var dto = new MidiEventQueueDto
        {
            Capacity = value.Capacity,
            OverflowPolicy = overflow,
            Events = events
        };

        var options = indented
            ? new JsonSerializerOptions(JsonOptions) { WriteIndented = true }
            : JsonOptions;

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string into a new <see cref="MidiEventQueue"/> instance.
    /// </summary>
    /// <param name="json">The JSON representation of a queue.</param>
    /// <returns>
    /// A new <see cref="MidiEventQueue"/> populated with the data from <paramref name="json"/>,
    /// or <c>null</c> if the JSON could not be deserialized.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static MidiEventQueue? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        var dto = JsonSerializer.Deserialize<MidiEventQueueDto>(json, JsonOptions);
        if (dto is null)
        {
            return null;
        }

        var queue = new MidiEventQueue(dto.Capacity, dto.OverflowPolicy);
        if (dto.Events is not null && dto.Events.Count > 0)
        {
            queue.EnqueueRange(dto.Events);
        }

        return queue;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into a <see cref="MidiEventQueue"/>.
    /// </summary>
    /// <param name="json">The JSON representation of a queue.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="MidiEventQueue"/>
    /// if the operation succeeded; otherwise <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if deserialization succeeded; <c>false</c> if a <see cref="JsonException"/>
    /// was thrown.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out MidiEventQueue? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    // DTO used for (de)serialization.
    private sealed class MidiEventQueueDto
    {
        public int Capacity { get; set; }
        public MidiEventQueue.OverflowPolicy OverflowPolicy { get; set; }
        public List<MidiEvent>? Events { get; set; }
    }
}
