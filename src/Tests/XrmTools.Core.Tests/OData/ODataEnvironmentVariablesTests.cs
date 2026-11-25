namespace XrmTools.Core.Tests.OData;

using System;
using System.Collections.Generic;
using XrmTools.OData;
using Xunit;

public class ODataEnvironmentVariablesTests
{
    [Fact]
    public void SelectsTheNamedProfileAndKeepsVariableNamesCaseSensitive()
    {
        const string json = "{\"Development\":{\"account1\":\"11111111-1111-1111-1111-111111111111\"},\"Test\":{\"account1\":\"22222222-2222-2222-2222-222222222222\"}}";
        var development = ODataEnvironmentVariables.Parse(json, "Development");
        var test = ODataEnvironmentVariables.Parse(json, "Test");
        var missing = ODataEnvironmentVariables.Parse(json, "Staging");

        Assert.Null(development.Error);
        Assert.Equal("11111111-1111-1111-1111-111111111111", development.Variables["account1"]);
        Assert.Equal("22222222-2222-2222-2222-222222222222", test.Variables["account1"]);
        Assert.Empty(missing.Variables);
        Assert.True(missing.ProfileMissing);
        Assert.False(development.ProfileMissing);
        Assert.False(ODataEnvironmentVariables.Missing().ProfileMissing);
        Assert.False(development.Variables.ContainsKey("Account1"));
    }

    [Fact]
    public void RejectsMalformedOrAmbiguousConfigurationWithoutReturningValues()
    {
        var malformed = ODataEnvironmentVariables.Parse("{", "Development");
        var ambiguous = ODataEnvironmentVariables.Parse(
            "{\"Development\":{\"privateName\":\"one\"},\"development\":{\"privateName\":\"two\"}}", "Development");
        var invalidValue = ODataEnvironmentVariables.Parse(
            "{\"Development\":{\"account1\":42}}", "Development");

        Assert.Empty(malformed.Variables);
        Assert.Contains("malformed JSON", malformed.Error);
        Assert.Empty(ambiguous.Variables);
        Assert.Contains("ambiguous", ambiguous.Error);
        Assert.DoesNotContain("one", ambiguous.Error);
        Assert.Empty(invalidValue.Variables);
        Assert.Contains("JSON strings", invalidValue.Error);
    }

    [Fact]
    public void DocumentDeclarationsOverrideEnvironmentValuesAndKeepLiteralExpansion()
    {
        var document = ODataDocument.Parse("@account1 = local-value\nGET /accounts({{account1}})/{{Account1}}");
        document.SetEnvironmentVariables(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["account1"] = "environment-value",
            ["Account1"] = "case-sensitive-value"
        });

        Assert.Equal("/accounts(local-value)/case-sensitive-value", document.Resolve(document.Requests[0]).Target);
        Assert.Equal("local-value", document.EffectiveVariables["account1"]);
    }

    [Fact]
    public void MissingProfileVariablesFailResolutionBeforeAnyCallerCanAuthenticate()
    {
        var document = ODataDocument.Parse("GET /accounts({{account1}})");
        document.SetEnvironmentVariables(ODataEnvironmentVariables.Parse("{}", "Development").Variables);

        var error = Assert.Throws<FormatException>(() => document.Resolve(document.Requests[0]));
        Assert.Contains("Undefined variable 'account1'", error.Message);
    }
}
