#nullable enable
namespace XrmTools.Tests.CodeGen.CustomApi;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;
using XrmTools.CodeGen.CustomApi;
using XrmTools.WebApi.Entities;
using XrmTools.WebApi.Types;
using Api = XrmTools.WebApi.Entities.CustomApi;

public sealed class ODataFunctionTests
{
    private static readonly Dictionary<string, EntityMetadata> Tables = new()
    {
        ["account"] = new EntityMetadata("account", "accounts") { PrimaryIdAttribute = "accountid" },
    };
    private static JObject Write(Api api, CustomApiSourceSchemas? source = null) => JObject.Parse(CustomApiOpenApiWriter.Write(api,
        new Uri("https://example.crm.dynamics.com/api/data/v9.2/"), Tables, source));
    private static Api Function() => new() { UniqueName = "new_Search", IsFunction = true,
        ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.String }] };

    [Theory]
    [InlineData(Api.BindingTypes.Global, "/new_Search(Name=@Name)")]
    [InlineData(Api.BindingTypes.Entity, "/accounts({recordId})/Microsoft.Dynamics.CRM.new_Search(Name=@Name)")]
    [InlineData(Api.BindingTypes.EntityCollection, "/accounts/Microsoft.Dynamics.CRM.new_Search(Name=@Name)")]
    public void FunctionsUseAliasesGetAndExistingBindings(Api.BindingTypes binding, string path)
    {
        var api = Function(); api.BindingType = binding; api.BoundEntityLogicalName = "account";
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Name", Type = CustomApiFieldType.String });
        if (binding == Api.BindingTypes.Entity) api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Target", Type = CustomApiFieldType.EntityReference });
        var spec = Write(api);
        var operation = spec["paths"]![path]!["get"]!;
        Assert.Null(spec["paths"]![path]!["post"]);
        Assert.Null(operation["requestBody"]);
        Assert.NotNull(operation["responses"]!["200"]);
        Assert.Null(operation["responses"]!["204"]);
        var alias = operation["parameters"]!.OfType<JObject>().Single(p => (string?)p["in"] == "query");
        Assert.Equal("@Name", (string?)alias["name"]);
        Assert.True((bool)alias["required"]!);
        Assert.False((bool)alias["allowReserved"]!);
        var literal = (string)alias["example"]!;
        Assert.Equal("'O''Brien & café/+%'", literal);
        var request = (string)operation["x-codeSamples"]![0]!["source"]!;
        Assert.Contains("@Name=" + Uri.EscapeDataString(literal), request);
        Assert.Equal(literal, Uri.UnescapeDataString(request.Split('\n')[0].Split(new[] { "@Name=" }, StringSplitOptions.None)[1]));
    }

    [Fact]
    public void OptionalAssignmentsKeepSignatureAndDistinguishNullFromEmptyString()
    {
        var api = Function();
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Name", Type = CustomApiFieldType.String, IsOptional = true });
        var operation = Write(api)["paths"]!["/new_Search(Name=@Name)"]!["get"]!;
        var parameter = operation["parameters"]![0]!;
        Assert.False((bool)parameter["required"]!);
        Assert.Contains("supplies null", (string)parameter["description"]!);
        var pattern = (string)parameter["schema"]!["pattern"]!;
        Assert.Matches(pattern, "null");
        Assert.Matches(pattern, "''");
        Assert.Matches(pattern, "'null'");
        Assert.DoesNotMatch(pattern, "'O'Brien'");
        Assert.Equal("null", ODataFunctionParameters.Serialize(CustomApiFieldType.String, JValue.CreateNull()));
        Assert.Equal("''", ODataFunctionParameters.Serialize(CustomApiFieldType.String, new JValue("")));
    }

    [Fact]
    public void ArraysUseJsonAndNumbersAreCultureIndependent()
    {
        var literal = ODataFunctionParameters.Serialize(CustomApiFieldType.StringArray, new JArray("O'Brien", "a&b/+%", "café"));
        Assert.Equal(new[] { "O'Brien", "a&b/+%", "café" }, JArray.Parse(literal).ToObject<string[]>()!);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal("1.25", ODataFunctionParameters.Serialize(CustomApiFieldType.Decimal, new JValue(1.25m)));
            Assert.Equal("true", ODataFunctionParameters.Serialize(CustomApiFieldType.Boolean, new JValue(true)));
            Assert.Equal("INF", ODataFunctionParameters.Serialize(CustomApiFieldType.Float, new JValue(double.PositiveInfinity)));
            Assert.Equal("-INF", ODataFunctionParameters.Serialize(CustomApiFieldType.Float, new JValue(double.NegativeInfinity)));
            Assert.Equal("NaN", ODataFunctionParameters.Serialize(CustomApiFieldType.Float, new JValue(double.NaN)));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void ParameterlessFunctionsAndOpenOutputEntitiesAreSupported()
    {
        var api = Function();
        api.ResponseProperties = [new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.Entity }];
        Assert.NotNull(Write(api)["paths"]!["/new_Search()"]!["get"]!["responses"]!["200"]);
        api.ResponseProperties.Clear();
        Assert.Throws<InvalidOperationException>(() => Write(api));
        api.ResponseProperties.Add(new CustomApiResponseProperty { UniqueName = "Result", Type = CustomApiFieldType.String });
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Open", Type = CustomApiFieldType.Entity });
        Assert.Contains("open entity", Assert.Throws<InvalidOperationException>(() => Write(api)).Message);
    }

    [Fact]
    public void EnumAliasesRetainNamesAndLogicalSchemaAndPermitOptionalNull()
    {
        var api = Function();
        api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "Choice", Type = CustomApiFieldType.Picklist, IsOptional = true });
        var source = new CustomApiSourceSchemas();
        source.Inputs["Choice"] = new JObject { ["type"] = "integer", ["enum"] = new JArray(1, 2), ["x-enumNames"] = new JArray("First", "Second"),
            ["x-enumDescriptions"] = new JObject { ["1"] = "First", ["2"] = "Second" } };
        var parameter = Write(api, source)["paths"]!["/new_Search(Choice=@Choice)"]!["get"]!["parameters"]![0]!;
        Assert.Equal(new[] { "1", "2", "null" }, parameter["schema"]!["enum"]!.ToObject<string[]>()!);
        Assert.Equal("First", (string?)parameter["schema"]!["x-enumDescriptions"]!["1"]);
        Assert.Equal("integer", (string?)parameter["x-odata-value-schema"]!["type"]);
    }

    [Fact]
    public void AllClosedInputTypesHaveExamplesThatMatchTheirWireSchema()
    {
        var api = Function();
        foreach (CustomApiFieldType type in Enum.GetValues(typeof(CustomApiFieldType)))
            api.RequestParameters.Add(new CustomApiRequestParameter { UniqueName = "P_" + type, Type = type, LogicalEntityName = "account" });
        var operation = ((JObject)Write(api)["paths"]!).Properties().Single().Value["get"]!;
        foreach (var parameter in operation["parameters"]!.OfType<JObject>())
        {
            var example = (string)parameter["example"]!;
            if (parameter["schema"]!["pattern"] is JValue pattern) Assert.Matches((string)pattern!, example);
            if (((string)parameter["name"]!).Contains("Entity")) Assert.Contains("accounts(", example);
        }
    }
}
