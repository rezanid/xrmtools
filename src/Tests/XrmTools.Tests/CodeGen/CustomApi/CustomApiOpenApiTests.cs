#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

public sealed class CustomApiOpenApiTests
{
    private static readonly Dictionary<string, EntityMetadata> Tables = new()
    {
        ["account"] = new EntityMetadata("account", "accounts") { PrimaryIdAttribute = "accountid" },
    };
    private static JObject Write(Api api) => JObject.Parse(CustomApiOpenApiWriter.Write(api,
        new Uri("https://example.crm.dynamics.com/api/data/v9.2/"), Tables));

    [Theory]
    [InlineData(Api.BindingTypes.Global, "/new_Test")]
    [InlineData(Api.BindingTypes.Entity, "/accounts({recordId})/Microsoft.Dynamics.CRM.new_Test")]
    [InlineData(Api.BindingTypes.EntityCollection, "/accounts/Microsoft.Dynamics.CRM.new_Test")]
    public void ActionBindingAndTargetUseActualHttpContract(Api.BindingTypes binding, string path)
    {
        var api = new Api { UniqueName = "new_Test", BindingType = binding, BoundEntityLogicalName = "account" };
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Target", Type = CustomApiFieldType.EntityReference, LogicalEntityName = "account" });
        var doc = Write(api);
        var operation = doc["paths"]![path]!["post"]!;
        Assert.Equal("3.0.4", (string?)doc["openapi"]);
        Assert.NotNull(operation["responses"]!["204"]);
        if (binding == Api.BindingTypes.Entity)
        {
            Assert.Null(operation["requestBody"]);
            Assert.Equal("recordId", (string?)operation["parameters"]![0]!["name"]);
            Assert.True((bool)operation["parameters"]![0]!["required"]!);
        }
        else Assert.NotNull(operation["requestBody"]!["content"]!["application/json"]!["schema"]!["properties"]!["Target"]);
    }

    [Fact]
    public void AllTypesPreserveDescriptionsAndOptionalityWithoutInventingNullability()
    {
        var api = new Api { UniqueName = "new_Test", Description = "Operation docs" };
        foreach (CustomApiFieldType type in Enum.GetValues(typeof(CustomApiFieldType)))
            api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "P_" + type, Type = type,
                Description = "Field docs", IsOptional = type == CustomApiFieldType.String });
        var operation = Write(api)["paths"]!["/new_Test"]!["post"]!;
        var schema = operation["requestBody"]!["content"]!["application/json"]!["schema"]!;
        Assert.Equal("Operation docs", (string?)operation["description"]);
        Assert.DoesNotContain("P_String", schema["required"]!.ToObject<string[]>()!);
        Assert.Contains("P_Integer", schema["required"]!.ToObject<string[]>()!);
        Assert.Equal("Field docs", (string?)schema["properties"]!["P_String"]!["description"]);
        Assert.Null(schema["properties"]!["P_String"]!["nullable"]);
        Assert.Equal("array", (string?)schema["properties"]!["P_EntityCollection"]!["type"]);
        Assert.Equal("uuid", (string?)schema["properties"]!["P_Guid"]!["format"]);
    }

    [Theory]
    [InlineData(CustomApiFieldType.Entity)]
    [InlineData(CustomApiFieldType.EntityCollection)]
    [InlineData(CustomApiFieldType.String)]
    public void SingleOutputHasDataverseResponseShape(CustomApiFieldType type)
    {
        var api = new Api { UniqueName = "new_Test" };
        api.ResponseProperties.Add(new CustomApiResponseProperty { UniqueName = "Result", Type = type, LogicalEntityName = "account" });
        var schema = Write(api)["paths"]!["/new_Test"]!["post"]!["responses"]!["200"]!["content"]!["application/json"]!["schema"]!;
        Assert.NotNull(schema["properties"]![type == CustomApiFieldType.Entity ? "accountid" : type == CustomApiFieldType.EntityCollection ? "value" : "Result"]);
        if (type != CustomApiFieldType.String) Assert.Null(schema["properties"]!["Result"]);
    }

    [Fact]
    public void ErrorResponsesShareAnExtensibleDataverseEnvelope()
    {
        foreach (var hasOutput in new[] { false, true })
        {
            var api = new Api { UniqueName = "new_Test" };
            if (hasOutput) api.ResponseProperties.Add(new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.String });
            var doc = Write(api);
            var responses = doc["paths"]!["/new_Test"]!["post"]!["responses"]!;
            Assert.NotNull(responses[hasOutput ? "200" : "204"]);
            foreach (var status in new[] { "4XX", "5XX" })
                Assert.Equal("#/components/schemas/DataverseError", (string?)responses[status]!["content"]!["application/json"]!["schema"]!["$ref"]);
            var schema = doc["components"]!["schemas"]!["DataverseError"]!;
            Assert.Equal(new[] { "error" }, schema["required"]!.ToObject<string[]>()!);
            var details = schema["properties"]!["error"]!;
            Assert.Equal(new[] { "code", "message" }, details["required"]!.ToObject<string[]>()!);
            Assert.True((bool)details["additionalProperties"]!);
            Assert.True((bool)schema["additionalProperties"]!);
            Assert.Equal("string", (string?)details["properties"]!["code"]!["type"]);
            Assert.Equal("string", (string?)details["properties"]!["message"]!["type"]);
            Assert.Null(details["properties"]!["code"]!["minLength"]);
            Assert.Null(doc["info"]!["license"]);
        }
    }

    [Fact]
    public void RejectsFunctionsDuplicateNamesAndUnknownTypes()
    {
        Assert.Throws<InvalidOperationException>(() => Write(new Api { UniqueName = "new_Test", IsFunction = true }));
        var api = new Api { UniqueName = "new_Test" };
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Value", Type = (CustomApiFieldType)999 });
        Assert.Throws<InvalidOperationException>(() => Write(api));
        api.RequestParameters.Clear();
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Value", Type = CustomApiFieldType.String });
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Value", Type = CustomApiFieldType.String });
        Assert.Throws<InvalidOperationException>(() => Write(api));
    }
}
