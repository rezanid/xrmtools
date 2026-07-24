namespace XrmTools.Tests.Analyzers;

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Meta.Model;
using Xunit;

public class AssemblyPluginTypeEnumerationTests
{
    [Fact]
    public async Task ParseProjectPluginsAsync_Should_Include_Unannotated_IPlugin_Types()
    {
        var project = CreateProject();
        var service = CreateService();

        var config = await service.ParseProjectPluginsAsync(project);

        config.Should().NotBeNull();
        config!.AssemblyPluginTypeNames.Should().NotBeNull();
        config.AssemblyPluginTypeNames.Should().Contain("TestPlugins.AnnotatedPlugin");
        config.AssemblyPluginTypeNames.Should().Contain("TestPlugins.UnannotatedPlugin");
        config.AssemblyPluginTypeNames.Should().Contain("TestPlugins.DerivedPlugin");
        config.AssemblyPluginTypeNames.Should().NotContain("TestPlugins.NotAPlugin");
        config.AssemblyPluginTypeNames.Should().NotContain("TestPlugins.PluginBase");
    }

    [Fact]
    public async Task ParsePluginsAsync_Should_Include_All_Assembly_IPlugin_Types()
    {
        var project = CreateProject();
        var document = project.Documents.Single(d => d.Name == "AnnotatedPlugin.cs");
        var service = CreateService();

        var config = await service.ParsePluginsAsync(document);

        config.Should().NotBeNull();
        config!.AssemblyPluginTypeNames.Should().NotBeNull();
        config.AssemblyPluginTypeNames.Should().Contain("TestPlugins.AnnotatedPlugin");
        config.AssemblyPluginTypeNames.Should().Contain("TestPlugins.UnannotatedPlugin");
    }

    private static CSharpXrmMetaDataService CreateService()
        => new(new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation()));

    private static Project CreateProject()
    {
        var workspace = CreateWorkspace();
        var project = workspace.AddProject("PluginTypeEnumerationProject", LanguageNames.CSharp)
            .AddMetadataReferences(GetMetadataReferences());

        project = project.AddDocument("Sdk.cs", SdkSource, filePath: Path.Combine("C:\\", "Tests", "Sdk.cs")).Project;
        project = project.AddDocument("Attributes.cs", AttributeSource, filePath: Path.Combine("C:\\", "Tests", "Attributes.cs")).Project;
        project = project.AddDocument("AnnotatedPlugin.cs", AnnotatedPluginSource, filePath: Path.Combine("C:\\", "Tests", "AnnotatedPlugin.cs")).Project;
        project = project.AddDocument("OtherPlugins.cs", OtherPluginsSource, filePath: Path.Combine("C:\\", "Tests", "OtherPlugins.cs")).Project;

        return project;
    }

    private static AdhocWorkspace CreateWorkspace()
    {
        var host = MefHostServices.Create(
            MefHostServices.DefaultAssemblies.Concat(
            [
                typeof(CSharpCompilation).Assembly,
                typeof(CSharpFormattingOptions).Assembly,
            ]));

        return new AdhocWorkspace(host);
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
        =>
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
        ];

    private const string SdkSource = @"namespace Microsoft.Xrm.Sdk;

using System;

public interface IServiceProvider2 { }

public interface IPlugin
{
    void Execute(System.IServiceProvider serviceProvider);
}";

    private const string AttributeSource = @"namespace XrmTools.Meta.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class PluginAttribute : Attribute
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowActivityGroupName { get; set; } = string.Empty;
}";

    private const string AnnotatedPluginSource = @"namespace TestPlugins;

using Microsoft.Xrm.Sdk;
using XrmTools.Meta.Attributes;

[Plugin(Name = ""AnnotatedPlugin"")]
public sealed class AnnotatedPlugin : IPlugin
{
    public void Execute(System.IServiceProvider serviceProvider) { }
}";

    private const string OtherPluginsSource = @"namespace TestPlugins;

using Microsoft.Xrm.Sdk;

public sealed class UnannotatedPlugin : IPlugin
{
    public void Execute(System.IServiceProvider serviceProvider) { }
}

public abstract class PluginBase : IPlugin
{
    public abstract void Execute(System.IServiceProvider serviceProvider);
}

public sealed class DerivedPlugin : PluginBase
{
    public override void Execute(System.IServiceProvider serviceProvider) { }
}

public sealed class NotAPlugin
{
}";
}
