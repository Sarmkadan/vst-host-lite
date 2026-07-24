using System;
using VstHostLite.Native;
using Xunit;

namespace VstHostLite.Native.Tests;

public sealed class NoiseGeneratorNodeTests
{
    private const int TestFrames = 64;
    private const string TestName = "noise";

    [Fact]
    public void Constructor_ValidParameters_SetsProperties()
    {
        var node = new NoiseGeneratorNode(TestName, TestFrames);
        Assert.Equal(TestName, node.Name);
        Assert.Equal(TestFrames, node.Frames);
        Assert.Equal(1.0f, node.Amplitude);
    }

    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new NoiseGeneratorNode(null!, TestFrames));
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void Amplitude_SetValidValues_Updates(float value)
    {
        var node = new NoiseGeneratorNode(TestName, TestFrames);
        node.Amplitude = value;
        Assert.Equal(value, node.Amplitude);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Amplitude_SetInvalidValues_ThrowsArgumentOutOfRangeException(float value)
    {
        var node = new NoiseGeneratorNode(TestName, TestFrames);
        Assert.Throws<ArgumentOutOfRangeException>(() => node.Amplitude = value);
    }

    [Fact]
    public void Process_NullOutput_ThrowsArgumentNullException()
    {
        var node = new NoiseGeneratorNode(TestName, TestFrames);
        Assert.Throws<ArgumentNullException>(() => node.Process(null!));
    }

    [Fact]
    public void Process_OutputLengthMismatch_ThrowsArgumentException()
    {
        var node = new NoiseGeneratorNode(TestName, TestFrames);
        var wrongLength = new float[TestFrames - 1];
        var ex = Assert.Throws<ArgumentException>(() => node.Process(wrongLength));
        Assert.Contains($"{TestFrames} frames", ex.Message);
    }

    [Fact]
    public void Process_GeneratesValuesWithinRange()
    {
        var node = new NoiseGeneratorNode(TestName, TestFrames);
        var buffer = new float[TestFrames];
        node.Process(buffer);

        foreach (var sample in buffer)
        {
            Assert.InRange(sample, -node.Amplitude, node.Amplitude);
        }
    }

    [Fact]
    public void Process_WithSeed_ProducesDeterministicOutput()
    {
        const int seed = 12345;
        var node1 = new NoiseGeneratorNode(TestName, TestFrames, seed);
        var node2 = new NoiseGeneratorNode(TestName, TestFrames, seed);

        var buffer1 = new float[TestFrames];
        var buffer2 = new float[TestFrames];

        node1.Process(buffer1);
        node2.Process(buffer2);

        Assert.Equal(buffer1, buffer2);
    }
}
