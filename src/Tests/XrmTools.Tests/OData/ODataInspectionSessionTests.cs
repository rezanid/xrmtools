namespace XrmTools.Tests.OData;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Moq;
using XrmTools;
using XrmTools.OData;
using Xunit;

public class ODataInspectionSessionTests
{
    private const string Source = "# @name r\nGET /WhoAmI";
    private static DataverseEnvironment Environment(string name = "Development")
        => new() { Name = name, ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };

    [Fact]
    public async Task BufferHoverUsesOnlyCurrentCaptureAndExplainsRerunInvalidation()
    {
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.Setup(s => s.GetText()).Returns(Source);
        var buffer = new Mock<ITextBuffer>();
        buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        using var session = ODataBufferSession.Attach(buffer.Object);
        await session.SetEnvironmentAsync(Environment());
        var document = session.CreateDocument(Source);
        var run = session.Captures.Begin(document, document.Requests[0], Environment());
        session.Captures.Complete(run, new ODataCapturedResponse { Body = "{\"id\":7}" }, CancellationToken.None);
        var inspected = await session.InspectAsync(Source, "r.response.body.$.id", CancellationToken.None);
        Assert.Equal("7", inspected.Preview);
        Assert.True(inspected.IsCurrent());
        session.Captures.Begin(document, document.Requests[0], Environment());
        Assert.False(inspected.IsCurrent());
        var missing = await session.InspectAsync(Source, "r.response.body.$.id", CancellationToken.None);
        Assert.Contains("No valid captured response", missing.Preview);
        Assert.Contains("completed 2xx", missing.Preview);
        Assert.Null(await session.InspectAsync(Source + "changed", "r.response.body.$.id", CancellationToken.None));
    }

    [Theory]
    [InlineData("environment")]
    [InlineData("rerun")]
    [InlineData("edit")]
    [InlineData("close")]
    [InlineData("epoch")]
    public async Task PendingCaptureInspectionIsRejectedAfterInvalidation(string change)
    {
        var captures = new ODataCaptureSession();
        var document = ODataDocument.Parse(Source);
        var run = captures.Begin(document, document.Requests[0], Environment());
        var capture = new ODataCapturedResponse { Body = "{\"id\":7}" };
        captures.Complete(run, capture, CancellationToken.None);
        long epoch = captures.Epoch;
        var inspected = new Dictionary<string, ODataCapturedResponse>();
        using var entered = new ManualResetEventSlim();
        using var proceed = new ManualResetEventSlim();
        var pending = ODataInspector.InspectAsync(document, "r.response.body.$.id", "Development", name =>
        {
            var current = captures.Get(name);
            inspected[name] = current;
            entered.Set();
            Assert.True(proceed.Wait(5000), "The test must release the pending inspection.");
            return current;
        }, _ => { }, CancellationToken.None);
        try
        {
            Assert.True(entered.Wait(5000), "Inspection must start before invalidating its context.");
            switch (change)
            {
                case "environment":
                    captures.SetEnvironment(Environment("Test"));
                    captures.SetEnvironment(Environment());
                    break;
                case "rerun": captures.Begin(document, document.Requests[0], Environment()); break;
                case "edit": captures.Update(ODataDocument.Parse("# @name r\nGET /accounts")); break;
                case "close": captures.Close(); break;
                default: captures.AdvanceEpoch(); break;
            }
        }
        finally { proceed.Set(); }
        await pending;
        Assert.False(captures.AreInspectionsCurrent(inspected, epoch));
        captures.Close();
    }

    [Fact]
    public void MissingCaptureInspectionBecomesStaleWhenAResponseIsPublished()
    {
        var captures = new ODataCaptureSession();
        var document = ODataDocument.Parse(Source);
        var run = captures.Begin(document, document.Requests[0], Environment());
        long epoch = captures.Epoch;
        var inspected = new Dictionary<string, ODataCapturedResponse> { ["r"] = captures.Get("r") };
        Assert.True(captures.AreInspectionsCurrent(inspected, epoch));
        var response = new ODataCapturedResponse { Body = "{}" };
        captures.Complete(run, response, CancellationToken.None);
        Assert.False(captures.AreInspectionsCurrent(inspected, epoch));
        inspected["r"] = response;
        Assert.True(captures.AreInspectionsCurrent(inspected, epoch));
        captures.Close();
    }

    [Fact]
    public async Task CompletedBufferInspectionRejectsClosedSessions()
    {
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.Setup(s => s.GetText()).Returns(Source);
        var buffer = new Mock<ITextBuffer>();
        buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        var session = ODataBufferSession.Attach(buffer.Object);
        session.Dispose();
        Assert.Null(await session.InspectAsync(Source, "$guid", CancellationToken.None));
    }
}
