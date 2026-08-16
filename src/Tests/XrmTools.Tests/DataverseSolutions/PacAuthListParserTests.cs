namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Xunit;
using XrmTools.DataverseSolutions;

public class PacAuthListParserTests
{
    [Fact]
    public void Parse_ParsesTabularPacAuthListOutput()
    {
        const string text = """
Index Active Kind      Name User                               Cloud  Type Environment                    Environment Url
[1]   *      UNIVERSAL      r.niroomand@reply.com              Public User BE-FSI-Accelerator-Process-DEV https://be-fsi-accelerator-process-dev.crm4.dynamics.com/
[2]          UNIVERSAL      reza.niroomand@businesselements.eu Public User Reza Niroomand's Environment   https://orgd67e4e96.crm4.dynamics.com/
""";

        var profiles = PacAuthListParser.Parse(text);

        profiles.Should().HaveCount(2);
        profiles[0].Index.Should().Be(1);
        profiles[0].IsActive.Should().BeTrue();
        profiles[0].EnvironmentUrl.Should().Be("https://be-fsi-accelerator-process-dev.crm4.dynamics.com/");
        profiles[1].EnvironmentName.Should().Be("Reza Niroomand's Environment");
    }
}
