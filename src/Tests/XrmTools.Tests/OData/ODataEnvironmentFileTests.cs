namespace XrmTools.Tests.OData;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Moq;
using XrmTools;
using XrmTools.OData;
using Xunit;

public class ODataEnvironmentFileTests
{
    [Fact]
    public async Task LoadsAndRefreshesTheSiblingProfileWithoutReadingFromDocumentEdits()
    {
        string directory = Path.Combine(Path.GetTempPath(), "XrmTools-OData-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string documentPath = Path.Combine(directory, "requests.odata");
        string sidecarPath = Path.Combine(directory, "odata-client.env.json");
        File.WriteAllText(sidecarPath, "{\"Development\":{\"account1\":\"first\"}}");
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.Setup(s => s.GetText()).Returns("GET /accounts({{account1}})");
        var buffer = new Mock<ITextBuffer>();
        buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        var session = ODataBufferSession.Attach(buffer.Object, documentPath);

        try
        {
            var environment = new DataverseEnvironment { Name = "Development", ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };
            await session.SetEnvironmentAsync(environment);
            Assert.True(session.TryCreateDocument(snapshot.Object.GetText(), "Development", out var initial, out _));
            Assert.Equal("first", initial.EffectiveVariables["account1"]);

            File.WriteAllText(sidecarPath, "{\"Development\":{\"account1\":\"second\"}}");
            await WaitUntilAsync(() => session.CreateDocument(snapshot.Object.GetText()).EffectiveVariables["account1"] == "second");
            Assert.True(session.TryCreateDocument(snapshot.Object.GetText(), "Development", out var updated, out _));
            Assert.Equal("second", updated.EffectiveVariables["account1"]);

            File.Delete(sidecarPath);
            await WaitUntilAsync(() => !session.CreateDocument(snapshot.Object.GetText()).EffectiveVariables.ContainsKey("account1"));
            Assert.True(session.TryCreateDocument(snapshot.Object.GetText(), "Development", out var deleted, out _));
            Assert.False(deleted.EffectiveVariables.ContainsKey("account1"));
        }
        finally
        {
            session.Dispose();
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task MismatchedProfileWarnsAndMatchingProfileLoadsAfterEdit()
    {
        string directory = Path.Combine(Path.GetTempPath(), "XrmTools-OData-Profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string documentPath = Path.Combine(directory, "requests.odata");
        string sidecarPath = Path.Combine(directory, "odata-client.env.json");
        File.WriteAllText(sidecarPath, "{\"DEV\":{\"account1_id\":\"one\",\"account2_id\":\"two\"}}");
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.Setup(s => s.GetText()).Returns("GET /accounts({{account1_id}})");
        var buffer = new Mock<ITextBuffer>();
        buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        var session = ODataBufferSession.Attach(buffer.Object, documentPath);
        try
        {
            await session.SetEnvironmentAsync(new DataverseEnvironment { Name = "DEV-FSI", ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" });
            Assert.Contains("DEV-FSI", session.ProfileWarning);
            Assert.Null(session.ConfigurationError);
            Assert.True(session.TryCreateDocument(snapshot.Object.GetText(), "DEV-FSI", out var before, out _));
            Assert.Empty(before.EnvironmentVariables);
            File.WriteAllText(sidecarPath, "{\"DEV-FSI\":{\"account1_id\":\"one\",\"account2_id\":\"two\"}}");
            await WaitUntilAsync(() => session.ProfileWarning == null && session.CreateDocument(snapshot.Object.GetText()).EnvironmentVariables.ContainsKey("account1_id"));
            Assert.True(session.TryCreateDocument(snapshot.Object.GetText(), "DEV-FSI", out var after, out _));
            Assert.Equal("/accounts(one)", after.Resolve(after.Requests[0]).Target);
            Assert.Equal("two", after.EnvironmentVariables["account2_id"]);
        }
        finally
        {
            session.Dispose();
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task RefreshesWhenANearerFileAppearsAndFallsBackToItsParentWhenDeleted()
    {
        string root = Path.Combine(Path.GetTempPath(), "XrmTools-OData-Ancestor-" + Guid.NewGuid().ToString("N"));
        string child = Path.Combine(root, "child");
        Directory.CreateDirectory(child);
        string documentPath = Path.Combine(child, "requests.odata");
        string parentSidecar = Path.Combine(root, "odata-client.env.json");
        string childSidecar = Path.Combine(child, "odata-client.env.json");
        File.WriteAllText(parentSidecar, "{\"Development\":{\"account1\":\"parent\"}}");
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.Setup(s => s.GetText()).Returns("GET /accounts({{account1}})");
        var buffer = new Mock<ITextBuffer>();
        buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        var session = ODataBufferSession.Attach(buffer.Object, documentPath);

        try
        {
            await session.SetEnvironmentAsync(new DataverseEnvironment { Name = "Development", ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" });
            Assert.Equal("parent", session.CreateDocument(snapshot.Object.GetText()).EffectiveVariables["account1"]);

            File.WriteAllText(childSidecar, "{\"Development\":{\"account1\":\"child\"}}");
            await WaitUntilAsync(() => session.CreateDocument(snapshot.Object.GetText()).EffectiveVariables["account1"] == "child");
            File.Delete(childSidecar);
            await WaitUntilAsync(() => session.CreateDocument(snapshot.Object.GetText()).EffectiveVariables["account1"] == "parent");

            File.Delete(parentSidecar);
            await WaitUntilAsync(() => !session.CreateDocument(snapshot.Object.GetText()).EffectiveVariables.ContainsKey("account1"));
        }
        finally
        {
            session.Dispose();
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            if (condition()) return;
            await Task.Delay(25);
        }
        Assert.True(condition(), "The sidecar update was not observed.");
    }
}
