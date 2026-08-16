namespace XrmTools.Tests.Helpers;

using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using XrmTools.Helpers;

public class PluginDetectionHelperTests
{
    [Theory]
    [InlineData("Plugin")]
    [InlineData("PluginAttribute")]
    [InlineData("XrmTools.Meta.Attributes.Plugin")]
    [InlineData("XrmTools.Meta.Attributes.PluginAttribute")]
    public void MatchesPluginName_Should_Match_Plugin_Attribute_Forms(string name)
        => PluginDetectionHelper.MatchesPluginName(name).Should().BeTrue();

    [Theory]
    [InlineData("MyPlugin")]
    [InlineData("PluginStep")]
    [InlineData("CustomApi")]
    [InlineData("Plugins")]
    public void MatchesPluginName_Should_Not_Match_Other_Names(string name)
        => PluginDetectionHelper.MatchesPluginName(name).Should().BeFalse();

    [Fact]
    public void HasPluginAttribute_Should_Detect_Plugin_Attribute_On_Class()
    {
        const string source = """
            namespace TestPlugins;
            [Plugin]
            public class MyPlugin { }
            """;
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();

        PluginDetectionHelper.HasPluginAttribute(root).Should().BeTrue();
    }

    [Fact]
    public void HasPluginAttribute_Should_Detect_Qualified_And_Argumented_Plugin_Attribute()
    {
        const string source = """
            namespace TestPlugins;
            [XrmTools.Meta.Attributes.Plugin(Name = "Foo")]
            public class MyPlugin { }
            """;
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();

        PluginDetectionHelper.HasPluginAttribute(root).Should().BeTrue();
    }

    [Fact]
    public void HasPluginAttribute_Should_Return_False_Without_Plugin_Attribute()
    {
        const string source = """
            namespace TestPlugins;
            using System;
            [Serializable]
            public class NotAPlugin { }
            """;
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();

        PluginDetectionHelper.HasPluginAttribute(root).Should().BeFalse();
    }
}
