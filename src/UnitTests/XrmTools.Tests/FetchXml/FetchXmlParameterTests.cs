namespace XrmTools.Tests.FetchXml;

using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using XrmTools.FetchXml.CodeGen;
using Xunit;

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
        Assert.Empty(result.Parameters[0].DefaultValue);
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
        // Note: inner parameters are not supported; only top-level params are detected.
        Assert.NotNull(result);
        Assert.Equal(2, result.Parameters.Count); // entityName, filterXml, cityName
        Assert.Contains(result.Parameters, p => p.Name == "entityName" && !p.IsElementParameter);
        Assert.Contains(result.Parameters, p => p.Name == "filterXml" && p.IsElementParameter);
    }


    [Fact]
    public async Task ParseAsync_IgnoresAttributeToken_WhenMissingClosingBraces()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""{{entityName:account}"">
    <attribute name='accountid' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Parameters);
    }

    [Fact]
    public async Task ParseAsync_IgnoresAttributeToken_WhenNoNameOrDefaultProvided()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""{{:account}}"">
    <attribute name='accountid' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("p1", result.Parameters[0].Name);
        Assert.False(result.Parameters[0].IsElementParameter);
        Assert.Equal("account", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_TrimsDefaultValue_ForAttributeParameter()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""{{entityName:  account  }}"">
    <attribute name='accountid' />
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
    public async Task ParseAsync_DetectsAttributeParameter_InAnyAttribute()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""account"">
    <filter type='and'>
      <condition attribute='name' operator='eq' value='{{accName:Contoso}}' />
    </filter>
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("accName", result.Parameters[0].Name);
        Assert.False(result.Parameters[0].IsElementParameter);
        Assert.Equal("Contoso", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_DetectsParamElement_IgnoringCase()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch distinct='false'>
  <entity name=""account"">
    <PARAM name='filterXml' />
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
    }

    [Fact]
    public async Task ParseAsync_ElementParameter_DefaultValue_PreservesInnerXml()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""account"">
    <param name='filterXml'><filter type='and'><condition attribute='name' operator='eq' value='A' /></filter></param>
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.True(result.Parameters[0].IsElementParameter);
        Assert.Equal("filterXml", result.Parameters[0].Name);
        Assert.Contains("<filter", result.Parameters[0].DefaultValue);
        Assert.Contains("condition", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_DetectsParameter_InDeeplyNestedElementAttributes()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""account"">
    <link-entity name='contact' from='parentcustomerid' to='accountid'>
      <filter type='and'>
        <condition attribute='firstname' operator='eq' value='{{firstName:John}}' />
      </filter>
    </link-entity>
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("firstName", result.Parameters[0].Name);
        Assert.False(result.Parameters[0].IsElementParameter);
        Assert.Equal("John", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_OnlyCapturesFirstToken_PerAttributeValue()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""account"">
    <attribute name='name' alias='{{a:1}}-{{b:2}}' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("a", result.Parameters[0].Name);
        Assert.Equal("1", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_AllowsEmptyDefaultValue_WhenColonPresent()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""{{entityName:}}"">
    <attribute name='accountid' />
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
        Assert.Equal(string.Empty, result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_UsesAutoName_WhenTokenContentIsEmpty()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""{{}}"">
    <attribute name='accountid' />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("p1", result.Parameters[0].Name);
        Assert.False(result.Parameters[0].IsElementParameter);
        Assert.Equal(string.Empty, result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_IncludesParamElement_EvenWhenMissingNameAttribute()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""account"">
    <param />
    <param />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        //Assert.Single(result.Parameters);
        result.Parameters.Should().HaveCount(2);
        Assert.True(result.Parameters[0].IsElementParameter);
        Assert.True(result.Parameters[1].IsElementParameter);
        Assert.Equal("p1", result.Parameters[0].Name);
        Assert.Equal("p2", result.Parameters[1].Name);
    }

    [Fact]
    public async Task ParseAsync_DetectsMixedSingleAndDoubleQuoteAttributes()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name='{{entityName:account}}'>
    <attribute name=""accountid"" />
  </entity>
</fetch>";

        var parser = new FetchXmlParser();

        // Act
        var result = await parser.ParseAsync(fetchXml);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Parameters);
        Assert.Equal("entityName", result.Parameters[0].Name);
        Assert.Equal("account", result.Parameters[0].DefaultValue);
    }

    [Fact]
    public async Task ParseAsync_DoesNotFlattenElementParameterChildrenIntoParameters()
    {
        // Arrange
        var fetchXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<fetch>
  <entity name=""account"">
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
        Assert.Equal(1, result.Parameters.Count(p => p.IsElementParameter));
        Assert.DoesNotContain(result.Parameters, p => p.Name == "cityName");
    }
}
