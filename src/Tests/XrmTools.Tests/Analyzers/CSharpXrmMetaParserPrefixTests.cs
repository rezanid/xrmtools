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

public class CSharpXrmMetaParserPrefixTests
{
    [Fact]
    public async Task ParsePluginAssemblyConfig_Should_Populate_ReplacePrefixes_From_Assembly_Attribute()
    {
        var project = CreateProjectWithPrefixAttribute();
        var compilation = await project.GetCompilationAsync();
        compilation.Should().NotBeNull();

        var parser = new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation());
        var config = parser.ParsePluginAssemblyConfig(compilation!);

        config.Should().NotBeNull();
        config!.ReplacePrefixes.Should().NotBeEmpty();
        config.ReplacePrefixes[0].PrefixList.Should().Contain("ber_");
        config.ReplacePrefixes[0].PrefixList.Should().Contain("ber_dt_");
    }

    private static Project CreateProjectWithPrefixAttribute()
    {
        var workspace = CreateWorkspace();
        var project = workspace.AddProject("PrefixProject", LanguageNames.CSharp)
            .AddMetadataReferences(GetMetadataReferences());

        project = project.AddDocument("Attributes.cs", AttributeSource, filePath: Path.Combine("C:\\", "Tests", "Attributes.cs")).Project;
        project = project.AddDocument("AssemblyInfo.cs", AssemblyInfoSource, filePath: Path.Combine("C:\\", "Tests", "AssemblyInfo.cs")).Project;

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

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
internal sealed class CodeGenReplacePrefixesAttribute : Attribute
{
    public string Prefixes { get; set; }
    public string ReplaceWith { get; set; }

    public CodeGenReplacePrefixesAttribute(string prefixes) : this(prefixes, string.Empty) { }

    public CodeGenReplacePrefixesAttribute(string prefixes, string replaceWith)
    {
        Prefixes = prefixes;
        ReplaceWith = replaceWith;
    }
}";

    private const string AssemblyInfoSource = @"using XrmTools.Meta.Attributes;

[assembly: PluginAssembly]
[assembly: CodeGenReplacePrefixes(""ber_dt_,ber_"")]";
}
