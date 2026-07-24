using Xunit;

namespace VstHostLite.Native.Tests
{
    public class AudioBufferExtensionsTests
    {
        [Fact]
        public void Clone_HappyPath_ReturnsNewBuffer()
        {
            // Arrange
            var buffer = new AudioBuffer(2, 10);
            for (int i = 0; i < buffer.Channels; i++)
            {
                for (int j = 0; j < buffer.Frames; j++)
                {
                    buffer[i, j] = 1.0f;
                }
            }

            // Act
            var clone = buffer.Clone();

            // Assert
            Assert.NotSame(buffer, clone);
            for (int i = 0; i < buffer.Channels; i++)
            {
                for (int j = 0; j < buffer.Frames; j++)
                {
                    Assert.Equal(buffer[i, j], clone[i, j]);
                }
            }
        }

        [Fact]
        public void Clone_NullBuffer_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new AudioBuffer(2, 10).Clone());
        }

        [Fact]
        public void CopyToChannel_HappyPath_CopiesData()
        {
            // Arrange
            var source = new AudioBuffer(1, 10);
            for (int i = 0; i < source.Frames; i++)
            {
                source[0, i] = 1.0f;
            }

            var target = new AudioBuffer(2, 10);
            for (int i = 0; i < target.Channels; i++)
            {
                for (int j = 0; j < target.Frames; j++)
                {
                    target[i, j] = 0.0f;
                }
            }

            // Act
            target.CopyToChannel(source, 0);

            // Assert
            for (int i = 0; i < target.Frames; i++)
            {
                Assert.Equal(source[i, 0], target[0, i]);
                Assert.Equal(source[i, 0], target[1, i]);
            }
        }

        [Fact]
        public void CopyToChannel_NullSource_ThrowsArgumentNullException()
        {
            // Act and Assert
            var target = new AudioBuffer(2, 10);
            Assert.Throws<ArgumentNullException>(() => target.CopyToChannel(null, 0));
        }

        [Fact]
        public void GetChannelBuffers_HappyPath_ReturnsEnumerable()
        {
            // Arrange
            var buffer = new AudioBuffer(2, 10);
            for (int i = 0; i < buffer.Channels; i++)
            {
                for (int j = 0; j < buffer.Frames; j++)
                {
                    buffer[i, j] = 1.0f;
                }
            }

            // Act
            var channels = buffer.GetChannelBuffers();

            // Assert
            Assert.Single(channels);
            var channel = channels.First();
            for (int i = 0; i < channel.Frames; i++)
            {
                Assert.Equal(buffer[0, i], channel[i, 0]);
            }
        }

        [Fact]
        public void MixWith_HappyPath_MixesBuffers()
        {
            // Arrange
            var buffer = new AudioBuffer(2, 10);
            for (int i = 0; i < buffer.Channels; i++)
            {
                for (int j = 0; j < buffer.Frames; j++)
                {
                    buffer[i, j] = 1.0f;
                }
            }

            var other = new AudioBuffer(2, 10);
            for (int i = 0; i < other.Channels; i++)
            {
                for (int j = 0; j < other.Frames; j++)
                {
                    other[i, j] = 2.0f;
                }
            }

            // Act
            buffer.MixWith(other, 0.5f);

            // Assert
            for (int i = 0; i < buffer.Frames; i++)
            {
                Assert.Equal(1.5f, buffer[0, i]);
                Assert.Equal(1.5f, buffer[1, i]);
            }
        }

        [Fact]
        public void MixWith_NullBuffer_ThrowsArgumentNullException()
        {
            // Act and Assert
            var other = new AudioBuffer(2, 10);
            Assert.Throws<ArgumentNullException>(() => other.MixWith(null, 0.5f));
        }
    }
}
