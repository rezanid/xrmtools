namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Xunit;
using XrmTools.DataverseSolutions;

public class PacAuthBridgeTests
{
    [Fact]
    public void FindMatchingProfiles_NormalizesUrls_AndPrefersActiveProfile()
    {
        var profiles = new[]
        {
            new PacAuthProfile { Index = 2, IsActive = false, EnvironmentUrl = "https://org.crm4.dynamics.com/" },
            new PacAuthProfile { Index = 1, IsActive = true, EnvironmentUrl = "https://ORG.crm4.dynamics.com" }
        };

        var matches = PacAuthBridge.FindMatchingProfiles(profiles, "https://org.crm4.dynamics.com");

        matches.Should().HaveCount(2);
        matches[0].Index.Should().Be(1);
    }
}
