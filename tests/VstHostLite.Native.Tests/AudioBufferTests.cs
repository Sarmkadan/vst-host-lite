using Xunit;

namespace VstHostLite.Native.Tests;

public class AudioBufferTests
{
    [Fact]
    public void Constructor_WithZeroChannels_CreatesBufferWithCorrectDimensions()
    {
        // Arrange & Act
        var buffer = new AudioBuffer(0, 10);

        // Assert
        Assert.Equal(0, buffer.Channels);
        Assert.Equal(10, buffer.Frames);
    }

    [Fact]
    public void Constructor_WithNegativeChannels_ThrowsOverflowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<OverflowException>(() => new AudioBuffer(-1, 10));
    }

    [Fact]
    public void Constructor_WithZeroFrames_CreatesBufferWithCorrectDimensions()
    {
        // Arrange & Act
        var buffer = new AudioBuffer(2, 0);

        // Assert
        Assert.Equal(2, buffer.Channels);
        Assert.Equal(0, buffer.Frames);
    }

    [Fact]
    public void Constructor_WithNegativeFrames_ThrowsOverflowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<OverflowException>(() => new AudioBuffer(2, -5));
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesBufferWithCorrectDimensions()
    {
        // Arrange & Act
        var buffer = new AudioBuffer(2, 10);

        // Assert
        Assert.Equal(2, buffer.Channels);
        Assert.Equal(10, buffer.Frames);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesBufferWithZeroedData()
    {
        // Arrange & Act
        var buffer = new AudioBuffer(2, 10);

        // Assert - all samples should be 0
        for (int channel = 0; channel < buffer.Channels; channel++)
        {
            for (int frame = 0; frame < buffer.Frames; frame++)
            {
                Assert.Equal(0f, buffer[channel, frame]);
            }
        }
    }

    [Fact]
    public void Clear_WhenCalled_SetsAllSamplesToZero()
    {
        // Arrange
        var buffer = new AudioBuffer(2, 10);
        buffer[0, 0] = 1.0f;
        buffer[1, 5] = 2.5f;

        // Act
        buffer.Clear();

        // Assert
        for (int channel = 0; channel < buffer.Channels; channel++)
        {
            for (int frame = 0; frame < buffer.Frames; frame++)
            {
                Assert.Equal(0f, buffer[channel, frame]);
            }
        }
    }

    [Fact]
    public void CopyFrom_WithNullBuffer_ThrowsArgumentNullException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<ArgumentNullException>(() => buffer.CopyFrom(null!));
    }

    [Fact]
    public void CopyFrom_WithDifferentChannels_ThrowsArgumentException()
    {
        var buffer1 = new AudioBuffer(2, 10);
        var buffer2 = new AudioBuffer(3, 10);

        var exception = Assert.Throws<ArgumentException>(() => buffer1.CopyFrom(buffer2));
        Assert.Contains("dimensions do not match", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CopyFrom_WithDifferentFrames_ThrowsArgumentException()
    {
        var buffer1 = new AudioBuffer(2, 10);
        var buffer2 = new AudioBuffer(2, 20);

        var exception = Assert.Throws<ArgumentException>(() => buffer1.CopyFrom(buffer2));
        Assert.Contains("dimensions do not match", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CopyFrom_WithSameDimensions_CopiesAllData()
    {
        // Arrange
        var source = new AudioBuffer(2, 10);
        source[0, 0] = 1.0f;
        source[1, 5] = 2.5f;
        source[0, 9] = 3.14f;

        var target = new AudioBuffer(2, 10);
        target[0, 1] = 999f; // Different value to verify copy

        // Act
        target.CopyFrom(source);

        // Assert - all values should match
        for (int channel = 0; channel < source.Channels; channel++)
        {
            for (int frame = 0; frame < source.Frames; frame++)
            {
                Assert.Equal(source[channel, frame], target[channel, frame]);
            }
        }
    }

    [Fact]
    public void ToFlatArray_ReturnsCopyOfInternalBuffer()
    {
        // Arrange
        var buffer = new AudioBuffer(2, 10);
        buffer[0, 0] = 1.0f;
        buffer[1, 5] = 2.5f;

        // Act
        var flatArray = buffer.ToFlatArray();

        // Assert - should be a copy, not the same reference
        var flatArray2 = buffer.ToFlatArray();
        Assert.NotSame(flatArray, flatArray2);
        Assert.Equal(20, flatArray.Length);
        Assert.Equal(1.0f, flatArray[0]);
        Assert.Equal(2.5f, flatArray[15]); // channel 1, frame 5
    }

    [Fact]
    public void Indexer_Get_WithValidIndices_ReturnsCorrectValue()
    {
        // Arrange
        var buffer = new AudioBuffer(2, 10);
        buffer[0, 5] = 42.0f;
        buffer[1, 3] = 17.5f;

        // Act & Assert
        Assert.Equal(42.0f, buffer[0, 5]);
        Assert.Equal(17.5f, buffer[1, 3]);
    }

    [Fact]
    public void Indexer_Get_WithNegativeChannel_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => _ = buffer[-1, 0]);
    }

    [Fact]
    public void Indexer_Get_WithChannelTooLarge_ThrowsIndexOutOfRangeException()
    {
        // The indexer validates bounds and throws for out-of-range indices
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => _ = buffer[2, 0]);
    }

    [Fact]
    public void Indexer_Get_WithNegativeFrame_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => _ = buffer[0, -1]);
    }

    [Fact]
    public void Indexer_Get_WithFrameTooLarge_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => _ = buffer[0, 10]);
    }

    [Fact]
    public void Indexer_Set_WithValidIndices_SetsCorrectValue()
    {
        // Arrange
        var buffer = new AudioBuffer(2, 10);

        // Act
        buffer[0, 5] = 100.0f;
        buffer[1, 3] = 200.5f;

        // Assert
        Assert.Equal(100.0f, buffer[0, 5]);
        Assert.Equal(200.5f, buffer[1, 3]);
    }

    [Fact]
    public void Indexer_Set_WithNegativeChannel_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => buffer[-1, 0] = 1.0f);
    }

    [Fact]
    public void Indexer_Set_WithChannelTooLarge_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => buffer[2, 0] = 1.0f);
    }

    [Fact]
    public void Indexer_Set_WithNegativeFrame_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => buffer[0, -1] = 1.0f);
    }

    [Fact]
    public void Indexer_Set_WithFrameTooLarge_ThrowsIndexOutOfRangeException()
    {
        var buffer = new AudioBuffer(2, 10);
        Assert.Throws<IndexOutOfRangeException>(() => buffer[0, 10] = 1.0f);
    }

    [Fact]
    public void Interleave_WithNullFirstBuffer_ThrowsNullReferenceException()
    {
        var buffer2 = new AudioBuffer(2, 10);
        Assert.Throws<NullReferenceException>(() => AudioBuffer.Interleave(null!, buffer2));
    }

    [Fact]
    public void Interleave_WithNullSecondBuffer_ThrowsNullReferenceException()
    {
        var buffer1 = new AudioBuffer(2, 10);
        Assert.Throws<NullReferenceException>(() => AudioBuffer.Interleave(buffer1, null!));
    }

    [Fact]
    public void Interleave_WithDifferentChannels_ThrowsArgumentException()
    {
        var buffer1 = new AudioBuffer(2, 10);
        var buffer2 = new AudioBuffer(3, 10);

        var exception = Assert.Throws<ArgumentException>(() => AudioBuffer.Interleave(buffer1, buffer2));
        Assert.Contains("Channel counts do not match", exception.Message);
    }

    [Fact]
    public void Interleave_WithValidBuffers_ReturnsCorrectDimensions()
    {
        // Arrange
        var buffer1 = new AudioBuffer(2, 10);
        var buffer2 = new AudioBuffer(2, 15);

        // Act
        var result = AudioBuffer.Interleave(buffer1, buffer2);

        // Assert
        Assert.Equal(2, result.Channels);
        Assert.Equal(25, result.Frames); // 10 + 15
    }

    [Fact]
    public void Interleave_WithValidBuffers_ConcatenatesFramesInOrder()
    {
        // Arrange
        var buffer1 = new AudioBuffer(2, 2);
        buffer1[0, 0] = 1.0f;
        buffer1[1, 0] = 2.0f;
        buffer1[0, 1] = 3.0f;
        buffer1[1, 1] = 4.0f;

        var buffer2 = new AudioBuffer(2, 2);
        buffer2[0, 0] = 5.0f;
        buffer2[1, 0] = 6.0f;
        buffer2[0, 1] = 7.0f;
        buffer2[1, 1] = 8.0f;

        // Act
        var result = AudioBuffer.Interleave(buffer1, buffer2);

        // Assert - first buffer's data comes first
        Assert.Equal(1.0f, result[0, 0]);
        Assert.Equal(2.0f, result[1, 0]);
        Assert.Equal(3.0f, result[0, 1]);
        Assert.Equal(4.0f, result[1, 1]);

        // Then second buffer's data
        Assert.Equal(5.0f, result[0, 2]);
        Assert.Equal(6.0f, result[1, 2]);
        Assert.Equal(7.0f, result[0, 3]);
        Assert.Equal(8.0f, result[1, 3]);
    }

    [Fact]
    public void Deinterleave_WithNullBuffer_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() => AudioBuffer.Deinterleave(null!));
    }

    [Fact]
    public void Deinterleave_WithSingleChannel_ThrowsArgumentException()
    {
        var buffer = new AudioBuffer(1, 10);

        var exception = Assert.Throws<ArgumentException>(() => AudioBuffer.Deinterleave(buffer));
        Assert.Contains("At least two channels required", exception.Message);
    }

    [Fact]
    public void Deinterleave_WithValidBuffer_ReturnsCorrectDimensions()
    {
        // Arrange
        var buffer = new AudioBuffer(3, 10);

        // Act
        var result = AudioBuffer.Deinterleave(buffer);

        // Assert
        Assert.Equal(1, result.Channels);
        Assert.Equal(30, result.Frames); // 3 * 10
    }

    [Fact]
    public void Deinterleave_WithValidBuffer_ReordersSamplesCorrectly()
    {
        // Arrange - 2 channels, 3 frames
        var buffer = new AudioBuffer(2, 3);
        buffer[0, 0] = 1.0f;  // Channel 0, Frame 0
        buffer[1, 0] = 2.0f;  // Channel 1, Frame 0
        buffer[0, 1] = 3.0f;  // Channel 0, Frame 1
        buffer[1, 1] = 4.0f;  // Channel 1, Frame 1
        buffer[0, 2] = 5.0f;  // Channel 0, Frame 2
        buffer[1, 2] = 6.0f;  // Channel 1, Frame 2

        // Act
        var result = AudioBuffer.Deinterleave(buffer);

        // Assert - samples are reordered from interleaved to deinterleaved
        // Original: [ch0f0, ch1f0, ch0f1, ch1f1, ch0f2, ch1f2]
        // Deinterleaved: [ch0f0, ch0f1, ch0f2, ch1f0, ch1f1, ch1f2]
        Assert.Equal(1.0f, result[0, 0]);  // ch0f0
        Assert.Equal(3.0f, result[0, 1]);  // ch0f1
        Assert.Equal(5.0f, result[0, 2]);  // ch0f2
        Assert.Equal(2.0f, result[0, 3]);  // ch1f0
        Assert.Equal(4.0f, result[0, 4]);  // ch1f1
        Assert.Equal(6.0f, result[0, 5]);  // ch1f2
    }

    [Fact]
    public void Deinterleave_WithFourChannelBuffer_ReordersSamplesCorrectly()
    {
        // Arrange - 4 channels, 2 frames
        var buffer = new AudioBuffer(4, 2);
        buffer[0, 0] = 1.0f;
        buffer[1, 0] = 2.0f;
        buffer[2, 0] = 3.0f;
        buffer[3, 0] = 4.0f;
        buffer[0, 1] = 5.0f;
        buffer[1, 1] = 6.0f;
        buffer[2, 1] = 7.0f;
        buffer[3, 1] = 8.0f;

        // Act
        var result = AudioBuffer.Deinterleave(buffer);

        // Assert
        Assert.Equal(1.0f, result[0, 0]);  // ch0f0
        Assert.Equal(5.0f, result[0, 1]);  // ch0f1
        Assert.Equal(2.0f, result[0, 2]);  // ch1f0
        Assert.Equal(6.0f, result[0, 3]);  // ch1f1
        Assert.Equal(3.0f, result[0, 4]);  // ch2f0
        Assert.Equal(7.0f, result[0, 5]);  // ch2f1
        Assert.Equal(4.0f, result[0, 6]);  // ch3f0
        Assert.Equal(8.0f, result[0, 7]);  // ch3f1
    }
}
