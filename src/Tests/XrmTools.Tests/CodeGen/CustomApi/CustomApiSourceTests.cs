#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Moq;
using System.Threading.Tasks;
using System.Windows.Threading;
using XrmTools.CodeRefactoringProviders;
using XrmTools.Environments;
using XrmTools.WebApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using XrmTools.Analyzers;
using XrmTools.Meta.Model;
using XrmTools.CodeGen.CustomApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

public sealed class CustomApiSourceTests
{
    private static IOpenApiOutputSettings OutputSettings(OpenApiOutputFormat format = OpenApiOutputFormat.Json)
    {
        var settings = new Mock<IOpenApiOutputSettings>();
        settings.Setup(s => s.GetFormatAsync()).ReturnsAsync(format);
        return settings.Object;
    }
    private const string Source = """
        using System;
        using System.Collections.Generic;
        using Microsoft.Xrm.Sdk;
        using XrmTools.Meta.Attributes;
        namespace XrmTools.Meta.Attributes {
          public class PluginAttribute : Attribute {}
          public class CustomApiAttribute(string uniqueName) : Attribute { public bool IsFunction { get;set; } }
          public class CustomApiRequestAttribute : Attribute {}
          public class CustomApiResponseAttribute : Attribute {}
          public class CustomApiRequestParameterAttribute : Attribute { public string UniqueName {get;set;} public string Description {get;set;} }
        }
        namespace Microsoft.Xrm.Sdk {
          public class AttributeLogicalNameAttribute(string name) : Attribute {}
          public class Entity {
            protected T GetAttributeValue<T>(string name) => default;
            protected void SetAttributeValue(string name, object value) {}
            public object Attributes {get;set;}
          }
        }
        [Plugin, CustomApi("new_Test")] public class Plugin {
          [CustomApiRequest] public class Request {
            [CustomApiRequestParameter(UniqueName="Payload", Description="Source docs")]
            public Child Data {get;set;}
            public IEnumerable<Child> Items {get;set;}
            public Choice? Choice {get;set;}
            public string Undeployed {get;set;}
          }
          [CustomApiResponse] public class Response { public Child Result {get;set;} }
        }
        public enum Choice {
          /// <summary>The first choice.</summary>
          First=1,
          Second=2,
          SecondAlias=2
        }
        public class Parent : Entity {
          [AttributeLogicalName("inherited")] public string Inherited {get;set;}
        }
        /// <summary>Child documentation.</summary>
        public class Child : Parent {
          const string Key = "custom_field";
          public string CustomField { get => GetAttributeValue<string>(Key); set => SetAttributeValue(Key,value); }
          [AttributeLogicalName("next")] public Child Next {get;set;}
          public string Computed => "unmapped";
          [AttributeLogicalName("identifier")] public Guid Identifier {get;set;}
        }
        """;

    private static (Api Api, INamedTypeSymbol Plugin, Compilation Compilation) Fixture()
    {
        var compilation = CSharpCompilation.Create("Plugins", [CSharpSyntaxTree.ParseText(Source)],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.Empty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        var api = new Api { UniqueName = "new_Test" };
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Payload", Type = CustomApiFieldType.Entity });
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Items", Type = CustomApiFieldType.EntityCollection });
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Choice", Type = CustomApiFieldType.Picklist, IsOptional = true });
        api.ResponseProperties.Add(new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.Entity });
        return (api, compilation.GetTypeByMetadataName("Plugin")!, compilation);
    }

    [Fact]
    public void EntityAccessorsInReferencedProjectsUseTheirOwningCompilation()
    {
        var (_, _, shared) = Fixture();
        var consumerTree = CSharpSyntaxTree.ParseText("""
            using XrmTools.Meta.Attributes;
            [Plugin, CustomApi("new_Consumer", IsFunction=true)]
            public class Consumer {
                [CustomApiResponse] public class Response { public Child Result {get;set;} }
            }
            """);
        var consumer = CSharpCompilation.Create("Consumer", [consumerTree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location), shared.ToMetadataReference()],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.Empty(consumer.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.False(consumer.ContainsSyntaxTree(shared.SyntaxTrees.Single()));
        var api = new Api { UniqueName = "new_Consumer", IsFunction = true,
            ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.Entity }] };
        var rich = CustomApiSourceReader.Enrich(api, api, consumer.GetTypeByMetadataName("Consumer")!, consumer, TestContext.Current.CancellationToken);
        var schema = rich.Components.Properties().Single().Value;
        Assert.NotNull(schema["properties"]!["custom_field"]);
        Assert.NotNull(schema["properties"]!["inherited"]);
        Assert.DoesNotContain(rich.Diagnostics, d => d.Contains("CustomField"));
        var spec = JObject.Parse(CustomApiOpenApiWriter.Write(api, null, new Dictionary<string, EntityMetadata>(), rich));
        Assert.NotNull(spec["paths"]!["/new_Consumer()"]!["get"]);
    }

    [Fact]
    public void TypedEntitiesCollectionsInheritanceAndCyclesShareComponents()
    {
        var (api, plugin, compilation) = Fixture();
        var rich = CustomApiSourceReader.Enrich(api, api, plugin, compilation, CancellationToken.None);
        var component = Assert.Single(rich.Components.Properties()).Value;
        Assert.Equal("Child documentation.", (string?)component["description"]);
        Assert.NotNull(component["properties"]!["custom_field"]);
        Assert.NotNull(component["properties"]!["inherited"]);
        Assert.Null(component["properties"]!["Attributes"]);
        Assert.Null(component["properties"]!["Computed"]);
        Assert.Null(component["required"]);
        var reference = (string?)rich.Inputs["Payload"]["allOf"]![0]!["$ref"];
        Assert.Equal(reference, (string?)rich.Inputs["Items"]["items"]!["allOf"]![0]!["$ref"]);
        Assert.Equal(reference, (string?)component["properties"]!["next"]!["allOf"]![0]!["$ref"]);
        Assert.Equal(reference, (string?)rich.Outputs["Result"]["allOf"]![0]!["$ref"]);
        Assert.Equal(new[] { 1, 2 }, rich.Inputs["Choice"]["enum"]!.ToObject<int[]>()!);
        Assert.Equal(new[] { "First", "Second" }, rich.Inputs["Choice"]["x-enumNames"]!.ToObject<string[]>()!);
        Assert.Equal("First: The first choice.", (string?)rich.Inputs["Choice"]["x-enumDescriptions"]!["1"]);
        Assert.Equal("Second; SecondAlias", (string?)rich.Inputs["Choice"]["x-enumDescriptions"]!["2"]);
        Assert.Contains(rich.Diagnostics, d => d.Contains("Computed"));
        Assert.Contains(rich.Diagnostics, d => d.Contains("Undeployed"));
        var doc = JObject.Parse(CustomApiOpenApiWriter.Write(api, new Uri("https://example/api/data/v9.2/"), new Dictionary<string, EntityMetadata>(), rich));
        Assert.NotNull(doc["components"]!["schemas"]!["DataverseError"]);
        Assert.NotNull(doc["x-xrmtools-diagnostics"]);
        var inputs = doc["paths"]!["/new_Test"]!["post"]!["requestBody"]!["content"]!["application/json"]!["schema"]!["properties"]!;
        Assert.Equal("Source docs", (string?)inputs["Payload"]!["allOf"]![1]!["description"]);
        Assert.Equal(reference, (string?)inputs["Items"]!["items"]!["$ref"]);
        Assert.Null(inputs["Items"]!["items"]!["allOf"]);
        Assert.Equal(reference, (string?)doc["components"]!["schemas"]![rich.Components.Properties().Single().Name]!["properties"]!["next"]!["$ref"]);
        Assert.DoesNotContain(doc.Descendants().OfType<JProperty>(), p => p.Name == "allOf" && p.Value is JArray a && a.Count < 2);
    }

    [Fact]
    public async Task PresentationIsDeferredOutsideTheCodeActionExecutionContext()
    {
        var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;
                var ambient = new AsyncLocal<string?> { Value = "code-action-scope" };
                var called = false;
                var frame = new DispatcherFrame();
                CustomApiOpenApiRefactoringProvider.QueueAfterApply(dispatcher, () =>
                {
                    try { Assert.Null(ambient.Value); called = true; }
                    catch (Exception ex) { completion.TrySetException(ex); }
                    finally { frame.Continue = false; }
                });
                Assert.False(called);
                ambient.Value = null;
                Dispatcher.PushFrame(frame);
                Assert.True(called);
                completion.TrySetResult(true);
            }
            catch (Exception ex) { completion.TrySetException(ex); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        await completion.Task;
    }

    [Fact]
    public async Task SourceActionIsAvailableOnClassAndApiAttributeWithoutPreviewSideEffects()
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("Plugins", LanguageNames.CSharp)
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        var document = project.AddDocument("Api.cs", SourceText.From(Source));
        foreach (var position in new[] { Source.IndexOf("class Plugin {", StringComparison.Ordinal) + 6, Source.IndexOf("CustomApi(\"new_Test\")", StringComparison.Ordinal) })
        {
            var actions = new List<CodeAction>();
            await new CustomApiOpenApiRefactoringProvider().ComputeRefactoringsAsync(new CodeRefactoringContext(document,
                new TextSpan(position, 0), actions.Add, TestContext.Current.CancellationToken));
            var action = Assert.Single(actions);
            Assert.Equal("Generate OpenAPI specification from source", action.Title);
            Assert.Empty(await action.GetPreviewOperationsAsync(TestContext.Current.CancellationToken));
            var operation = Assert.Single(await action.GetOperationsAsync(TestContext.Current.CancellationToken));
            Assert.IsNotType<ApplyChangesOperation>(operation);
        }
        var unrelated = new List<CodeAction>();
        await new CustomApiOpenApiRefactoringProvider().ComputeRefactoringsAsync(new CodeRefactoringContext(document,
            new TextSpan(Source.IndexOf("class Child", StringComparison.Ordinal) + 6, 0), unrelated.Add, TestContext.Current.CancellationToken));
        Assert.Empty(unrelated);
    }

    [Theory]
    [InlineData(OpenApiOutputFormat.Json)]
    [InlineData(OpenApiOutputFormat.Yaml)]
    public async Task UndeployedFunctionGeneratesFromSourceWithoutNetwork(OpenApiOutputFormat format)
    {
        var source = Source.Replace("CustomApi(\"new_Test\")", "CustomApi(\"new_Test\", IsFunction=true)")
            .Replace("public Child Data", "public string Data")
            .Replace("public IEnumerable<Child> Items", "public string[] Items");
        using var workspace = new AdhocWorkspace();
        var document = workspace.AddProject("Plugins", LanguageNames.CSharp)
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddDocument("Api.cs", SourceText.From(source));
        var root = await document.GetSyntaxRootAsync(TestContext.Current.CancellationToken);
        var declaration = root!.DescendantNodes().OfType<ClassDeclarationSyntax>().Single(c => c.Identifier.ValueText == "Plugin");
        var environments = new Mock<IEnvironmentSelection>(MockBehavior.Strict);
        environments.Setup(e => e.GetSelectedEnvironmentAsync()).ReturnsAsync((DataverseEnvironment?)null);
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        var generator = new CustomApiSourceGenerator(new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation()), environments.Object, webApi.Object, OutputSettings(format));
        var result = await generator.GenerateAsync(document, declaration.Span, TestContext.Current.CancellationToken);
        Assert.Equal("new_Test.openapi." + (format == OpenApiOutputFormat.Json ? "json" : "yaml"), result.FileName);
        var spec = format == OpenApiOutputFormat.Json ? JObject.Parse(result.Content)
            : JObject.FromObject(new YamlDotNet.Serialization.DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build().Deserialize<object>(result.Content));
        var path = ((JObject)spec["paths"]!).Properties().Single();
        Assert.Equal("/new_Test(Choice=@Choice,Items=@Items,Payload=@Payload,Undeployed=@Undeployed)", path.Name);
        var operation = path.Value["get"]!;
        Assert.Null(operation["requestBody"]);
        Assert.NotNull(operation["responses"]!["200"]);
        Assert.NotNull(spec["components"]!["schemas"]!["Source_Child"]);
        Assert.Empty(webApi.Invocations);
    }

    [Fact]
    public async Task UndeployedGlobalApiGeneratesOfflineWithoutNetworkOrEnvironmentPrompts()
    {
        using var workspace = new AdhocWorkspace();
        var document = workspace.AddProject("Plugins", LanguageNames.CSharp)
            .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddDocument("Api.cs", SourceText.From(Source));
        var root = await document.GetSyntaxRootAsync(TestContext.Current.CancellationToken);
        var declaration = root!.DescendantNodes().OfType<ClassDeclarationSyntax>().Single(c => c.Identifier.ValueText == "Plugin");
        var environments = new Mock<IEnvironmentSelection>(MockBehavior.Strict);
        environments.Setup(e => e.GetSelectedEnvironmentAsync()).ReturnsAsync((DataverseEnvironment?)null);
        var webApi = new Mock<IWebApiService>(MockBehavior.Strict);
        var generator = new CustomApiSourceGenerator(new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation()), environments.Object, webApi.Object, OutputSettings());
        var result = await generator.GenerateAsync(document, declaration.Span, TestContext.Current.CancellationToken);
        var spec = JObject.Parse(result.Content);
        Assert.Equal("new_Test.openapi.json", result.FileName);
        var properties = spec["paths"]!["/new_Test"]!["post"]!["requestBody"]!["content"]!["application/json"]!["schema"]!["properties"]!;
        Assert.NotNull(properties["Undeployed"]);
        Assert.NotNull(properties["Payload"]);
        Assert.Equal("YOUR-ORGANIZATION", (string?)spec["servers"]![0]!["variables"]!["organization"]!["default"]);
        Assert.Empty(webApi.Invocations);
        environments.Verify(e => e.GetSelectedEnvironmentAsync(), Times.Once);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generator.GenerateAsync(document, declaration.Span, new CancellationToken(true)));
        var boundApi = new Api { UniqueName = "new_Test", BindingType = Api.BindingTypes.Entity, BoundEntityLogicalName = "account" };
        var boundParser = new Mock<ICSharpXrmMetaParser>(MockBehavior.Strict);
        boundParser.Setup(p => p.ParsePluginConfig(It.IsAny<INamedTypeSymbol>(), It.IsAny<Compilation>()))
            .Returns(new XrmTools.Meta.Model.Configuration.PluginTypeConfig { CustomApi = boundApi });
        var boundGenerator = new CustomApiSourceGenerator(boundParser.Object, environments.Object, webApi.Object, OutputSettings());
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => boundGenerator.GenerateAsync(document, declaration.Span, TestContext.Current.CancellationToken));
        Assert.Contains("entity set name", error.Message);
        Assert.Empty(webApi.Invocations);
    }

    [Fact]
    public void RegistrationParserAndSourceSchemasAgreeOnTypedFields()
    {
        var (api, plugin, compilation) = Fixture();
        var parser = new CSharpXrmMetaParser(new CSharpDependencyAnalyzer(), new DependencyPreparation());
        var source = parser.ParsePluginConfig(plugin, compilation)!.CustomApi!;
        Assert.Equal("new_Test", source.UniqueName);
        var rich = CustomApiSourceReader.Enrich(api, source, plugin, compilation, CancellationToken.None);
        Assert.NotNull(rich.Inputs["Payload"]);
        Assert.NotNull(rich.Inputs["Items"]);
        Assert.NotNull(rich.Inputs["Choice"]);
        Assert.NotNull(rich.Outputs["Result"]);
        Assert.DoesNotContain(rich.Diagnostics, d => d.Contains("differs"));
    }

    [Fact]
    public void MismatchedFieldIsNotEnrichedAndCancellationIsHonored()
    {
        var (api, plugin, compilation) = Fixture();
        var deployed = new Api { UniqueName = "new_Test" };
        deployed.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Payload", Type = CustomApiFieldType.String });
        var rich = CustomApiSourceReader.Enrich(deployed, api, plugin, compilation, CancellationToken.None);
        Assert.Empty(rich.Inputs);
        Assert.Contains(rich.Diagnostics, d => d.Contains("differs"));
        Assert.Throws<OperationCanceledException>(() => CustomApiSourceReader.Enrich(api, api, plugin, compilation, new CancellationToken(true)));
    }
}
