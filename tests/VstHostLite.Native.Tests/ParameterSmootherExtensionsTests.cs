using Xunit;

namespace VstHostLite.Native.Tests;

public class ParameterSmootherExtensionsTests
{
    private const float SampleRate = 44100f;
    private const float TimeConstant = 0.1f;
    private const float InitialValue = 0.5f;

    [Fact]
    public void ProcessToArray_WithValidParameters_ReturnsArrayOfCorrectSize()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var result = smoother.ProcessToArray(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Length);
    }

    [Fact]
    public void ProcessToArray_WithValidParameters_ReturnsSmoothedValues()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        smoother.Target = 1.0f;

        // Act
        var result = smoother.ProcessToArray(10);

        // Assert - values should be smoothing towards target
        Assert.NotEqual(InitialValue, result[0]); // Should have changed
        Assert.True(result[0] >= InitialValue && result[0] <= 1.0f); // Within bounds
        Assert.True(result[9] >= result[0] && result[9] <= 1.0f); // Progressing towards target
    }

    [Fact]
    public void ProcessToArray_WithCountOne_ReturnsSingleValue()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        smoother.Target = 1.0f;

        // Act
        var result = smoother.ProcessToArray(1);

        // Assert
        Assert.Single(result);
        Assert.NotEqual(InitialValue, result[0]);
    }

    [Fact]
    public void ProcessToArray_WithCountZero_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => smoother.ProcessToArray(0));
    }

    [Fact]
    public void ProcessToArray_WithNegativeCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => smoother.ProcessToArray(-1));
    }

    [Fact]
    public void ProcessToArray_WithLargeCount_ReturnsArrayOfCorrectSize()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var result = smoother.ProcessToArray(10000);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10000, result.Length);
    }

    [Fact]
    public void ProcessToArray_WithCustomTarget_ReturnsSmoothedValuesTowardsCustomTarget()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var customTarget = 0.8f;

        // Act
        var result = smoother.ProcessToArray(5, customTarget);

        // Assert
        Assert.Equal(5, result.Length);
        // All values should be smoothing towards customTarget
        foreach (var value in result)
        {
            Assert.InRange(value, InitialValue, customTarget);
        }
    }

    [Fact]
    public void ProcessToArray_WithCustomTarget_PreservesOriginalTargetAfterProcessing()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var originalTarget = 0.3f;
        smoother.Target = originalTarget;
        var customTarget = 0.8f;

        // Act
        var result = smoother.ProcessToArray(5, customTarget);

        // Assert
        Assert.Equal(originalTarget, smoother.Target);
    }

    [Fact]
    public void ProcessToArray_WithCountOneAndCustomTarget_ReturnsSingleValue()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var customTarget = 0.9f;

        // Act
        var result = smoother.ProcessToArray(1, customTarget);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public void ProcessTargets_WithNullTargets_ThrowsArgumentNullException()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => smoother.ProcessTargets(null!));
    }

    [Fact]
    public void ProcessTargets_WithEmptyList_ReturnsEmptyArray()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var result = smoother.ProcessTargets(new List<float>());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ProcessTargets_WithSingleTarget_ReturnsSingleValue()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, 0f);

        // Act
        var result = smoother.ProcessTargets(new[] { 1.0f });

        // Assert
        Assert.Single(result);
        Assert.NotEqual(0f, result[0]); // Should have changed from initial value
    }

    [Fact]
    public void ProcessTargets_WithMultipleTargets_ReturnsArrayOfCorrectSize()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var targets = new[] { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

        // Act
        var result = smoother.ProcessTargets(targets);

        // Assert
        Assert.Equal(targets.Length, result.Length);
    }

    [Fact]
    public void ProcessTargets_WithMultipleTargets_ReturnsSmoothedValues()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var targets = new[] { 0.2f, 0.8f };

        // Act
        var result = smoother.ProcessTargets(targets);

        // Assert - values should be smoothing between targets
        Assert.Equal(2, result.Length);
        Assert.NotEqual(targets[0], result[0]); // First value should be smoothed from initial
        Assert.NotEqual(targets[1], result[1]); // Second value should be smoothed from first target
    }

    [Fact]
    public void ProcessTargets_WithIEnumerable_ReturnsCorrectValues()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        IEnumerable<float> targets = new[] { 0.1f, 0.3f, 0.5f };

        // Act
        var result = smoother.ProcessTargets(targets);

        // Assert
        Assert.Equal(3, result.Length);
    }

    [Fact]
    public void GetSmoothingRatio_WithNullSmoother_ThrowsArgumentNullException()
    {
        // Arrange
        ParameterSmoother? smoother = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => smoother!.GetSmoothingRatio());
    }

    [Fact]
    public void GetSmoothingRatio_ReturnsValueBetweenZeroAndOne()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var ratio = smoother.GetSmoothingRatio();

        // Assert - smoothing ratio should be in valid range
        Assert.InRange(ratio, 0f, 1f);
    }

    [Fact]
    public void GetSmoothingRatio_ReturnsConsistentValue()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var ratio1 = smoother.GetSmoothingRatio();
        var ratio2 = smoother.GetSmoothingRatio();

        // Assert - should be deterministic
        Assert.Equal(ratio1, ratio2);
    }

    [Fact]
    public void GetSmoothingRatio_WithDifferentTimeConstants_ReturnsDifferentRatios()
    {
        // Arrange
        var smootherFast = new ParameterSmoother(SampleRate, 0.01f, InitialValue); // Fast smoothing
        var smootherSlow = new ParameterSmoother(SampleRate, 1.0f, InitialValue); // Slow smoothing

        // Act
        var ratioFast = smootherFast.GetSmoothingRatio();
        var ratioSlow = smootherSlow.GetSmoothingRatio();

        // Assert - fast smoothing should have higher alpha
        Assert.True(ratioFast > ratioSlow);
    }

    [Fact]
    public void ProcessToArray_DoesNotModifySmootherState()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var originalTarget = smoother.Target;
        var originalCurrent = smoother.Current;

        // Act
        var result = smoother.ProcessToArray(10);

        // Assert
        Assert.Equal(originalTarget, smoother.Target);
        Assert.Equal(originalCurrent, smoother.Current);
    }

    [Fact]
    public void ProcessTargets_DoesNotModifySmootherState()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var originalTarget = smoother.Target;
        var originalCurrent = smoother.Current;

        var targets = new[] { 0.2f, 0.8f, 0.5f };

        // Act
        var result = smoother.ProcessTargets(targets);

        // Assert
        Assert.Equal(originalTarget, smoother.Target);
        Assert.Equal(originalCurrent, smoother.Current);
    }

}