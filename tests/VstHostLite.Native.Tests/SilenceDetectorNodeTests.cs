namespace VstHostLite.Native.Tests;

using System;
using VstHostLite.Native;
using Xunit;

public class SilenceDetectorNodeTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var node = new SilenceDetectorNode(channelCount: 2, requiredSilentBuffers: 3, silenceThreshold: 0.01f);

        // Assert
        Assert.Equal(2, node.GetType().GetField("_channelCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(node));
        Assert.Equal(3, node.GetType().GetField("_requiredSilentBuffers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(node));
        Assert.Equal(0.01f, node.GetType().GetField("_silenceThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(node));
        Assert.False(node.IsSilent);
        Assert.Equal(0, node.SilentBufferCount);
    }

    [Fact]
    public void Constructor_ChannelCountLessThan1_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new SilenceDetectorNode(channelCount: 0));
    }

    [Fact]
    public void Constructor_RequiredSilentBuffersLessThan1_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new SilenceDetectorNode(channelCount: 1, requiredSilentBuffers: 0));
    }

    [Fact]
    public void Constructor_SilenceThresholdOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new SilenceDetectorNode(channelCount: 1, silenceThreshold: 0.0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SilenceDetectorNode(channelCount: 1, silenceThreshold: 1.1f));
    }

    [Fact]
    public void Process_FloatArray_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var node = new SilenceDetectorNode(channelCount: 2);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => node.Process((float[])null));
    }

    [Fact]
    public void Process_FloatArray_WrongLength_ThrowsArgumentException()
    {
        // Arrange
        var node = new SilenceDetectorNode(channelCount: 2);
        float[] buffer = new float[3]; // length 3 not multiple of 2

        // Act & Assert
        Assert.Throws<ArgumentException>(() => node.Process(buffer));
    }

    [Fact]
    public void Process_FloatArray_SilentAudio_SetsIsSilentTrueAfterRequiredBuffers()
    {
        // Arrange
        const int channelCount = 2;
        const int requiredSilentBuffers = 2;
        const float silenceThreshold = 0.1f;
        var node = new SilenceDetectorNode(channelCount, requiredSilentBuffers, silenceThreshold);
        int frames = 4;
        float[] silentBuffer = new float[channelCount * frames]; // all zeros

        // Act
        node.Process(silentBuffer);
        Assert.False(node.IsSilent); // after first buffer, count=1 < required
        Assert.Equal(1, node.SilentBufferCount);

        node.Process(silentBuffer);
        // Act
        // Assert
        Assert.True(node.IsSilent);
        Assert.Equal(2, node.SilentBufferCount);
    }

    [Fact]
    public void Process_FloatArray_NonSilentAudio_ResetsSilentBufferCount()
    {
        // Arrange
        const int channelCount = 2;
        const int requiredSilentBuffers = 2;
        const float silenceThreshold = 0.1f;
        var node = new SilenceDetectorNode(channelCount, requiredSilentBuffers, silenceThreshold);
        int frames = 4;
        float[] silentBuffer = new float[channelCount * frames]; // all zeros
        float[] noisyBuffer = new float[channelCount * frames];
        for (int i = 0; i < noisyBuffer.Length; i++)
            noisyBuffer[i] = 0.5f; // RMS = 0.5 > threshold

        // Act
        node.Process(silentBuffer); // count=1
        node.Process(noisyBuffer); // should reset
        // Assert
        Assert.False(node.IsSilent);
        Assert.Equal(0, node.SilentBufferCount);
    }

    [Fact]
    public void Process_AudioBuffer_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var node = new SilenceDetectorNode(channelCount: 2);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => node.Process((AudioBuffer)null));
    }

    [Fact]
    public void Process_AudioBuffer_ChannelCountMismatch_ThrowsArgumentException()
    {
        // Arrange
        var node = new SilenceDetectorNode(channelCount: 2);
        var buffer = new AudioBuffer(channels: 3, frames: 4); // 3 channels != 2

        // Act & Assert
        Assert.Throws<ArgumentException>(() => node.Process(buffer));
    }

    [Fact]
    public void Process_AudioBuffer_SilentAudio_SetsIsSilentTrueAfterRequiredBuffers()
    {
        // Arrange
        const int channelCount = 2;
        const int requiredSilentBuffers = 2;
        const float silenceThreshold = 0.1f;
        var node = new SilenceDetectorNode(channelCount, requiredSilentBuffers, silenceThreshold);
        int frames = 4;
        var silentBuffer = new AudioBuffer(channelCount, frames); // all zeros by default

        // Act
        node.Process(silentBuffer);
        Assert.False(node.IsSilent);
        Assert.Equal(1, node.SilentBufferCount);

        node.Process(silentBuffer);
        // Assert
        Assert.True(node.IsSilent);
        Assert.Equal(2, node.SilentBufferCount);
    }

    [Fact]
    public void Process_AudioBuffer_NonSilentAudio_ResetsSilentBufferCount()
    {
        // Arrange
        const int channelCount = 2;
        const int requiredSilentBuffers = 2;
        const float silenceThreshold = 0.1f;
        var node = new SilenceDetectorNode(channelCount, requiredSilentBuffers, silenceThreshold);
        int frames = 4;
        var silentBuffer = new AudioBuffer(channelCount, frames); // zeros
        var noisyBuffer = new AudioBuffer(channelCount, frames);
        // Fill noisy buffer with 0.5f
        for (int ch = 0; ch < channelCount; ch++)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                noisyBuffer[ch, frame] = 0.5f;
            }
        }

        // Act
        node.Process(silentBuffer); // count=1
        node.Process(noisyBuffer); // reset
        // Assert
        Assert.False(node.IsSilent);
        Assert.Equal(0, node.SilentBufferCount);
    }

    [Fact]
    public void Reset_ResetsState()
    {
        // Arrange
        const int channelCount = 2;
        const int requiredSilentBuffers = 2;
        const float silenceThreshold = 0.1f;
        var node = new SilenceDetectorNode(channelCount, requiredSilentBuffers, silenceThreshold);
        int frames = 4;
        var silentBuffer = new AudioBuffer(channelCount, frames);

        // Make it silent for required buffers
        node.Process(silentBuffer);
        node.Process(silentBuffer);
        Assert.True(node.IsSilent);
        Assert.Equal(2, node.SilentBufferCount);

        // Act
        node.Reset();

        // Assert
        Assert.False(node.IsSilent);
        Assert.Equal(0, node.SilentBufferCount);
        // Also check internal _sumSquares cleared? We can't directly, but we can process another buffer and see if it works.
        var noisyBuffer = new AudioBuffer(channelCount, frames);
        for (int ch = 0; ch < channelCount; ch++)
            for (int frame = 0; frame < frames; frame++)
                noisyBuffer[ch, frame] = 0.5f;
        node.Process(noisyBuffer);
        Assert.False(node.IsSilent);
        Assert.Equal(0, node.SilentBufferCount);
    }

    [Fact]
    public void IsSilent_Property_ReturnsCorrectValue()
    {
        // Arrange
        var node = new SilenceDetectorNode(channelCount: 1, requiredSilentBuffers: 1, silenceThreshold: 0.1f);
        int frames = 2;
        var silentBuffer = new AudioBuffer(1, frames);
        var noisyBuffer = new AudioBuffer(1, frames);
        noisyBuffer[0, 0] = 0.5f;
        noisyBuffer[0, 1] = 0.5f;

        // Act & Assert
        Assert.False(node.IsSilent);
        node.Process(silentBuffer);
        Assert.True(node.IsSilent);
        node.Process(noisyBuffer);
        Assert.False(node.IsSilent);
    }

    [Fact]
    public void SilentBufferCount_Property_ReturnsCorrectValue()
    {
        // Arrange
        var node = new SilenceDetectorNode(channelCount: 1, requiredSilentBuffers: 3, silenceThreshold: 0.1f);
        int frames = 2;
        var silentBuffer = new AudioBuffer(1, frames);
        var noisyBuffer = new AudioBuffer(1, frames);
        noisyBuffer[0, 0] = 0.5f;
        noisyBuffer[0, 1] = 0.5f;

        // Act & Assert
        Assert.Equal(0, node.SilentBufferCount);
        node.Process(silentBuffer);
        Assert.Equal(1, node.SilentBufferCount);
        node.Process(silentBuffer);
        Assert.Equal(2, node.SilentBufferCount);
        node.Process(silentBuffer);
        Assert.Equal(3, node.SilentBufferCount);
        node.Process(noisyBuffer);
        Assert.Equal(0, node.SilentBufferCount);
    }
}