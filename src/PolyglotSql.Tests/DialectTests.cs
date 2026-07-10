using PolyglotSql;
using PolyglotSql.Bundle;
using PolyglotSql.Models;
using System.Reflection;

namespace PolyglotSql.Tests;

public class DialectTests
{
    static DialectTests()
    {
        BundleInitializer.Initialize();
    }

    private static INativePolyglot Provider
        => Polyglot._provider ?? throw new InvalidOperationException("Provider not registered");

    [Fact]
    public void TestDialectCountMatchesEnum()
    {
        int count = Provider.DialectCount();
        int enumCount = Enum.GetValues<Dialect>().Length;

        Assert.Equal(enumCount, count);
    }

    [Fact]
    public void TestDialectListMatchesEnum()
    {
        string[] dialectList = Provider.DialectList();
        string[] enumNames = Enum.GetNames<Dialect>();

        Assert.Equal(enumNames.Length, dialectList.Length);

        var enumSet = new HashSet<string>(enumNames, StringComparer.OrdinalIgnoreCase);
        foreach (var name in dialectList)
        {
            Assert.Contains(name, enumSet);
        }

        var listSet = new HashSet<string>(dialectList, StringComparer.OrdinalIgnoreCase);
        foreach (var name in enumNames)
        {
            Assert.Contains(name, listSet);
        }
    }

    [Fact]
    public void TestEachDialectIsValidEnumMember()
    {
        string[] dialectList = Provider.DialectList();

        foreach (var name in dialectList)
        {
            Assert.True(
                Enum.TryParse<Dialect>(name, true, out _),
                $"Dialect '{name}' from polyglot is not a member of the Dialect enum");
        }
    }

    [Fact]
    public void TestVersionIsNotEmpty()
    {
        string version = Provider.Version();
        Assert.False(string.IsNullOrEmpty(version));
    }
}
