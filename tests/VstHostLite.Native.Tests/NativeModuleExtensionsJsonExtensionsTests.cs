using System;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests
{
    public class NativeModuleExtensionsJsonExtensionsTests
    {
        [Fact]
        public void GetJsonExtensions_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            var extensions = new NativeModuleExtensionsJsonExtensions();
            string json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => extensions.GetJsonExtensions(json));
        }

        [Fact]
        public void GetJsonExtensions_WithEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var extensions = new NativeModuleExtensionsJsonExtensions();
            string json = string.Empty;

            // Act
            string result = extensions.GetJsonExtensions(json);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetJsonExtensions_WithWhitespaceString_ReturnsEmptyString()
        {
            // Arrange
            var extensions = new NativeModuleExtensionsJsonExtensions();
            string json = "   \t\n  ";

            // Act
            string result = extensions.GetJsonExtensions(json);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetJsonExtensions_WithValidJson_ReturnsEmptyString()
        {
            // Arrange
            var extensions = new NativeModuleExtensionsJsonExtensions();
            string json = "{\"key\": \"value\"}";

            // Act
            string result = extensions.GetJsonExtensions(json);

            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}