using System;
using VstHostLite.Native;
using Xunit;

public class AudioBufferTests
{
    [Fact]
    public void Interleave_ThrowsArgumentNullException_WhenBuffer1IsNull()
    {
        AudioBuffer buffer2 = new AudioBuffer(2, 10);
        Assert.Throws<ArgumentNullException>(() => AudioBuffer.Interleave(null, buffer2));
    }

    [Fact]
    public void Interleave_ThrowsArgumentNullException_WhenBuffer2IsNull()
    {
        AudioBuffer buffer1 = new AudioBuffer(2, 10);
        Assert.Throws<ArgumentNullException>(() => AudioBuffer.Interleave(buffer1, null));
    }

    [Fact]
    public void Deinterleave_ThrowsArgumentNullException_WhenBufferIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => AudioBuffer.Deinterleave(null));
    }
}