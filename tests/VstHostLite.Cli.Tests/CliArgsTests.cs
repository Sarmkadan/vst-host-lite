using System;
using System.IO;
using Xunit;

/// <summary>
/// Tests for <see cref="CliArgs"/> class
/// </summary>
public class CliArgsTests : IEquatable<CliArgsTests>
{
    public bool Equals(CliArgsTests? other)
    {
        if (other == null)
        {
            return false;
        }

        // TODO: Once we have all properties, generate this method
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as CliArgsTests);

    public override int GetHashCode()
    {
        // TODO: Once we have all properties, generate this method
        return 0;
    }

    public static bool operator ==(CliArgsTests? left, CliArgsTests? right) => Equals(left, right);

    public static bool operator !=(CliArgsTests? left, CliArgsTests? right) => !Equals(left, right);

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