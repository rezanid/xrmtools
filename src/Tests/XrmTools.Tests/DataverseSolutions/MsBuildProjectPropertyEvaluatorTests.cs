namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using XrmTools.DataverseSolutions;
using Xunit;

public class MsBuildProjectPropertyEvaluatorTests
{
    [Fact]
    public void ParseItemsJson_ReadsIdentityAndEvaluatedMetadata()
    {
        const string json = """
{
  "Items": {
    "WebResource": [
      {
        "Identity": "C:\\repo\\dist\\app.js",
        "FullPath": "C:\\repo\\dist\\app.js",
        "Name": "contoso_/scripts/app.js",
        "DisplayName": "Application"
      }
    ]
  }
}
""";

        var items = MsBuildProjectPropertyEvaluator.ParseItemsJson(json, "WebResource");

        items.Should().ContainSingle();
        items[0].Identity.Should().Be("C:\\repo\\dist\\app.js");
        items[0].Metadata["Name"].Should().Be("contoso_/scripts/app.js");
        items[0].Metadata["DisplayName"].Should().Be("Application");
    }
}
