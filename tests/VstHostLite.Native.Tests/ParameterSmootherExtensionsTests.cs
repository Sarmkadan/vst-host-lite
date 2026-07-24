using Xunit;

namespace VstHostLite.Native.Tests;

public class ParameterSmootherExtensionsTests
{
    private const float SampleRate = 44100f;
    private const float TimeConstant = 0.1f;
    private const float InitialValue = 0.5f;

    [Fact]
    public void ProcessToArray_WithZeroCount_ThrowsArgumentOutOfRangeException()
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
    public void ProcessToArray_WithNullDestination_ThrowsArgumentNullException()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        float[]? destination = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => smoother.Process(destination!));
    }

    [Fact]
    public void ProcessToArray_WithTargetEqualToCurrentValue_NoStateChange()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var originalCurrent = smoother.Current;
        var originalTarget = smoother.Target;

        // Act - process with target equal to current value
        var result = smoother.ProcessToArray(10, InitialValue);

        // Assert - no state should change when target equals current
        Assert.Equal(10, result.Length);
        Assert.All(result, value => Assert.Equal(InitialValue, value));
        Assert.Equal(originalCurrent, smoother.Current);
        Assert.Equal(originalTarget, smoother.Target);
    }

    [Fact]
    public void ProcessToArray_WithTargetEqualToCurrentValue_ReturnsCorrectValues()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        smoother.Target = InitialValue;

        // Act
        var result = smoother.ProcessToArray(5);

        // Assert - all values should remain at initial value
        Assert.Equal(5, result.Length);
        Assert.All(result, value => Assert.Equal(InitialValue, value));
    }

    [Fact]
    public void ProcessToArray_WithExtremelyLargeTimeConstant_AlphaApproachesZero()
    {
        // Arrange - extremely large time constant results in very slow smoothing (alpha close to 0)
        var largeTimeConstant = 1000000f; // 1 million seconds = ~11 days
        var smoother = new ParameterSmoother(SampleRate, largeTimeConstant, InitialValue);

        // Act - calculate alpha manually to verify
        var expectedAlpha = 1f - (float)Math.Exp(-1.0 / (SampleRate * largeTimeConstant));

        // Assert - alpha should be very close to 0 (extremely slow smoothing)
        Assert.Equal(0f, expectedAlpha);
        Assert.Equal(0f, smoother.GetSmoothingRatio());
    }

    [Fact]
    public void ProcessToArray_WithVerySmallTimeConstant_AlphaApproachesOne()
    {
        // Arrange - very small time constant should result in alpha close to 1 (fast smoothing)
        var smallTimeConstant = 0.0001f;
        var smoother = new ParameterSmoother(SampleRate, smallTimeConstant, InitialValue);
        smoother.Target = 1.0f;

        // Act
        var result = smoother.ProcessToArray(10);

        // Assert - values should quickly approach target
        Assert.All(result, value => Assert.InRange(value, InitialValue, 1.0f));
        Assert.True(result[9] > InitialValue); // Should have changed
    }

    [Fact]
    public void ProcessToArray_WithZeroSampleRate_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(0f, TimeConstant, InitialValue));
    }

    [Fact]
    public void ProcessToArray_WithNegativeSampleRate_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(-44100f, TimeConstant, InitialValue));
    }

    [Fact]
    public void ProcessToArray_WithZeroTimeConstant_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(SampleRate, 0f, InitialValue));
    }

    [Fact]
    public void ProcessToArray_WithNegativeTimeConstant_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new ParameterSmoother(SampleRate, -TimeConstant, InitialValue));
    }

    [Fact]
    public void ProcessToArray_WithNaNTarget_ResultsInNaN()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act - NaN target results in NaN through smoothing math
        var result = smoother.ProcessToArray(5, float.NaN);

        // Assert - NaN propagates through the smoothing calculation
        Assert.All(result, value => Assert.Equal(float.NaN, value));
    }

    [Fact]
    public void ProcessToArray_WithPositiveInfinityTarget_ResultsInNaN()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act - PositiveInfinity target results in NaN through smoothing math
        // (target - current) * alpha where target is Infinity
        var result = smoother.ProcessToArray(5, float.PositiveInfinity);

        // Assert - Infinity causes NaN in smoothing calculation
        Assert.Contains(float.NaN, result);
    }

    [Fact]
    public void ProcessToArray_WithNegativeInfinityTarget_ResultsInNaN()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act - NegativeInfinity target results in NaN through smoothing math
        // (target - current) * alpha where target is Infinity
        var result = smoother.ProcessToArray(5, float.NegativeInfinity);

        // Assert - Infinity causes NaN in smoothing calculation
        Assert.Contains(float.NaN, result);
    }

    [Fact]
    public void ProcessToArray_WithVeryLargeTargetValue_HandlesCorrectly()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var largeTarget = 1000000f;

        // Act
        var result = smoother.ProcessToArray(5, largeTarget);

        // Assert - should handle large values without overflow
        Assert.All(result, value => Assert.InRange(value, InitialValue, largeTarget));
    }

    [Fact]
    public void ProcessToArray_WithVerySmallTargetValue_HandlesCorrectly()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var smallTarget = -1000000f;

        // Act
        var result = smoother.ProcessToArray(5, smallTarget);

        // Assert - should handle small values without underflow
        Assert.All(result, value => Assert.InRange(value, smallTarget, InitialValue));
    }

    [Fact]
    public void ProcessToArray_WithCustomTarget_PreservesOriginalTarget()
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
    public void ProcessToArray_WithCustomTarget_DoesNotModifyCurrentValue()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var originalCurrent = smoother.Current;
        var customTarget = 0.8f;

        // Act
        var result = smoother.ProcessToArray(5, customTarget);

        // Assert - current value should be restored after processing with custom target
        // Use approximate comparison due to floating point precision
        Assert.Equal(originalCurrent, smoother.Current, 3);
    }

    [Fact]
    public void ProcessTargets_WithTargetEqualToCurrentValue_NoStateChange()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        var targets = new[] { InitialValue, InitialValue, InitialValue };
        var originalCurrent = smoother.Current;
        var originalTarget = smoother.Target;

        // Act
        var result = smoother.ProcessTargets(targets);

        // Assert - all values should remain at initial value, no state change
        Assert.Equal(targets.Length, result.Length);
        Assert.All(result, value => Assert.Equal(InitialValue, value));
        Assert.Equal(originalCurrent, smoother.Current);
        Assert.Equal(originalTarget, smoother.Target);
    }

    [Fact]
    public void ProcessTargets_WithMixedTargetValues_ReturnsCorrectSmoothedSequence()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, 0f);
        var targets = new[] { 0.2f, 0.8f, 0.5f, 0.1f };

        // Act
        var result = smoother.ProcessTargets(targets);

        // Assert
        Assert.Equal(targets.Length, result.Length);
        for (int i = 0; i < result.Length; i++)
        {
            Assert.InRange(result[i], 0f, targets[i]);
        }
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
        Assert.NotEqual(0f, result[0]);
    }

    [Fact]
    public void ProcessTargets_WithEmptyArray_ReturnsEmptyArray()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var result = smoother.ProcessTargets(Array.Empty<float>());

        // Assert
        Assert.Empty(result);
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
    public void GetSmoothingRatio_ReturnsValueInRange()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var ratio = smoother.GetSmoothingRatio();

        // Assert
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

        // Assert
        Assert.Equal(ratio1, ratio2);
    }

    [Fact]
    public void GetSmoothingRatio_WithDifferentTimeConstants_ReturnsDifferentRatios()
    {
        // Arrange
        var smootherFast = new ParameterSmoother(SampleRate, 0.01f, InitialValue);
        var smootherSlow = new ParameterSmoother(SampleRate, 1.0f, InitialValue);

        // Act
        var ratioFast = smootherFast.GetSmoothingRatio();
        var ratioSlow = smootherSlow.GetSmoothingRatio();

        // Assert
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

    [Fact]
    public void ProcessToArray_WithCustomTarget_ReturnsArrayOfCorrectSize()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var result = smoother.ProcessToArray(10, 0.8f);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Length);
    }

    [Fact]
    public void ProcessToArray_WithCustomTarget_ReturnsSmoothedValues()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);

        // Act
        var result = smoother.ProcessToArray(10, 1.0f);

        // Assert
        Assert.NotEqual(InitialValue, result[0]);
        Assert.True(result[0] >= InitialValue && result[0] <= 1.0f);
        Assert.True(result[9] >= result[0] && result[9] <= 1.0f);
    }

    [Fact]
    public void ProcessToArray_WithCountOne_ReturnsSingleValue()
    {
        // Arrange
        var smoother = new ParameterSmoother(SampleRate, TimeConstant, InitialValue);
        smoother.Target = 1.0f; // Set a different target to see smoothing

        // Act
        var result = smoother.ProcessToArray(1);

        // Assert
        Assert.Single(result);
        Assert.NotEqual(InitialValue, result[0]);
    }
}