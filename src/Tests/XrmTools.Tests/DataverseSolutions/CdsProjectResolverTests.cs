namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Xunit;
using XrmTools.DataverseSolutions;

public class CdsProjectResolverTests
{
    [Fact]
    public void ParsePropertiesJson_ReadsMsBuildGetPropertyJson()
    {
        const string json = """
{
  "Properties": {
    "SolutionPackageMapFilePath": "solution-mapping-Debug.xml",
    "SolutionRootPath": "src",
    "SolutionPackageZipFilePath": "bin\\Debug\\Solution.zip"
  }
}
""";

        var properties = CdsProjectResolver.ParsePropertiesJson(json);

        properties["SolutionPackageMapFilePath"].Should().Be("solution-mapping-Debug.xml");
        properties["SolutionRootPath"].Should().Be("src");
        properties["SolutionPackageZipFilePath"].Should().Be("bin\\Debug\\Solution.zip");
    }

    [Fact]
    public void ResolvePath_CombinesRelativePathWithProjectDirectory()
    {
        var resolved = CdsProjectResolver.ResolvePath("C:\\repo\\Solution", "src\\Other\\Solution.xml");

        resolved.Should().Be("C:\\repo\\Solution\\src\\Other\\Solution.xml");
    }
}
