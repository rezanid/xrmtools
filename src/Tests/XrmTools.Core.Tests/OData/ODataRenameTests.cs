namespace XrmTools.Core.Tests.OData;

using System.Linq;
using XrmTools.OData;
using Xunit;

public class ODataRenameTests
{
    [Fact]
    public void LinksOnlyExecutableOccurrencesOfTheSelectedSymbol()
    {
        const string text = "@item = value\r\n# @name item\r\nGET /contacts\r\n###\r\n# {{item}} {{item.response.body.$}}\r\nPOST /{{ item }}/{{item.response.body.$.id}}\r\nX-Example: {{item}}\r\n\r\n{\"id\":\"{{item.response.body.$.id}}\"}";

        var variable = Assert.IsType<ODataRename>(ODataRename.Find(text, 5, 10));
        Assert.Equal(new[] { 0, 5, 6 }, variable.Occurrences.Select(item => item.Line));
        Assert.Equal(9, variable.Active.Column);
        Assert.False(variable.Active.IsRequest);

        var request = Assert.IsType<ODataRename>(ODataRename.Find(text, 1, 9));
        Assert.Equal(new[] { 1, 5, 8 }, request.Occurrences.Select(item => item.Line));
        Assert.True(request.Active.IsRequest);
    }

    [Fact]
    public void RejectsAmbiguityInvalidNamesAndSameKindCollisions()
    {
        const string text = "@first = 1\n@taken = 2\n# @name available\nGET /{{first}}";
        var rename = Assert.IsType<ODataRename>(ODataRename.Find(text, 0, 2));
        Assert.NotNull(rename.Validate("taken"));
        Assert.NotNull(rename.Validate("1invalid"));
        Assert.Null(rename.Validate("available"));
        Assert.Null(ODataRename.Find("@first = 1\n" + text, 1, 2));
    }
}
