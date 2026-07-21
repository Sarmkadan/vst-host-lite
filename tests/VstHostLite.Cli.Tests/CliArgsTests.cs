using System;
using System.IO;
using Xunit;

namespace VstHostLite.Cli.Tests;

public class CliArgsTests
{
    [Fact]
    public void NoArguments_PrintsUsageAndReturns1()
    {
        // Arrange
        var writer = new StringWriter();
        Console.SetOut(writer);
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = Array.Empty<string>();

        // Act
        var result = Program.Main(args);

        // Assert
        var output = writer.ToString();
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("vst-host-lite", output);
        Assert.Contains("usage:", output);
    }

    [Fact]
    public void UnknownCommand_PrintsUsageAndReturns1()
    {
        // Arrange
        var writer = new StringWriter();
        Console.SetOut(writer);
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "unknowncommand" };

        // Act
        var result = Program.Main(args);

        // Assert
        var output = writer.ToString();
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("vst-host-lite", output);
        Assert.Contains("usage:", output);
    }

    [Fact]
    public void InfoCommand_MissingPath_Returns1()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "info" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("usage: vsthost info <path-to.vst3>", errorOutput);
    }

    [Fact]
    public void ValidateCommand_MissingPath_Returns1()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "validate" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("usage: vsthost validate <path-to-graph.json>", errorOutput);
    }

    [Fact]
    public void GraphCommand_MissingPath_Returns1()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "graph" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("usage: vsthost graph <path-to-graph.json>", errorOutput);
    }

    [Fact]
    public void StatsCommand_MissingPath_Returns1()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "stats" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("usage: vsthost stats <path-to-graph.json>", errorOutput);
    }

    [Fact]
    public void PlayCommand_Returns2WithMessage()
    {
        // Arrange
        var writer = new StringWriter();
        Console.SetOut(writer);
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "play" };

        // Act
        var result = Program.Main(args);

        // Assert
        var output = writer.ToString();
        var errorOutput = errorWriter.ToString();

        Assert.Equal(2, result);
        Assert.Contains("`play` is not implemented", errorOutput);
        Assert.Contains("audio graph routing is unfinished", errorOutput);
    }

    [Fact]
    public void InfoCommand_WithPath_ShowsUsageMessage()
    {
        // Arrange
        var writer = new StringWriter();
        Console.SetOut(writer);
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "info", "test.vst3" };

        // Act
        var result = Program.Main(args);

        // Assert - Info will fail because the file doesn't exist, but it should try to load it
        var output = writer.ToString();
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("error:", errorOutput);
    }

    [Fact]
    public void ValidateCommand_WithPath_ShowsFileNotFound()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "validate", "nonexistent.json" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("error: file not found: nonexistent.json", errorOutput);
    }

    [Fact]
    public void GraphCommand_WithPath_ShowsFileNotFound()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "graph", "nonexistent.json" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("error: file not found: nonexistent.json", errorOutput);
    }

    [Fact]
    public void StatsCommand_WithPath_ShowsFileNotFound()
    {
        // Arrange
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        var args = new[] { "stats", "nonexistent.json" };

        // Act
        var result = Program.Main(args);

        // Assert
        var errorOutput = errorWriter.ToString();

        Assert.Equal(1, result);
        Assert.Contains("error: file not found: nonexistent.json", errorOutput);
    }
}
