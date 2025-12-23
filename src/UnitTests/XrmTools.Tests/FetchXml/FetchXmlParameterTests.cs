namespace XrmTools.Tests.FetchXml;

using System.Threading.Tasks;
using Xunit;
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
}
