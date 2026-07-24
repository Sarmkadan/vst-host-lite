using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class MeteringNodeExtensionsTests
{
    // Helper to create a MeteringNode with custom Peak/RMS data via reflection.
    private static MeteringNode CreateNode(float[] peak, float[] rms)
    {
        // Create an instance of the concrete MeteringNode type.
        var node = (MeteringNode)Activator.CreateInstance(typeof(MeteringNode))!;

        // Find the nested MeteringData type (could be public or internal).
        var meteringDataType = typeof(MeteringNode).GetNestedType("MeteringData",
            BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("MeteringData type not found.");

        // Create an instance of the MeteringData struct/class.
        var meteringData = Activator.CreateInstance(meteringDataType)!;

        // Set the Peak and RMS fields (they are expected to be float[]).
        var peakField = meteringDataType.GetField("Peak",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Peak field not found.");
        var rmsField = meteringDataType.GetField("RMS",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("RMS field not found.");

        peakField.SetValue(meteringData, peak);
        rmsField.SetValue(meteringData, rms);

        // Assign the MeteringData to the node's CurrentMetering property (may be private setter).
        var prop = typeof(MeteringNode).GetProperty("CurrentMetering",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("CurrentMetering property not found.");

        prop.SetValue(node, meteringData);
        return node;
    }

    [Fact]
    public void GetPeakLevel_HappyPath_ReturnsMaximumPeak()
    {
        var node = CreateNode(new[] { 0.2f, 0.9f, 0.5f }, new[] { 0.1f, 0.2f, 0.3f });
        var result = node.GetPeakLevel();
        Assert.Equal(0.9f, result);
    }

    [Fact]
    public void GetPeakLevel_EmptyPeak_ReturnsZero()
    {
        var node = CreateNode(Array.Empty<float>(), new[] { 0.1f });
        var result = node.GetPeakLevel();
        Assert.Equal(0.0f, result);
    }

    [Fact]
    public void GetRmsLevel_HappyPath_ReturnsMaximumRms()
    {
        var node = CreateNode(new[] { 0.2f, 0.9f }, new[] { 0.1f, 0.4f });
        var result = node.GetRmsLevel();
        Assert.Equal(0.4f, result);
    }

    [Fact]
    public void GetRmsLevel_EmptyRms_ReturnsZero()
    {
        var node = CreateNode(new[] { 0.2f }, Array.Empty<float>());
        var result = node.GetRmsLevel();
        Assert.Equal(0.0f, result);
    }

    [Fact]
    public void GetChannelMetering_HappyPath_ReturnsAllChannels()
    {
        var node = CreateNode(new[] { 0.2f, 0.9f }, new[] { 0.1f, 0.3f });
        var result = node.GetChannelMetering().ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal((0, 0.2f, 0.1f), result[0]);
        Assert.Equal((1, 0.9f, 0.3f), result[1]);
    }

    [Fact]
    public void GetChannelMetering_MismatchedLengths_UsesMinimumLength()
    {
        var node = CreateNode(new[] { 0.5f, 0.6f, 0.7f }, new[] { 0.1f, 0.2f });
        var result = node.GetChannelMetering().ToList();

        // Only two channels because RMS array is shorter.
        Assert.Equal(2, result.Count);
        Assert.Equal((0, 0.5f, 0.1f), result[0]);
        Assert.Equal((1, 0.6f, 0.2f), result[1]);
    }

    [Fact]
    public void GetPeakChannel_HappyPath_ReturnsHighestPeak()
    {
        var node = CreateNode(new[] { 0.2f, 0.9f, 0.5f }, new[] { 0.1f, 0.2f, 0.3f });
        var (index, peak) = node.GetPeakChannel();

        Assert.Equal(1, index);
        Assert.Equal(0.9f, peak);
    }

    [Fact]
    public void GetPeakChannel_NoChannels_ReturnsMinusOneZero()
    {
        var node = CreateNode(Array.Empty<float>(), Array.Empty<float>());
        var (index, peak) = node.GetPeakChannel();

        Assert.Equal(-1, index);
        Assert.Equal(0.0f, peak);
    }

    [Fact]
    public void GetPeakLevel_NullNode_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((MeteringNode)null!).GetPeakLevel());
    }

    [Fact]
    public void GetRmsLevel_NullNode_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((MeteringNode)null!).GetRmsLevel());
    }

    [Fact]
    public void GetChannelMetering_NullNode_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((MeteringNode)null!).GetChannelMetering());
    }

    [Fact]
    public void GetPeakChannel_NullNode_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((MeteringNode)null!).GetPeakChannel());
    }
}
