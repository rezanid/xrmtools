namespace XrmTools.Core.Tests.OData;

using XrmTools.OData;
using Xunit;

public class ODataNavigationTests
{
    [Fact]
    public void ResolvesVariablesAndNamedRequestsInSeparateNamespaces()
    {
        const string text = "@item = old\r\n  @item = current\r\n  // @name item\r\nGET /contacts\r\n###\r\nGET /{{item}}/{{item.response.body.$.contactid}}";
        Assert.Equal((1, 3), ODataNavigation.FindDefinition(text, 5, 9));
        Assert.Equal((2, 11), ODataNavigation.FindDefinition(text, 5, 31));
    }

    [Fact]
    public void IgnoresCommentsAndAmbiguousRequestNames()
    {
        const string text = "@id = 1\n# {{id}}\n# @name item\nGET /contacts\n###\n# @name item\nGET /{{item.response.body.$}}";
        Assert.Null(ODataNavigation.FindDefinition(text, 1, 5));
        Assert.Null(ODataNavigation.FindDefinition(text, 6, 9));
    }
}
