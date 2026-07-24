using System;
using Xunit;

namespace VstHostLite.Native.Tests
{
    public class MeteringNodeTests
    {
        [Fact]
        public void Constructor_HappyPath_CreatesNode()
        {
            // Arrange and Act
            var node = new MeteringNode(2);

            // Assert
            Assert.NotNull(node);
        }

        [Fact]
        public void Constructor_ChannelCountLessThanOne_ThrowsArgumentOutOfRangeException()
        {
            // Act and Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new MeteringNode(0));
        }

        [Fact]
        public void Process_HappyPath_UpdatesMetering()
        {
            // Arrange
            var node = new MeteringNode(1);
            var buffer = new float[] { 1.0f, 2.0f, 3.0f };

            // Act
            node.Process(buffer);

            // Assert
            var metering = node.CurrentMetering;
            Assert.Equal(3.0f, metering.Peak[0]);
            Assert.True(metering.RMS[0] > 0);
        }

        [Fact]
        public void Process_NullBuffer_ThrowsArgumentNullException()
        {
            // Arrange
            var node = new MeteringNode(1);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => node.Process(null));
        }

        [Fact]
        public void Reset_HappyPath_ResetsMetering()
        {
            // Arrange
            var node = new MeteringNode(1);
            var buffer = new float[] { 1.0f, 2.0f, 3.0f };
            node.Process(buffer);

            // Act
            node.Reset();

            // Assert
            var metering = node.CurrentMetering;
            Assert.Equal(0.0f, metering.Peak[0]);
            Assert.Equal(0.0f, metering.RMS[0]);
        }

        [Fact]
        public void CurrentMetering_HappyPath_ReturnsMetering()
        {
            // Arrange
            var node = new MeteringNode(1);
            var buffer = new float[] { 1.0f, 2.0f, 3.0f };
            node.Process(buffer);

            // Act
            var metering = node.CurrentMetering;

            // Assert
            Assert.NotNull(metering);
            Assert.Equal(3.0f, metering.Peak[0]);
            Assert.True(metering.RMS[0] > 0);
        }
    }
}
