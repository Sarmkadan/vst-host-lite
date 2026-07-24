using System.Linq;
using System.Reflection;
using Xunit;

namespace VstHostLite.Native.Tests;

/// <summary>
/// Guards against JSON-helper generator drift: running the generator over its own previous
/// output produces types named after other generated types (e.g. "FooJsonExtensionsJsonExtensions"),
/// which serialize static extension classes rather than real domain data. This test fails fast if
/// any public type in <see cref="VstHostLite.Native"/> exhibits that duplicated-suffix pattern.
/// </summary>
public class PublicTypeNamingGuardTests
{
    /// <summary>
    /// Suffix fragments that, when repeated back-to-back in a type name, indicate the JSON-helper
    /// generator was re-run over its own previous output instead of over a real domain type.
    /// </summary>
    private static readonly string[] _duplicatedSuffixFragments =
    [
        "ExtensionsExtensions",
        "JsonExtensionsJsonExtensions",
    ];

    [Fact]
    public void PublicTypes_DoNotContainDuplicatedGeneratorSuffixes()
    {
        var assembly = typeof(AudioGraph).Assembly;

        var offendingTypes = assembly.GetTypes()
            .Where(type => type.IsPublic)
            .Where(type => _duplicatedSuffixFragments.Any(fragment => type.Name.Contains(fragment)))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(
            offendingTypes.Count == 0,
            $"Found type name(s) with a duplicated generator suffix, indicating the JSON-helper " +
            $"generator was run over its own output: {string.Join(", ", offendingTypes)}");
    }

    [Fact]
    public void PublicTypes_DoNotSerializeOtherJsonExtensionsTypes()
    {
        var assembly = typeof(AudioGraph).Assembly;

        var offendingTypes = assembly.GetTypes()
            .Where(type => type.IsPublic && type.IsAbstract && type.IsSealed) // static class
            .Where(type => type.Name.EndsWith("JsonExtensions", System.StringComparison.Ordinal))
            .Where(type => type.Name[..^"JsonExtensions".Length].EndsWith("JsonExtensions", System.StringComparison.Ordinal))
            .Select(type => type.FullName)
            .ToList();

        Assert.True(
            offendingTypes.Count == 0,
            $"Found second-order JSON helper type(s) serializing another JsonExtensions type: {string.Join(", ", offendingTypes)}");
    }
}
