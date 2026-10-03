namespace XrmTools.Tests.PluginTrace;

using System;
using XrmTools.Environments;
using XrmTools.PluginTrace;
using Xunit;

public class TraceLoggingPolicyTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(99, 0)]
    public void RestorationNeverEnablesAll(int previous, int expected) =>
        Assert.Equal((TraceLoggingMode)expected, TraceLoggingPolicy.RestoreMode((TraceLoggingMode)previous));

    [Fact]
    public void RestartPreservesOriginalExceptionsMode()
    {
        var environment = new DataverseEnvironment { ConnectionString = "Url=https://contoso.crm.dynamics.com" };
        var id = Guid.NewGuid();
        var expiry = DateTimeOffset.UtcNow.AddHours(1);
        var lease = new TraceLoggingLease(environment.Url + "/", id, TraceLoggingMode.Exceptions, expiry, expiry.AddDays(7), null);

        Assert.Equal(TraceLoggingMode.Exceptions, TraceLoggingPolicy.RestoreModeForStart(lease, environment, id, TraceLoggingMode.All));
        Assert.Equal(TraceLoggingMode.Off, TraceLoggingPolicy.RestoreModeForStart(lease, environment, Guid.NewGuid(), TraceLoggingMode.All));
        var other = environment with { ConnectionString = "Url=https://other.crm.dynamics.com" };
        Assert.Equal(TraceLoggingMode.Off, TraceLoggingPolicy.RestoreModeForStart(lease, other, id, TraceLoggingMode.All));
    }
}
