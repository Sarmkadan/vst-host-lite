using System;
using System.Collections.Generic;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

public class SineGeneratorNodeExtensionsTests
{
    private const int Frames = 512;

    private SineGeneratorNode CreateNode(string name = "test", float frequency = 440f, int frames = Frames)
    {
        return new SineGeneratorNode(name, frequency, frames);
    }

    [Fact]
    public void SetFrequencyFromNote_HappyPath()
    {
        var node = CreateNode();
        node.SetFrequencyFromNote("A4");
        Assert.InRange(node.Frequency, 440f - 0.1f, 440f + 0.1f);
    }

    [Fact]
    public void SetFrequencyFromNote_InvalidNote_Throws()
    {
        var node = CreateNode();
        Assert.Throws<ArgumentException>(() => node.SetFrequencyFromNote("H#4"));
    }

    [Fact]
    public void SetFrequencyFromNote_NullNode_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ((SineGeneratorNode)null!).SetFrequencyFromNote("A4"));
    }

    [Fact]
    public void Generate_HappyPath()
    {
        var node = CreateNode();
        var buffer = node.Generate(440f, 0.5f);
        Assert.Equal(Frames, buffer.Length);
        foreach (var sample in buffer)
        {
            Assert.InRange(sample, -0.5f, 0.5f);
        }
    }

    [Fact]
    public void Generate_AmplitudeOutOfRange_Throws()
    {
        var node = CreateNode();
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Generate(440f, -0.1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Generate(440f, 1.1f));
    }

    [Fact]
    public void Generate_NaNAmplitude_Throws()
    {
        var node = CreateNode();
        Assert.Throws<ArgumentException>(() => node.Generate(440f, float.NaN));
    }

    [Fact]
    public void Generate_NullNode_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ((SineGeneratorNode)null!).Generate(440f, 0.5f));
    }

    [Fact]
    public void GenerateFromNote_HappyPath()
    {
        var node = CreateNode();
        var buffer = node.Generate("C4", 0.3f);
        Assert.Equal(Frames, buffer.Length);
        foreach (var sample in buffer)
        {
            Assert.InRange(sample, -0.3f, 0.3f);
        }
    }

    [Fact]
    public void GenerateBuffers_HappyPath()
    {
        var node = CreateNode();
        var originalFreq = node.Frequency;
        var originalAmp = node.Amplitude;

        var buffers = node.GenerateBuffers(440f, 0.4f, 3);
        Assert.Equal(3, buffers.Count);
        foreach (var buffer in buffers)
        {
            Assert.Equal(Frames, buffer.Length);
            foreach (var sample in buffer)
            {
                Assert.InRange(sample, -0.4f, 0.4f);
            }
        }

        // Ensure node state is restored
        Assert.Equal(originalFreq, node.Frequency);
        Assert.Equal(originalAmp, node.Amplitude);
    }

    [Fact]
    public void GenerateBuffers_BufferCountZero_Throws()
    {
        var node = CreateNode();
        Assert.Throws<ArgumentOutOfRangeException>(() => node.GenerateBuffers(440f, 0.5f, 0));
    }

    [Fact]
    public void GenerateBuffers_NullNode_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ((SineGeneratorNode)null!).GenerateBuffers(440f, 0.5f, 1));
    }
}
