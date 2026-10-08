#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;

public sealed class OpenApiSchemaVersionTests
{
    [Theory]
    [InlineData(OpenApiSpecificationVersion.V3_0)]
    [InlineData(OpenApiSpecificationVersion.V3_2)]
    public void RichReferencesKeepLocalDescriptionsAndRecursiveComponents(OpenApiSpecificationVersion version)
    {
        var source = new CustomApiSourceSchemas();
        source.Inputs["Payload"] = Reference();
        source.Components["Child"] = new JObject { ["type"] = "object", ["properties"] = new JObject { ["next"] = Reference() } };
        var api = new CustomApi { UniqueName = "new_Test", RequestParameters = [new CustomApiRequestParameter
            { UniqueName = "Payload", Type = CustomApiFieldType.Entity, Description = "Local description" }] };
        var doc = CustomApiOpenApiWriter.Build(api, null, new Dictionary<string, EntityMetadata>(), source, version);
        var input = doc["paths"]!["/new_Test"]!["post"]!["requestBody"]!["content"]!["application/json"]!["schema"]!["properties"]!["Payload"]!;
        if (version == OpenApiSpecificationVersion.V3_2)
        {
            Assert.Equal("#/components/schemas/Child", (string?)input["$ref"]);
            Assert.Equal("Local description", (string?)input["description"]);
            Assert.Null(input["allOf"]);
        }
        else
        {
            Assert.Equal(2, ((JArray)input["allOf"]!).Count);
            Assert.Equal("Local description", (string?)input["allOf"]![1]!["description"]);
        }
        Assert.Equal("#/components/schemas/Child", (string?)doc["components"]!["schemas"]!["Child"]!["properties"]!["next"]!["$ref"]);
        Assert.NotNull(source.Inputs["Payload"]["allOf"]); // Reusable source model stays unchanged.
    }

    [Fact]
    public void LatestSchemasAnnotateEnumsIncludingExplicitNullableValuesWithoutChangingPayloads()
    {
        var payload = JObject.Parse("{\"allOf\":[{\"$ref\":\"literal\"}],\"example\":42,\"nullable\":true}");
        var choice = new JObject { ["type"] = "integer", ["nullable"] = true, ["enum"] = new JArray(1, 2),
            ["x-enumNames"] = new JArray("First", "Second"), ["x-enumDescriptions"] = new JObject { ["1"] = "First value", ["2"] = "Second value" } };
        var schema = new JObject { ["type"] = "object", ["required"] = new JArray("choice"), ["example"] = payload.DeepClone(),
            ["properties"] = new JObject { ["choice"] = choice, ["example"] = new JObject { ["type"] = "string" } } };
        var doc = new JObject { ["components"] = new JObject { ["schemas"] = new JObject { ["Model"] = schema } } };
        OpenApiSchemaVersion.Apply(doc, OpenApiSpecificationVersion.V3_2);
        Assert.Null(schema["example"]);
        Assert.True(JToken.DeepEquals(payload, schema["examples"]![0]));
        Assert.NotNull(schema["properties"]!["example"]);
        Assert.Equal(new[] { "integer", "null" }, choice["type"]!.ToObject<string[]>()!);
        Assert.Null(choice["nullable"]);
        Assert.Equal(3, ((JArray)choice["oneOf"]!).Count);
        Assert.Equal(1, (int)choice["oneOf"]![0]!["const"]!);
        Assert.Equal("First", (string?)choice["oneOf"]![0]!["title"]);
        Assert.Equal("First value", (string?)choice["oneOf"]![0]!["description"]);
        Assert.Equal(JTokenType.Null, choice["oneOf"]![2]!["const"]!.Type);
        Assert.Equal(new[] { "choice" }, schema["required"]!.ToObject<string[]>()!);
    }

    [Fact]
    public void LatestFunctionExamplesMatchWireSchemasAndHttpSampleForEveryInputType()
    {
        var api = new CustomApi { UniqueName = "new_Test", IsFunction = true,
            ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.String }] };
        foreach (CustomApiFieldType type in Enum.GetValues(typeof(CustomApiFieldType)))
            api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "P_" + type, Type = type, LogicalEntityName = "account", IsOptional = true });
        var doc = CustomApiOpenApiWriter.Build(api, null, new Dictionary<string, EntityMetadata> { ["account"] = new EntityMetadata("account", "accounts") }, version: OpenApiSpecificationVersion.V3_2);
        var operation = ((JObject)doc["paths"]!).Properties().Single().Value["get"]!;
        var sample = (string)operation["x-codeSamples"]![0]!["source"]!;
        foreach (var parameter in operation["parameters"]!.OfType<JObject>())
        {
            Assert.Null(parameter["example"]);
            Assert.False((bool)parameter["required"]!);
            foreach (var example in ((JObject)parameter["examples"]!).Properties())
            {
                var literal = (string)example.Value["dataValue"]!;
                var assignment = (string)example.Value["serializedValue"]!;
                Assert.Null(example.Value["value"]);
                Assert.Null(example.Value["x-odata-serialized-value"]);
                Assert.Equal(literal, Uri.UnescapeDataString(assignment.Substring(assignment.IndexOf('=') + 1)));
                if (parameter["schema"]!["pattern"] is JValue pattern) Assert.Matches((string)pattern!, literal);
            }
            Assert.Contains(ODataFunctionParameters.Assignment(parameter), sample);
            Assert.Equal("null", (string?)parameter["examples"]!["null"]!["dataValue"]);
        }
        var stringParameter = operation["parameters"]!.Single(p => (string?)p["name"] == "@P_String");
        Assert.Equal("''", (string?)stringParameter["examples"]!["emptyString"]!["dataValue"]);
        Assert.Equal("3.2.1", (string?)doc["openapi"]);
    }

    [Fact]
    public void UnknownVersionsAreRejected() => Assert.Throws<ArgumentOutOfRangeException>(() =>
        CustomApiOpenApiWriter.Build(new CustomApi { UniqueName = "new_Test" }, null, new Dictionary<string, EntityMetadata>(), version: (OpenApiSpecificationVersion)999));

    private static JObject Reference() => new() { ["allOf"] = new JArray(new JObject { ["$ref"] = "#/components/schemas/Child" }) };
}
