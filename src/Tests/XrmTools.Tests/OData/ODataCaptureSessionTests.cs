namespace XrmTools.Tests.OData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Moq;
using XrmTools;
using XrmTools.OData;
using Xunit;

public class ODataCaptureSessionTests
{
    private static readonly DataverseEnvironment Environment = new() { ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };
    private static ODataCapturedResponse Response(string body) => new() { Body = body, Headers = new Dictionary<string, string>(), Principal = "user" };

    [Fact]
    public void BufferSessionIsSharedUntilTheLastViewCloses()
    {
        const string text = "# @name a\nGET /WhoAmI";
        var snapshot = new Mock<ITextSnapshot>();
        snapshot.Setup(s => s.GetText()).Returns(text);
        var buffer = new Mock<ITextBuffer>();
        buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        var first = ODataBufferSession.Attach(buffer.Object);
        var second = ODataBufferSession.Attach(buffer.Object);
        Assert.Same(first, second);
        var document = ODataDocument.Parse(text);
        var run = first.Captures.Begin(document, document.Requests[0], Environment);
        var result = Response("{}");
        first.Captures.Complete(run, result, CancellationToken.None);
        first.Dispose();
        Assert.Same(result, second.Captures.Get("a"));
        second.Dispose();
        Assert.Null(second.Captures.Get("a"));
        var reopened = ODataBufferSession.Attach(buffer.Object);
        Assert.NotSame(second, reopened);
        Assert.Null(reopened.Captures.Get("a"));
        reopened.Dispose();
    }

    [Fact]
    public void RerunningOrEditingADoesNotInvalidateCompletedB()
    {
        var session = new ODataCaptureSession();
        var document = ODataDocument.Parse("### A\n# @name a\nGET /accounts\n### B\n# @name b\nGET /accounts({{a.response.body.$.id}})");
        var a = session.Begin(document, document.Requests[0], Environment);
        session.Complete(a, Response("{\"id\":1}"), CancellationToken.None);
        var b = session.Begin(document, document.Requests[1], Environment);
        var bResult = Response("{\"ok\":true}");
        session.Complete(b, bResult, CancellationToken.None);
        session.Begin(document, document.Requests[0], Environment);
        Assert.Null(session.Get("a"));
        Assert.Same(bResult, session.Get("b"));
        session.Update(ODataDocument.Parse("### A\n# @name a\nGET /contacts\n### B\n# @name b\nGET /accounts({{a.response.body.$.id}})"));
        Assert.Same(bResult, session.Get("b"));
        session.Complete(a, Response("{\"id\":2}"), CancellationToken.None);
        Assert.Null(session.Get("a"));
    }

    [Fact]
    public void MovingOrChangingHeadingPreservesCaptureButVariableEditsInvalidateIt()
    {
        var session = new ODataCaptureSession();
        var document = ODataDocument.Parse("@x = one\n### Before\n# @name a\nGET /{{x}}\n### Other\nGET /WhoAmI");
        var run = session.Begin(document, document.Requests[0], Environment);
        var result = Response("{}");
        session.Complete(run, result, CancellationToken.None);
        session.Update(ODataDocument.Parse("@x = one\n### Other\nGET /WhoAmI\n### After\n# @name a\nGET /{{x}}"));
        Assert.Same(result, session.Get("a"));
        session.Update(ODataDocument.Parse("@x = two\n### Other\nGET /WhoAmI\n### After\n# @name a\nGET /{{x}}"));
        Assert.Null(session.Get("a"));
        session.Complete(run, result, CancellationToken.None);
        Assert.Null(session.Get("a"));
    }

    [Fact]
    public void IncompleteEditingOnlyInvalidatesTheAffectedCapture()
    {
        var session = new ODataCaptureSession();
        var document = ODataDocument.Parse("@x = one\n# @name a\nGET /{{x}}\n###\n# @name b\nGET /accounts");
        var a = session.Begin(document, document.Requests[0], Environment);
        var b = session.Begin(document, document.Requests[1], Environment);
        session.Complete(a, Response("a"), CancellationToken.None);
        var bResult = Response("b");
        session.Complete(b, bResult, CancellationToken.None);
        session.Update(ODataDocument.Parse("@bad =\n# @name a\nGET /{{x}}\n###\n# @name b\nGET /accounts"));
        Assert.Null(session.Get("a"));
        Assert.Same(bResult, session.Get("b"));
        session.Complete(a, Response("old"), CancellationToken.None);
        Assert.Null(session.Get("a"));
    }

    [Fact]
    public void EditingVersionOrRenamingOrDeletingInvalidatesCapture()
    {
        var session = new ODataCaptureSession();
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI");
        var run = session.Begin(document, document.Requests[0], Environment);
        session.Complete(run, Response("{}"), CancellationToken.None);
        session.Update(ODataDocument.Parse("# @name a\nGET /WhoAmI HTTP/1.1"));
        Assert.Null(session.Get("a"));
        document = ODataDocument.Parse("# @name a\nGET /WhoAmI HTTP/1.1");
        run = session.Begin(document, document.Requests[0], Environment);
        session.Complete(run, Response("{}"), CancellationToken.None);
        session.Update(ODataDocument.Parse("# @name renamed\nGET /WhoAmI HTTP/1.1"));
        Assert.Null(session.Get("a"));
        session.Complete(run, Response("old"), CancellationToken.None);
        Assert.Null(session.Get("a"));
        document = ODataDocument.Parse("# @name renamed\nGET /WhoAmI HTTP/1.1");
        run = session.Begin(document, document.Requests[0], Environment);
        session.Update(ODataDocument.Parse("# empty"));
        session.Complete(run, Response("old"), CancellationToken.None);
        Assert.Null(session.Get("renamed"));
    }

    [Fact]
    public void EnvironmentVariableChangesInvalidateOnlyDependentCapturesAndRejectLatePublication()
    {
        var session = new ODataCaptureSession();
        var document = ODataDocument.Parse("# @name dependent\nGET /accounts({{account1}})\n###\n# @name independent\nGET /WhoAmI");
        document.SetEnvironmentVariables(new Dictionary<string, string> { ["account1"] = "one" });
        var dependent = session.Begin(document, document.Requests[0], Environment);
        var independent = session.Begin(document, document.Requests[1], Environment);
        session.Complete(dependent, Response("dependent"), CancellationToken.None);
        var independentResponse = Response("independent");
        session.Complete(independent, independentResponse, CancellationToken.None);

        var changed = ODataDocument.Parse("# @name dependent\nGET /accounts({{account1}})\n###\n# @name independent\nGET /WhoAmI");
        changed.SetEnvironmentVariables(new Dictionary<string, string> { ["account1"] = "two" });
        session.Update(changed);

        Assert.Null(session.Get("dependent"));
        Assert.Same(independentResponse, session.Get("independent"));
        session.Complete(dependent, Response("stale"), CancellationToken.None);
        Assert.Null(session.Get("dependent"));
    }

    [Fact]
    public void CancellationEnvironmentChangeAndClosePreventLatePublication()
    {
        var session = new ODataCaptureSession();
        var document = ODataDocument.Parse("# @name a\nGET /WhoAmI");
        var run = session.Begin(document, document.Requests[0], Environment);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        session.Complete(run, Response("{}"), cts.Token);
        Assert.Null(session.Get("a"));
        session.SetEnvironment(new DataverseEnvironment { ConnectionString = "Url=https://other.crm.dynamics.com;TenantId=test" });
        session.SetEnvironment(Environment);
        session.Complete(run, Response("{}"), CancellationToken.None);
        Assert.Null(session.Get("a"));
        run = session.Begin(document, document.Requests[0], Environment);
        session.Close();
        session.Complete(run, Response("{}"), CancellationToken.None);
        Assert.Null(session.Get("a"));
    }

    [Fact]
    public void CaptureBudgetIncludesHeadersAndDisposesAnOversizedCapture()
    {
        string path = Path.Combine(Path.GetTempPath(), "odata-capture-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(path, "body");
        try
        {
            var session = new ODataCaptureSession();
            var document = ODataDocument.Parse("# @name a\nGET /WhoAmI");
            var run = session.Begin(document, document.Requests[0], Environment);
            var response = new ODataCapturedResponse
            {
                StoredBody = new ODataStoredBody(path, Encoding.UTF8),
                ContentByteLength = 200L * 1024 * 1024 - 512,
                Headers = new Dictionary<string, string> { ["Large"] = new string('x', 512) }
            };
            session.Complete(run, response, CancellationToken.None);
            Assert.Null(session.Get("a"));
            Assert.False(File.Exists(path));
            session.Close();
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
