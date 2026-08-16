namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Xunit;
using XrmTools.DataverseSolutions;

public class DataverseEnvironmentUrlTests
{
    [Fact]
    public void Normalize_RemovesTrailingSlash_AndNormalizesHostCase()
    {
        var normalized = DataverseEnvironmentUrl.Normalize("https://ORG.crm4.dynamics.com/");

        normalized.Should().Be("https://org.crm4.dynamics.com");
    }
}
