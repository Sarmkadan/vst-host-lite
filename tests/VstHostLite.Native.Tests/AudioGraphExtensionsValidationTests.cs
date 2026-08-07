using System;
using System.Collections.Generic;
using Xunit;
using VstHostLite.Native;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Contains tests for the AudioGraphExtensionsValidation class.
/// </summary>
public class AudioGraphExtensionsValidationTests
{
    [Fact]
    public void Validate_ThrowsOnNullGraph()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AudioGraphExtensionsValidation.Validate(null!));
    }

    [Fact]
    public void Validate_ReturnsEmptyListForValidGraph()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var result = AudioGraphExtensionsValidation.Validate(graph);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_ThrowsOnNullGraph()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AudioGraphExtensionsValidation.IsValid(null!));
    }

    [Fact]
    public void IsValid_ReturnsTrueForValidGraph()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act
        var result = AudioGraphExtensionsValidation.IsValid(graph);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_ThrowsOnNullGraph()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => AudioGraphExtensionsValidation.EnsureValid(null!));
    }

    [Fact]
    public void EnsureValid_DoesNotThrowForValidGraph()
    {
        // Arrange
        var graph = new AudioGraph();

        // Act & Assert
        var exception = Record.Exception(() => AudioGraphExtensionsValidation.EnsureValid(graph));
        Assert.Null(exception);
    }
}