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
using Api = XrmTools.WebApi.Entities.CustomApi;

public sealed class MultiApiOpenApiTests
{
    private static JObject Build(params CustomApiOpenApiInput[] inputs) => CustomApiOpenApiWriter.BuildMany(inputs, null, new Dictionary<string, EntityMetadata>());

    [Theory]
    [InlineData(OpenApiSpecificationVersion.V3_0)]
    [InlineData(OpenApiSpecificationVersion.V3_2)]
    public void MixedOperationsShareDocumentSettingsAndAreDeterministic(OpenApiSpecificationVersion version)
    {
        var action = new CustomApiOpenApiInput(new Api { UniqueName = "new_Z", Description = "Action" }, null, "Assembly Z");
        var function = new CustomApiOpenApiInput(new Api { UniqueName = "new_A", IsFunction = true,
            RequestParameters = [new CustomApiRequestParameter { UniqueName = "Name", Type = CustomApiFieldType.String }],
            ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.String }] }, null, "Assembly A");
        var tables = new Dictionary<string, EntityMetadata>();
        var first = CustomApiOpenApiWriter.BuildMany([action, function], null, tables, version);
        var second = CustomApiOpenApiWriter.BuildMany([function, action], null, tables, version);
        Assert.Equal(first.ToString(), second.ToString());
        Assert.Equal(2, ((JObject)first["paths"]!).Count);
        Assert.NotNull(first["paths"]!["/new_A(Name=@Name)"]!["get"]);
        Assert.NotNull(first["paths"]!["/new_Z"]!["post"]);
        Assert.Single((JArray)first["servers"]!);
        Assert.Single((JObject)first["components"]!["schemas"]!);
        Assert.Single((JObject)first["components"]!["securitySchemes"]!);
        Assert.Equal("Assembly A", (string?)first["paths"]!["/new_A(Name=@Name)"]!["get"]!["tags"]![0]);
        Assert.Equal(version == OpenApiSpecificationVersion.V3_0 ? "3.0.4" : "3.2.1", (string?)first["openapi"]);
    }

    [Fact]
    public void IdenticalSourceComponentsAreSharedWithoutMutatingSources()
    {
        var source = Source("string");
        var snapshot = source.Components.ToString();
        var doc = Build(Input("new_A", source), Input("new_B", Source("string")));
        Assert.Equal(3, ((JObject)doc["components"]!["schemas"]!).Count); // Parent, Child, DataverseError
        Assert.NotNull(doc["components"]!["schemas"]!["Parent"]);
        Assert.Equal(snapshot, source.Components.ToString());
    }

    [Fact]
    public void ConflictingChildSchemasNamespaceTheirParentsAndRecursiveReferences()
    {
        var a = Source("string"); var b = Source("integer");
        a.Components["Child"]!["properties"]!["parent"] = Ref("Parent");
        b.Components["Child"]!["properties"]!["parent"] = Ref("Parent");
        a.Components["Parent"]!["example"] = new JObject { ["$ref"] = "#/components/schemas/Child" };
        b.Components["Parent"]!["example"] = new JObject { ["$ref"] = "#/components/schemas/Child" };
        var doc = Build(Input("new_A", a), Input("new_B", b));
        var schemas = doc["components"]!["schemas"]!;
        foreach (var name in new[] { "new_A", "new_B" })
        {
            Assert.Equal("#/components/schemas/" + name + ".Child", (string?)schemas[name + ".Parent"]!["properties"]!["child"]!["$ref"]);
            Assert.Equal("#/components/schemas/" + name + ".Parent", (string?)schemas[name + ".Child"]!["properties"]!["parent"]!["$ref"]);
            Assert.Equal("#/components/schemas/Child", (string?)schemas[name + ".Parent"]!["example"]!["$ref"]);
            var request = doc["paths"]!["/" + name]!["post"]!["requestBody"]!["content"]!["application/json"]!["schema"]!["properties"]!["Payload"]!;
            Assert.Contains("#/components/schemas/" + name + ".Parent", request.ToString());
        }
        Assert.Equal("string", (string?)schemas["new_A.Child"]!["properties"]!["value"]!["type"]);
        Assert.Equal("integer", (string?)schemas["new_B.Child"]!["properties"]!["value"]!["type"]);
    }

    [Fact]
    public void SourceTypeNamedDataverseErrorDoesNotOverwriteTheErrorEnvelope()
    {
        var source = new CustomApiSourceSchemas();
        source.Components["DataverseError"] = new JObject { ["type"] = "string" };
        source.Inputs["Payload"] = new JObject { ["allOf"] = new JArray(Ref("DataverseError")) };
        var doc = Build(Input("new_A", source));
        Assert.NotNull(doc["components"]!["schemas"]!["DataverseError"]!["properties"]!["error"]);
        Assert.Equal("string", (string?)doc["components"]!["schemas"]!["new_A.DataverseError"]!["type"]);
        Assert.Contains("#/components/schemas/new_A.DataverseError", doc["paths"]!.ToString());
    }

    [Fact]
    public void DuplicateOperationsAndInvalidApisFailTheWholeDocument()
    {
        var input = Input("new_A", null);
        Assert.Throws<InvalidOperationException>(() => Build(input, input));
        Assert.Throws<ArgumentException>(() => Build());
        var bad = Input("new_B", null);
        bad.Api.RequestParameters.First().Type = (CustomApiFieldType)999;
        var error = Assert.Throws<InvalidOperationException>(() => Build(input, bad));
        Assert.Contains("new_B", error.Message);
    }

    private static CustomApiOpenApiInput Input(string name, CustomApiSourceSchemas? source) => new(new Api
    {
        UniqueName = name, RequestParameters = [new CustomApiRequestParameter { UniqueName = "Payload", Type = CustomApiFieldType.Entity }],
    }, source, "Assembly");
    private static JObject Ref(string name) => new() { ["$ref"] = "#/components/schemas/" + name };
    private static CustomApiSourceSchemas Source(string type)
    {
        var source = new CustomApiSourceSchemas();
        source.Components["Parent"] = new JObject { ["type"] = "object", ["properties"] = new JObject { ["child"] = Ref("Child") } };
        source.Components["Child"] = new JObject { ["type"] = "object", ["properties"] = new JObject { ["value"] = new JObject { ["type"] = type } } };
        source.Inputs["Payload"] = new JObject { ["allOf"] = new JArray(Ref("Parent")) };
        return source;
    }
}
