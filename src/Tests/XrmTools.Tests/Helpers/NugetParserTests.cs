namespace XrmTools.Tests.Helpers;

using FluentAssertions;
using Xunit;
using XrmTools.Helpers;

public class NugetParserTests
{
    [Theory]
    [InlineData("prefix_MyPlugin", "prefix")]
    [InlineData("Prefix_MyPlugin", "prefix")]
    public void HasPackagePrefix_Should_Return_True_When_Package_Already_Has_Prefix(string packageName, string prefix)
    {
        NugetParser.HasPackagePrefix(packageName, prefix).Should().BeTrue();
    }

    [Theory]
    [InlineData("prefix", "prefix")]
    [InlineData("prefix_", "prefix")]
    [InlineData("MyPlugin", "prefix")]
    [InlineData("other_MyPlugin", "prefix")]
    public void HasPackagePrefix_Should_Return_False_When_Package_Does_Not_Have_Prefix(string packageName, string prefix)
    {
        NugetParser.HasPackagePrefix(packageName, prefix).Should().BeFalse();
    }

    [Fact]
    public void EnsurePackagePrefix_Should_Add_Prefix_When_Missing()
    {
        NugetParser.EnsurePackagePrefix("MyPlugin", "prefix").Should().Be("prefix_MyPlugin");
    }

    [Fact]
    public void EnsurePackagePrefix_Should_Not_Duplicate_Prefix_When_Already_Present()
    {
        NugetParser.EnsurePackagePrefix("prefix_MyPlugin", "prefix").Should().Be("prefix_MyPlugin");
    }

    [Fact]
    public void EnsurePackagePrefix_Should_Normalize_Trailing_Underscore_In_Prefix()
    {
        NugetParser.EnsurePackagePrefix("MyPlugin", "prefix_").Should().Be("prefix_MyPlugin");
    }
}
