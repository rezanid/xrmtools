#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
    private const string Source = """
        using System;
        using System.Collections.Generic;
        using Microsoft.Xrm.Sdk;
        using XrmTools.Meta.Attributes;
        namespace XrmTools.Meta.Attributes {
          public class PluginAttribute : Attribute {}
          public class CustomApiAttribute(string uniqueName) : Attribute {}
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
