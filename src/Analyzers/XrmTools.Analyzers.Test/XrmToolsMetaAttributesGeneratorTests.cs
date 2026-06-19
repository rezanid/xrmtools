namespace XrmTools.Analyzers.Test;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;
using XrmTools.Meta.Attributes.SourceGenerator;

public class XrmToolsMetaAttributesGeneratorTests
{
    [Fact]
    public void GeneratedTypes_AreInternalByDefault()
    {
        var compilation = RunGenerator(CreateCompilation("public sealed class Consumer { }"));

        var pluginAttribute = compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.PluginAttribute");
        var stages = compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.Stages");

        Assert.NotNull(pluginAttribute);
        Assert.NotNull(stages);
        Assert.Equal(Accessibility.Internal, pluginAttribute!.DeclaredAccessibility);
        Assert.Equal(Accessibility.Internal, stages!.DeclaredAccessibility);
    }

    [Fact]
    public void GeneratedTypes_CanBePublic_WhenEnabled()
    {
        var compilation = RunGenerator(CreateCompilation("public sealed class Consumer { }"), usePublicAccessibility: true);

        var pluginAttribute = compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.PluginAttribute");
        var stages = compilation.GetTypeByMetadataName("XrmTools.Meta.Attributes.Stages");

        Assert.NotNull(pluginAttribute);
        Assert.NotNull(stages);
        Assert.Equal(Accessibility.Public, pluginAttribute!.DeclaredAccessibility);
        Assert.Equal(Accessibility.Public, stages!.DeclaredAccessibility);
    }

    [Fact]
    public void DependencyScope_IsOnlyGenerated_WhenXrmSdkIsReferenced()
    {
        var withoutXrmSdk = RunGenerator(CreateCompilation("public sealed class Consumer { }"));
        var withXrmSdk = RunGenerator(
            CreateCompilation("public sealed class Consumer { }", CreateXrmSdkMetadataReference()));

        Assert.Null(withoutXrmSdk.GetTypeByMetadataName("XrmTools.DependencyScope`1"));
        Assert.NotNull(withXrmSdk.GetTypeByMetadataName("XrmTools.DependencyScope`1"));
    }

    [Fact]
    public void GeneratedSources_Compile_WithNullableEnabled()
    {
        var compilation = RunGenerator(CreateCompilation("#nullable enable\npublic sealed class Consumer { }"));

        AssertNoErrors(compilation);
    }

    [Fact]
    public void GeneratedSources_Compile_WithNullableDisabled()
    {
        var compilation = RunGenerator(CreateCompilation("#nullable disable\npublic sealed class Consumer { }"));

        AssertNoErrors(compilation);
    }

    private static CSharpCompilation CreateCompilation(string source, MetadataReference? additionalReference = null)
    {
        List<MetadataReference> references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
            .ToList();

        if (additionalReference is not null)
        {
            references.Add(additionalReference);
        }

        return CSharpCompilation.Create(
            assemblyName: "GeneratorConsumer",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview))],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static CSharpCompilation RunGenerator(CSharpCompilation compilation, bool usePublicAccessibility = false)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new XrmToolsMetaAttributesGenerator().AsSourceGenerator()],
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.First().Options,
            optionsProvider: new TestAnalyzerConfigOptionsProvider(usePublicAccessibility));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        Assert.DoesNotContain(generatorDiagnostics, static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        return (CSharpCompilation)outputCompilation;
    }

    private static MetadataReference CreateXrmSdkMetadataReference()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "Microsoft.Xrm.Sdk",
            syntaxTrees: [CSharpSyntaxTree.ParseText("namespace Microsoft.Xrm.Sdk { public interface IPlugin { } }")],
            references: ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
                .Split(Path.PathSeparator)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(static path => MetadataReference.CreateFromFile(path)),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        Assert.True(emitResult.Success, string.Join(Environment.NewLine, emitResult.Diagnostics));
        stream.Position = 0;

        return MetadataReference.CreateFromStream(stream);
    }

    private static void AssertNoErrors(CSharpCompilation compilation)
    {
        var diagnostics = compilation.GetDiagnostics();
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    private sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private static readonly AnalyzerConfigOptions Empty = new TestAnalyzerConfigOptions(ImmutableDictionary<string, string>.Empty);
        private readonly AnalyzerConfigOptions _globalOptions;

        public TestAnalyzerConfigOptionsProvider(bool usePublicAccessibility)
        {
            _globalOptions = new TestAnalyzerConfigOptions(
                usePublicAccessibility
                    ? ImmutableDictionary<string, string>.Empty.Add("build_property.XrmToolsMetaAttributesUsePublicAccessibility", bool.TrueString)
                    : ImmutableDictionary<string, string>.Empty);
        }

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;
    }

    private sealed class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly ImmutableDictionary<string, string> _values;

        public TestAnalyzerConfigOptions(ImmutableDictionary<string, string> values)
        {
            _values = values;
        }

        public override bool TryGetValue(string key, out string value) => _values.TryGetValue(key, out value!);
    }
}
