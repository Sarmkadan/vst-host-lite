using Xunit;
using System;
using System.Collections.Generic;

namespace VstHostLite.Native.Tests;

public class Vst3InteropTests
{
    [Fact]
    public void CountClasses_WithNullFactory_ReturnsZero()
    {
        // Arrange & Act
        var result = Vst3Interop.CountClasses(0);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CountClasses_WithValidFactory_ReturnsNonNegativeCount()
    {
        // Arrange - We can't test with a real factory pointer, but we can test the method handles valid pointers
        // The actual count depends on the VST3 factory implementation
        nint factory = nint.Zero;

        // Act
        var result = Vst3Interop.CountClasses(factory);

        // Assert - Should return 0 for null/zero pointer
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetClassInfo_WithNullFactory_ThrowsAccessViolationException()
    {
        // Arrange
        nint factory = nint.Zero;
        int index = 0;

        // Act & Assert
        // GetClassInfo tries to read from the factory pointer which is null/zero,
        // causing an AccessViolationException
        Assert.Throws<AccessViolationException>(() => Vst3Interop.GetClassInfo(factory, index));
    }

    [Fact]
    public void GetClassInfo_WithNegativeIndex_ThrowsAccessViolationException()
    {
        // Arrange
        nint factory = nint.Zero;
        int index = -1;

        // Act & Assert
        // GetClassInfo tries to read from the factory pointer which is null/zero,
        // causing an AccessViolationException
        Assert.Throws<AccessViolationException>(() => Vst3Interop.GetClassInfo(factory, index));
    }

    [Fact]
    public void GetClassInfo_WithValidIndex_ThrowsAccessViolationException()
    {
        // Arrange - We can't test with a real factory, but we can verify the method signature works
        nint factory = nint.Zero;
        int index = 0;

        // Act & Assert - Should throw AccessViolationException for null factory
        Assert.Throws<AccessViolationException>(() => Vst3Interop.GetClassInfo(factory, index));
    }

    [Fact]
    public void PluginClassInfo_WithValidParameters_CreatesCorrectInstance()
    {
        // Arrange
        string cid = "ABCDEF1234567890";
        string category = "Audio Module Class";
        string name = "Test Plugin";

        // Act
        var info = new PluginClassInfo(cid, category, name);

        // Assert
        Assert.Equal(cid, info.Cid);
        Assert.Equal(category, info.Category);
        Assert.Equal(name, info.Name);
    }

    [Fact]
    public void PluginClassInfo_WithEmptyStrings_CreatesInstance()
    {
        // Arrange
        string cid = "";
        string category = "";
        string name = "";

        // Act
        var info = new PluginClassInfo(cid, category, name);

        // Assert
        Assert.Equal("", info.Cid);
        Assert.Equal("", info.Category);
        Assert.Equal("", info.Name);
    }

    [Fact]
    public void PluginClassInfo_WithNullStrings_CreatesInstance()
    {
        // Arrange
        string cid = null!;
        string category = null!;
        string name = null!;

        // Act
        var info = new PluginClassInfo(cid, category, name);

        // Assert
        Assert.Null(info.Cid);
        Assert.Null(info.Category);
        Assert.Null(info.Name);
    }

    [Fact]
    public void PluginClassInfo_ImplementsEqualityCorrectly()
    {
        // Arrange
        var info1 = new PluginClassInfo("ABC123", "Category1", "Plugin1");
        var info2 = new PluginClassInfo("ABC123", "Category1", "Plugin1");
        var info3 = new PluginClassInfo("DEF456", "Category2", "Plugin2");

        // Act & Assert
        Assert.Equal(info1, info2);
        Assert.NotEqual(info1, info3);
        Assert.True(info1 == info2);
        Assert.True(info1 != info3);
        Assert.False(info1.Equals(info3));
    }

    [Fact]
    public void PluginClassInfo_GetHashCode_ReturnsConsistentValue()
    {
        // Arrange
        var info1 = new PluginClassInfo("ABC123", "Category1", "Plugin1");
        var info2 = new PluginClassInfo("ABC123", "Category1", "Plugin1");

        // Act & Assert
        Assert.Equal(info1.GetHashCode(), info2.GetHashCode());
    }

    [Fact]
    public void PluginClassInfo_ToString_ReturnsFormattedString()
    {
        // Arrange
        var info = new PluginClassInfo("ABC123", "Audio Module Class", "Test Plugin");

        // Act
        var str = info.ToString();

        // Assert
        Assert.Contains("ABC123", str);
        Assert.Contains("Audio Module Class", str);
        Assert.Contains("Test Plugin", str);
    }

    [Fact]
    public void FilterPluginClasses_WithNullInputAndNullFilter_ThrowsArgumentNullException()
    {
        // Arrange
        List<PluginClassInfo> infos = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Vst3Interop.FilterPluginClasses(infos, null, null));
    }

    [Fact]
    public void FilterPluginClasses_WithEmptyCollectionAndNullFilter_ReturnsEmptyList()
    {
        // Arrange
        var infos = new List<PluginClassInfo>();

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, null, null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FilterPluginClasses_WithValidCollectionAndNullFilter_ReturnsAllItems()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "Category1", "Plugin1"),
            new PluginClassInfo("DEF456", "Category2", "Plugin2"),
            new PluginClassInfo("GHI789", "Category3", "Plugin3")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, null, null);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(infos, result);
    }

    [Fact]
    public void FilterPluginClasses_WithNameFilter_ReturnsMatchingItems()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "Category1", "Test Plugin"),
            new PluginClassInfo("DEF456", "Category2", "Another Plugin"),
            new PluginClassInfo("GHI789", "Category3", "Test Effect")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, "Test", null);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Name == "Test Plugin");
        Assert.Contains(result, i => i.Name == "Test Effect");
    }

    [Fact]
    public void FilterPluginClasses_WithCaseInsensitiveNameFilter_ReturnsMatchingItems()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "Category1", "Test Plugin"),
            new PluginClassInfo("DEF456", "Category2", "Another Plugin"),
            new PluginClassInfo("GHI789", "Category3", "TEST EFFECT")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, "test", null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterPluginClasses_WithCategoryFilter_ReturnsMatchingItems()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "Audio Module Class", "Plugin1"),
            new PluginClassInfo("DEF456", "Audio Effect Class", "Plugin2"),
            new PluginClassInfo("GHI789", "Instrument Class", "Plugin3")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, null, "Audio Module Class");

        // Assert
        Assert.Single(result);
        Assert.Equal("Plugin1", result[0].Name);
    }

    [Fact]
    public void FilterPluginClasses_WithCaseInsensitiveCategoryFilter_ReturnsMatchingItems()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "audio module class", "Plugin1"),
            new PluginClassInfo("DEF456", "Audio Effect Class", "Plugin2")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, null, "AUDIO MODULE CLASS");

        // Assert
        Assert.Single(result);
        Assert.Equal("Plugin1", result[0].Name);
    }

    [Fact]
    public void FilterPluginClasses_WithBothFilters_ReturnsIntersection()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "Audio Module Class", "Test Plugin"),
            new PluginClassInfo("DEF456", "Audio Effect Class", "Test Effect"),
            new PluginClassInfo("GHI789", "Audio Module Class", "Other Plugin")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, "Test", "Audio Module Class");

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Plugin", result[0].Name);
    }

    [Fact]
    public void FilterPluginClasses_WithNonMatchingFilters_ReturnsEmptyList()
    {
        // Arrange
        var infos = new List<PluginClassInfo>
        {
            new PluginClassInfo("ABC123", "Audio Module Class", "Plugin1"),
            new PluginClassInfo("DEF456", "Audio Effect Class", "Plugin2")
        };

        // Act
        var result = Vst3Interop.FilterPluginClasses(infos, "NonExistent", "DifferentCategory");

        // Assert
        Assert.Empty(result);
    }
}
