namespace XrmTools.Tests.Analyzers;

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XrmTools.Analyzers;
using XrmTools.Meta.Model;
using Xunit;

public class CSharpXrmMetaDataServiceTests
{
    [Fact]
    public async Task ParseProjectPluginsAsync_Should_Throw_When_CustomApi_UniqueNames_Are_Duplicated()
    {
        var project = CreateProjectWithDuplicateCustomApiUniqueNames();
        var service = CreateService();

        Func<Task> act = () => service.ParseProjectPluginsAsync(project);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Duplicate Custom API unique names were found*'contoso.duplicateapi'*TestPlugins.FirstPlugin*TestPlugins.SecondPlugin*");
    }

    [Fact]
    public async Task ParsePluginsAsync_Should_Throw_When_CustomApi_UniqueName_Duplicates_Exist_In_Project()
    {
        var project = CreateProjectWithDuplicateCustomApiUniqueNames();
        var document = project.Documents.Single(d => d.Name == "FirstPlugin.cs");
        var service = CreateService();

        Func<Task> act = () => service.ParsePluginsAsync(document);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Duplicate Custom API unique names were found*'contoso.duplicateapi'*TestPlugins.FirstPlugin*TestPlugins.SecondPlugin*");
    }

    private static CSharpXrmMetaDataService CreateService()
        => new(new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation()));

    private static Project CreateProjectWithDuplicateCustomApiUniqueNames()
    {
        var workspace = CreateWorkspace();
        var project = workspace.AddProject("DuplicateCustomApiProject", LanguageNames.CSharp)
            .AddMetadataReferences(GetMetadataReferences());

        project = project.AddDocument("Attributes.cs", AttributeSource, filePath: Path.Combine("C:\\", "Tests", "Attributes.cs")).Project;
        project = project.AddDocument("FirstPlugin.cs", FirstPluginSource, filePath: Path.Combine("C:\\", "Tests", "FirstPlugin.cs")).Project;
        project = project.AddDocument("SecondPlugin.cs", SecondPluginSource, filePath: Path.Combine("C:\\", "Tests", "SecondPlugin.cs")).Project;

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

    private const string AttributeSource = @"namespace XrmTools.Meta.Attributes;

using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class CustomApiAttribute : Attribute
{
    public string DisplayName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UniqueName { get; }

    public CustomApiAttribute(string uniqueName)
    {
        UniqueName = uniqueName;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
internal sealed class PluginAttribute : Attribute
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string WorkflowActivityGroupName { get; set; } = string.Empty;
}";

    private const string FirstPluginSource = @"namespace TestPlugins;

using XrmTools.Meta.Attributes;

[Plugin(Name = ""FirstPlugin"")]
[CustomApi(""contoso.duplicateapi"", Name = ""duplicateapi1"")]
internal sealed class FirstPlugin
{
}";

    private const string SecondPluginSource = @"namespace TestPlugins;

using XrmTools.Meta.Attributes;

[Plugin(Name = ""SecondPlugin"")]
[CustomApi(""contoso.duplicateapi"", Name = ""duplicateapi2"")]
internal sealed class SecondPlugin
{
}";
}
