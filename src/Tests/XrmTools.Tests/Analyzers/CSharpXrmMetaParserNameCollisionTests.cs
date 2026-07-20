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

public class CSharpXrmMetaParserNameCollisionTests
{
    [Fact]
    public async Task ParsePluginAssemblyConfig_Should_Populate_NameCollisionSuffix_From_Assembly_Attribute()
    {
        var project = CreateProject(AssemblyInfoWithSuffix);
        var compilation = await project.GetCompilationAsync();
        compilation.Should().NotBeNull();

        var parser = new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation());
        var config = parser.ParsePluginAssemblyConfig(compilation!);

        config.Should().NotBeNull();
        config!.NameCollision.Should().NotBeNull();
        config.NameCollision.Suffix.Should().Be("Attribute");
    }

    [Fact]
    public async Task ParsePluginAssemblyConfig_Should_DefaultNameCollisionSuffix_When_Attribute_Absent()
    {
        var project = CreateProject(AssemblyInfoWithoutSuffix);
        var compilation = await project.GetCompilationAsync();
        compilation.Should().NotBeNull();

        var parser = new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation());
        var config = parser.ParsePluginAssemblyConfig(compilation!);

        config.Should().NotBeNull();
        config!.NameCollision.Suffix.Should().Be(CodeGenNameCollisionConfig.DefaultSuffix);
    }

    private static Project CreateProject(string assemblyInfoSource)
    {
        var workspace = CreateWorkspace();
        var project = workspace.AddProject("NameCollisionProject", LanguageNames.CSharp)
            .AddMetadataReferences(GetMetadataReferences());

        project = project.AddDocument("Attributes.cs", AttributeSource, filePath: Path.Combine("C:\\", "Tests", "Attributes.cs")).Project;
        project = project.AddDocument("AssemblyInfo.cs", assemblyInfoSource, filePath: Path.Combine("C:\\", "Tests", "AssemblyInfo.cs")).Project;

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

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
internal sealed class PluginAssemblyAttribute : Attribute
{
    public int SourceType { get; set; }
    public int IsolationMode { get; set; }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
internal sealed class CodeGenNameCollisionSuffixAttribute : Attribute
{
    public string Suffix { get; set; }

    public CodeGenNameCollisionSuffixAttribute(string suffix)
    {
        Suffix = suffix;
    }
}";

    private const string AssemblyInfoWithSuffix = @"using XrmTools.Meta.Attributes;

[assembly: PluginAssembly]
[assembly: CodeGenNameCollisionSuffix(""Attribute"")]";

    private const string AssemblyInfoWithoutSuffix = @"using XrmTools.Meta.Attributes;

[assembly: PluginAssembly]";
}
