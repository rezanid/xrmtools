namespace XrmTools.Tests.FetchXml;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Language.Xml;
using Xunit;
using XrmTools.FetchXml;
using XrmTools.FetchXml.CodeGen;

public class FetchXmlParameterTests
{
    [Fact]
    public async Task ParseAsync_DetectsElementParameter_WithoutDefault()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch distinct='false' aggregate='true'>
  <entity name=""account"">
    <attribute name='accountid' aggregate='count' alias='account_count' />
    <param name='filterXml' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("filterXml", result.Parameters[0].Name);
        Assert.True(result.Parameters[0].IsElementParameter);
        Assert.True(string.IsNullOrEmpty(result.Parameters[0].DefaultValue));
    }

    [Fact]
    public async Task ParseAsync_DetectsElementParameter_WithDefault()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch distinct='false' aggregate='true'>
  <entity name=""account"">
    <attribute name='accountid' aggregate='count' alias='account_count' />
    <param name='filterXml'>
      <filter type='and'>
        <condition attribute='address1_city' operator='eq' value='Redmond' />
      </filter>
    </param>
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("filterXml", result.Parameters[0].Name);
        Assert.True(result.Parameters[0].IsElementParameter);
        Assert.False(string.IsNullOrEmpty(result.Parameters[0].DefaultValue));
        Assert.Contains("filter", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_DetectsValueParameter_WithoutDefault()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch distinct='false' aggregate='true'>
  <entity name=""{{entityName}}"">
    <attribute name='accountid' aggregate='count' alias='account_count' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("entityName", result.Parameters[0].Name);
        Assert.False(result.Parameters[0].IsElementParameter);
        Assert.Null(result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_DetectsValueParameter_WithDefault()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch distinct='false' aggregate='true'>
  <entity name=""{{entityName:account}}"">
    <attribute name='accountid' aggregate='count' alias='account_count' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("entityName", result.Parameters[0].Name);
        Assert.False(result.Parameters[0].IsElementParameter);
        Assert.Equal("account", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_DetectsMultipleParameters()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch distinct='false' aggregate='true'>
  <entity name=""{{entityName:account}}"">
    <attribute name='accountid' aggregate='count' alias='account_count' />
    <param name='filterXml'>
      <filter type='and'>
        <condition attribute='address1_city' operator='eq' value='{{cityName:Redmond}}' />
      </filter>
    </param>
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Parameters.Count); // entityName, filterXml, cityName
        Assert.Contains(result.Parameters, p => p.Name == "entityName" && !p.IsElementParameter);
        Assert.Contains(result.Parameters, p => p.Name == "filterXml" && p.IsElementParameter);
        Assert.Contains(result.Parameters, p => p.Name == "cityName" && !p.IsElementParameter);
    }

    [Fact]
    public void ReplaceParameters_ReplacesElementParameter()
    {
        // Arrange
        var fetchXml = @"<fetch>
  <entity name=""account"">
    <param name='filterXml' />
  </entity>
</fetch>";
        var document = Parser.ParseText(fetchXml);
        var parameters = new Dictionary<string, string>
        {
            ["filterXml"] = "<filter><condition attribute='name' operator='eq' value='Test' /></filter>"
        };

        // Act
        var result = FetchXmlParameterReplacer.ReplaceParameters(document, parameters, updateDefaults: false);

        // Assert
        var resultString = result.ToFullString();
        Assert.Contains("<filter><condition attribute='name' operator='eq' value='Test' /></filter>", resultString);
        Assert.DoesNotContain("<param", resultString);
    }

    [Fact]
    public void ReplaceParameters_ReplacesValueParameter()
    {
        // Arrange
        var fetchXml = @"<fetch>
  <entity name=""{{entityName}}"">
    <attribute name='accountid' />
  </entity>
</fetch>";
        var document = Parser.ParseText(fetchXml);
        var parameters = new Dictionary<string, string>
        {
            ["entityName"] = "contact"
        };

        // Act
        var result = FetchXmlParameterReplacer.ReplaceParameters(document, parameters, updateDefaults: false);

        // Assert
        var resultString = result.ToFullString();
        Assert.Contains("name=\"contact\"", resultString);
        Assert.DoesNotContain("{{entityName}}", resultString);
    }

    [Fact]
    public void ReplaceParameters_ReplacesValueParameterWithDefault()
    {
        // Arrange
        var fetchXml = @"<fetch>
  <entity name=""{{entityName:account}}"">
    <attribute name='accountid' />
  </entity>
</fetch>";
        var document = Parser.ParseText(fetchXml);
        var parameters = new Dictionary<string, string>
        {
            ["entityName"] = "contact"
        };

        // Act
        var result = FetchXmlParameterReplacer.ReplaceParameters(document, parameters, updateDefaults: false);

        // Assert
        var resultString = result.ToFullString();
        Assert.Contains("name=\"contact\"", resultString);
        Assert.DoesNotContain("{{entityName", resultString);
    }

    [Fact]
    public void ReplaceParameters_UpdatesDefaultValueForElementParameter()
    {
        // Arrange
        var fetchXml = @"<fetch>
  <entity name=""account"">
    <param name='filterXml'>
      <filter><condition attribute='oldfield' operator='eq' value='old' /></filter>
    </param>
  </entity>
</fetch>";
        var document = Parser.ParseText(fetchXml);
        var parameters = new Dictionary<string, string>
        {
            ["filterXml"] = "<filter><condition attribute='newfield' operator='eq' value='new' /></filter>"
        };

        // Act
        var result = FetchXmlParameterReplacer.ReplaceParameters(document, parameters, updateDefaults: true);

        // Assert
        var resultString = result.ToFullString();
        Assert.Contains("<param name='filterXml'><filter><condition attribute='newfield' operator='eq' value='new' /></filter></param>", resultString);
    }

    [Fact]
    public void ReplaceParameters_UpdatesDefaultValueForValueParameter()
    {
        // Arrange
        var fetchXml = @"<fetch>
  <entity name=""{{entityName:account}}"">
    <attribute name='accountid' />
  </entity>
</fetch>";
        var document = Parser.ParseText(fetchXml);
        var parameters = new Dictionary<string, string>
        {
            ["entityName"] = "contact"
        };

        // Act
        var result = FetchXmlParameterReplacer.ReplaceParameters(document, parameters, updateDefaults: true);

        // Assert
        var resultString = result.ToFullString();
        Assert.Contains("{{entityName:contact}}", resultString);
    }

    [Fact]
    public void ReplaceParameters_HandlesMultipleParameters()
    {
        // Arrange
        var fetchXml = @"<fetch>
  <entity name=""{{entityName:account}}"">
    <attribute name='accountid' />
    <param name='filterXml' />
    <order attribute='{{orderBy:name}}' />
  </entity>
</fetch>";
        var document = Parser.ParseText(fetchXml);
        var parameters = new Dictionary<string, string>
        {
            ["entityName"] = "contact",
            ["filterXml"] = "<filter><condition attribute='statecode' operator='eq' value='0' /></filter>",
            ["orderBy"] = "createdon"
        };

        // Act
        var result = FetchXmlParameterReplacer.ReplaceParameters(document, parameters, updateDefaults: false);

        // Assert
        var resultString = result.ToFullString();
        Assert.Contains("name=\"contact\"", resultString);
        Assert.Contains("<filter><condition attribute='statecode' operator='eq' value='0' /></filter>", resultString);
        Assert.Contains("attribute='createdon'", resultString);
        Assert.DoesNotContain("<param", resultString);
        Assert.DoesNotContain("{{", resultString);
    }
}
