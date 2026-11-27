namespace XrmTools.Tests.OData;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Moq;
using XrmTools;
using XrmTools.Environments;
using XrmTools.Http;
using XrmTools.OData;
using Xunit;

public class ODataExecutionCoordinatorTests
{
    private static readonly DataverseEnvironment Environment = new() { ConnectionString = "Url=https://contoso.crm.dynamics.com;TenantId=test" };
    private static readonly DataverseEnvironment Other = new() { ConnectionString = "Url=https://other.crm.dynamics.com;TenantId=test" };
    private const string Source = "# @name a\nGET /accounts\n###\n# @name b\nGET /accounts({{a.response.body.$.id}})\n###\nGET /WhoAmI";
    private static TaskCompletionSource<bool> Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static ODataResponse Ok(string body = "{}") => new() { StatusCode = 200, Status = "200 OK", Body = body, CanCapture = true, Principal = "user" };

    private static async Task Finished(ODataExecutionCoordinator coordinator)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (coordinator.IsExecuting && DateTime.UtcNow < deadline) await Task.Delay(10);
        Assert.False(coordinator.IsExecuting);
    }

    [Fact]
    public async Task RunUsesSnapshotAndIsolatedChainingWithoutChangingManualCaptures()
    {
        var document = ODataDocument.Parse(Source);
        var manual = new ODataCaptureSession();
        var oldRun = manual.Begin(document, document.Requests[0], Environment);
        var previous = new ODataCapturedResponse { Body = "{\"id\":\"old\"}", Principal = "user" };
        manual.Complete(oldRun, previous, CancellationToken.None);
        var entered = Signal(); var release = Signal();
        var seen = new List<string>();
        using var coordinator = new ODataExecutionCoordinator(async (snapshot, request, environment, token, captures, run) =>
        {
            seen.Add(request.Target);
            if (request.RequestName == "a") { entered.SetResult(true); await release.Task; return Ok("{\"id\":\"new\"}"); }
            if (request.RequestName == "b") Assert.Equal("new", snapshot.Resolve(request, captures.Get, _ => { }, token).Target.Split('(')[1].TrimEnd(')'));
            return Ok();
        }, () => Task.FromResult<DataverseEnvironment>(Environment), manual);
        Assert.True(coordinator.TryStartDocument(document, Environment));
        await entered.Task;
        document.Requests[1].Target = "/changed";
        manual.AdvanceEpoch();
        Assert.False(coordinator.TryStartManual(document, document.Requests[0], Environment));
        release.SetResult(true);
        await Finished(coordinator);
        Assert.Equal(new[] { "/accounts", "/accounts({{a.response.body.$.id}})", "/WhoAmI" }, seen);
        Assert.All(coordinator.Result.Rows, row => Assert.Equal("Succeeded", row.State));
        Assert.Same(previous, manual.Get("a"));
        manual.Close();
    }

    [Fact]
    public async Task FailedResponseStopsAndSkipsFollowingRows()
    {
        int count = 0;
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) =>
        {
            count++;
            return Task.FromResult(new ODataResponse { StatusCode = 400, Status = "400 Bad Request" });
        }, () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await Finished(coordinator);
        Assert.Equal(1, count);
        Assert.Equal(new[] { "Failed", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(r => r.State));
        Assert.Equal("400 Bad Request", coordinator.Result.Rows[0].Message);
    }

    [Fact]
    public async Task CancellationHonorsExecutionIdAndStopsSubsequentRows()
    {
        var entered = Signal();
        using var coordinator = new ODataExecutionCoordinator(async (doc, request, env, token, captures, run) =>
        {
            entered.SetResult(true);
            await Task.Delay(Timeout.Infinite, token);
            return Ok();
        }, () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await entered.Task;
        coordinator.Cancel(Guid.NewGuid());
        Assert.True(coordinator.IsExecuting);
        coordinator.Cancel(coordinator.ActiveId);
        await Finished(coordinator);
        Assert.Equal(new[] { "Canceled", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(r => r.State));
    }

    [Fact]
    public async Task TransportTimeoutPreservesCanceledPresentationAndSkipsRemainingRequests()
    {
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) =>
            throw new OperationCanceledException(), () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await Finished(coordinator);
        Assert.Equal(new[] { "Canceled", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(row => row.State));
        Assert.Equal("Timed out or canceled.", coordinator.Result.StopReason);
    }

    [Fact]
    public async Task TransientEnvironmentChangePermanentlyStopsScheduling()
    {
        var entered = Signal(); var release = Signal(); int count = 0;
        using var coordinator = new ODataExecutionCoordinator(async (doc, request, env, token, captures, run) =>
        {
            count++; entered.SetResult(true); await release.Task; return Ok();
        }, () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await entered.Task;
        coordinator.EnvironmentChanged(Other);
        coordinator.EnvironmentChanged(Environment);
        release.SetResult(true);
        await Finished(coordinator);
        Assert.Equal(1, count);
        Assert.Equal(new[] { "Succeeded", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(r => r.State));
        Assert.Contains("environment changed", coordinator.Result.StopReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ManualCaptureIsSharedAndResultReplacementReleasesStorage()
    {
        string path = Path.Combine(System.Environment.CurrentDirectory, "coordinator-" + Guid.NewGuid().ToString("N") + ".body");
        File.WriteAllText(path, "body");
        var captures = new ODataCaptureSession();
        var document = ODataDocument.Parse(Source);
        int count = 0;
        try
        {
            using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, session, run) =>
            {
                count++;
                return Task.FromResult(count == 1 ? new ODataResponse { StatusCode = 200, StoredBody = new ODataStoredBody(path, Encoding.UTF8), ContentByteLength = 4, CanCapture = true } : Ok());
            }, () => Task.FromResult<DataverseEnvironment>(Environment), captures);
            Assert.True(coordinator.TryStartManual(document, document.Requests[0], Environment));
            await Finished(coordinator);
            Assert.NotNull(captures.Get("a"));
            Assert.True(File.Exists(path));
            captures.Begin(document, document.Requests[0], Environment);
            Assert.True(File.Exists(path));
            Assert.True(coordinator.TryStartManual(document, document.Requests[1], Environment));
            await Finished(coordinator);
            Assert.False(File.Exists(path));
            captures.Close();
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task OversizedRetainedBodyIsReleasedAndReportedUnavailable()
    {
        string path = Path.Combine(System.Environment.CurrentDirectory, "coordinator-" + Guid.NewGuid().ToString("N") + ".body");
        File.WriteAllText(path, "body");
        try
        {
            using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) =>
                Task.FromResult(new ODataResponse { StatusCode = 200, StoredBody = new ODataStoredBody(path, Encoding.UTF8), ContentByteLength = 201L * 1024 * 1024, Body = "preview" }),
                () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
            var document = ODataDocument.Parse("GET /WhoAmI");
            Assert.True(coordinator.TryStartManual(document, document.Requests[0], Environment));
            await Finished(coordinator);
            Assert.False(coordinator.Result.Rows[0].BodyAvailable);
            Assert.Equal("", coordinator.Result.Rows[0].Response.Body);
            Assert.False(File.Exists(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public void BufferSharesCoordinatorUntilLastClose()
    {
        var snapshot = new Mock<ITextSnapshot>(); snapshot.Setup(s => s.GetText()).Returns(Source);
        var buffer = new Mock<ITextBuffer>(); buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
        buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
        var first = ODataBufferSession.Attach(buffer.Object);
        var second = ODataBufferSession.Attach(buffer.Object);
        var execution = new ODataExecutionService(Mock.Of<IEnvironmentSelection>(), Mock.Of<IXrmHttpClientFactory>(), Mock.Of<IODataTransport>());
        var coordinator = first.GetCoordinator(execution);
        Assert.Same(coordinator, second.GetCoordinator(execution));
        first.Dispose();
        Assert.Same(coordinator, second.GetCoordinator(execution));
        second.Dispose();
        Assert.False(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        Assert.Same(coordinator, second.GetCoordinator(execution));
    }

    [Fact]
    public async Task DisposeCancelsInFlightWorkAndRejectsFutureStarts()
    {
        var entered = Signal(); var canceled = Signal();
        var coordinator = new ODataExecutionCoordinator(async (doc, request, env, token, captures, run) =>
        {
            entered.SetResult(true);
            try { await Task.Delay(Timeout.Infinite, token); }
            finally { canceled.TrySetResult(true); }
            return Ok();
        }, () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await entered.Task;
        coordinator.Dispose();
        await canceled.Task;
        Assert.False(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        Assert.Null(coordinator.Result);
    }

    [Fact]
    public async Task ChangedSelectionAndExecutionExceptionStopSubsequentRows()
    {
        int count = 0;
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) =>
        {
            count++;
            return Task.FromResult(Ok());
        }, () => Task.FromResult<DataverseEnvironment>(Other), new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await Finished(coordinator);
        Assert.Equal(1, count);
        Assert.Equal(new[] { "Succeeded", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(r => r.State));
        Assert.Contains("environment changed", coordinator.Result.StopReason, StringComparison.OrdinalIgnoreCase);
        using var failing = new ODataExecutionCoordinator((doc, request, env, token, captures, run) => throw new InvalidOperationException("failed"),
            () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        Assert.True(failing.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await Finished(failing);
        Assert.Equal("failed", failing.Result.Rows[0].Message);
        Assert.Equal(new[] { "Failed", "Skipped", "Skipped" }, failing.Result.Rows.Select(r => r.State));
    }

    [Fact]
    public async Task ManualCaptureCannotSatisfyAFirstRunReference()
    {
        var manual = new ODataCaptureSession();
        var producer = ODataDocument.Parse("# @name a\nGET /accounts");
        var run = manual.Begin(producer, producer.Requests[0], Environment);
        var existing = new ODataCapturedResponse { Body = "{\"id\":\"manual\"}" };
        manual.Complete(run, existing, CancellationToken.None);
        var document = ODataDocument.Parse("GET /accounts({{a.response.body.$.id}})");
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, executionRun) =>
        {
            doc.Resolve(request, captures.Get, _ => { }, token);
            return Task.FromResult(Ok());
        }, () => Task.FromResult<DataverseEnvironment>(Environment), manual);
        Assert.True(coordinator.TryStartDocument(document, Environment));
        await Finished(coordinator);
        Assert.Equal("Failed", coordinator.Result.Rows[0].State);
        Assert.Contains("No captured response", coordinator.Result.Rows[0].Message);
        Assert.Same(existing, manual.Get("a"));
        manual.Close();
    }

    [Fact]
    public async Task CancelAfterCompletedRowBeforeNextDispatchSkipsPendingRows()
    {
        var checking = Signal(); var release = Signal();
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) => Task.FromResult(Ok()),
            async () => { checking.SetResult(true); await release.Task; return Environment; }, new ODataCaptureSession());
        Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        await checking.Task;
        coordinator.Cancel(coordinator.ActiveId);
        release.SetResult(true);
        await Finished(coordinator);
        Assert.Equal(new[] { "Succeeded", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(r => r.State));
        Assert.Equal("Canceled.", coordinator.Result.StopReason);
    }

    [Theory]
    [InlineData("Headers")]
    [InlineData("Request")]
    [InlineData("Raw")]
    public async Task AllResponseTextCountsAgainstRetentionBudget(string field)
    {
        string path = Path.Combine(System.Environment.CurrentDirectory, "coordinator-" + Guid.NewGuid().ToString("N") + ".body");
        File.WriteAllText(path, "body");
        try
        {
            var response = new ODataResponse { StatusCode = 200, Status = "200 OK", Body = "preview", StoredBody = new ODataStoredBody(path, Encoding.UTF8), ContentByteLength = 200L * 1024 * 1024 - 512 };
            if (field == "Headers") response.Headers = new string('h', 512);
            else if (field == "Request") response.Request = new string('r', 512);
            else response.Raw = new string('x', 512);
            using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) => Task.FromResult(response),
                () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
            var document = ODataDocument.Parse("GET /WhoAmI");
            Assert.True(coordinator.TryStartManual(document, document.Requests[0], Environment));
            await Finished(coordinator);
            Assert.False(coordinator.Result.Rows[0].BodyAvailable);
            Assert.Contains("unavailable", response.Raw);
            Assert.Contains("unavailable", response.Headers);
            Assert.Contains("unavailable", response.Request);
            Assert.False(File.Exists(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task CompletedResultSurvivesCancelButCloseDisposesItsBody()
    {
        string path = Path.Combine(System.Environment.CurrentDirectory, "coordinator-" + Guid.NewGuid().ToString("N") + ".body");
        File.WriteAllText(path, "body");
        try
        {
            var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) =>
                Task.FromResult(new ODataResponse { StatusCode = 200, StoredBody = new ODataStoredBody(path, Encoding.UTF8), ContentByteLength = 4 }),
                () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
            var document = ODataDocument.Parse("GET /WhoAmI");
            Assert.True(coordinator.TryStartManual(document, document.Requests[0], Environment));
            var id = coordinator.ActiveId;
            await Finished(coordinator);
            var result = coordinator.Result;
            coordinator.Cancel(id);
            Assert.Null(result.StopReason);
            Assert.Equal("Succeeded", result.Rows[0].State);
            Assert.True(File.Exists(path));
            coordinator.Dispose();
            Assert.Null(coordinator.Result);
            Assert.Null(result.Rows[0].Response);
            Assert.False(File.Exists(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Fact]
    public async Task ClosingLastBufferViewReleasesCompletedResponse()
    {
        string path = Path.Combine(System.Environment.CurrentDirectory, "coordinator-" + Guid.NewGuid().ToString("N") + ".body");
        File.WriteAllText(path, "body");
        try
        {
            var snapshot = new Mock<ITextSnapshot>(); snapshot.Setup(s => s.GetText()).Returns("GET /WhoAmI");
            var buffer = new Mock<ITextBuffer>(); buffer.SetupGet(b => b.CurrentSnapshot).Returns(snapshot.Object);
            buffer.SetupGet(b => b.Properties).Returns(new PropertyCollection());
            var first = ODataBufferSession.Attach(buffer.Object);
            var second = ODataBufferSession.Attach(buffer.Object);
            var selection = new Mock<IEnvironmentSelection>();
            selection.Setup(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(Environment);
            var authentication = new Mock<IXrmHttpClientFactory>();
            authentication.Setup(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(UserToken("alice"));
            var transport = new Mock<IODataTransport>();
            transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ODataResponse { StatusCode = 200, StoredBody = new ODataStoredBody(path, Encoding.UTF8), ContentByteLength = 4 });
            var coordinator = first.GetCoordinator(new ODataExecutionService(selection.Object, authentication.Object, transport.Object));
            var document = ODataDocument.Parse("GET /WhoAmI");
            Assert.True(coordinator.TryStartManual(document, document.Requests[0], Environment));
            await Finished(coordinator);
            Assert.True(File.Exists(path));
            first.Dispose();
            Assert.True(File.Exists(path));
            second.Dispose();
            Assert.Null(coordinator.Result);
            Assert.False(File.Exists(path));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RealServiceChainsOnlyWhenPrincipalMatches(bool matching)
    {
        var selection = new Mock<IEnvironmentSelection>();
        selection.Setup(s => s.GetSelectedEnvironmentAsync()).ReturnsAsync(Environment);
        var authentication = new Mock<IXrmHttpClientFactory>();
        authentication.SetupSequence(a => a.PreAuthenticateAsync(It.IsAny<DataverseEnvironment>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserToken("alice")).ReturnsAsync(UserToken(matching ? "alice" : "bob"));
        var transport = new Mock<IODataTransport>();
        var targets = new List<string>();
        transport.Setup(t => t.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns((HttpRequestMessage request, CancellationToken token) =>
            {
                targets.Add(request.RequestUri.AbsolutePath);
                return Task.FromResult(Ok("{\"id\":\"123\"}"));
            });
        var execution = new ODataExecutionService(selection.Object, authentication.Object, transport.Object);
        using var coordinator = new ODataExecutionCoordinator(execution, new ODataCaptureSession());
        var document = ODataDocument.Parse("# @name a\nGET /accounts\n###\nGET /accounts({{a.response.body.$.id}})");
        Assert.True(coordinator.TryStartDocument(document, Environment));
        await Finished(coordinator);
        Assert.Equal(matching ? 2 : 1, targets.Count);
        Assert.Equal(matching ? "Succeeded" : "Failed", coordinator.Result.Rows[1].State);
        if (matching) Assert.EndsWith("/accounts(123)", targets[1]);
        else Assert.Contains("identity", coordinator.Result.Rows[1].Message);
    }

    [Fact]
    public void CancelBeforeFirstDispatchSkipsEveryRow()
    {
        var context = new QueuedContext();
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) => throw new Exception("must not dispatch"),
            () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        var previous = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(Source), Environment));
        }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
        coordinator.Cancel(coordinator.ActiveId);
        context.RunOne();
        Assert.False(coordinator.IsExecuting);
        Assert.Equal(new[] { "Skipped", "Skipped", "Skipped" }, coordinator.Result.Rows.Select(r => r.State));
        Assert.Equal("Canceled.", coordinator.Result.StopReason);
    }

    [Fact]
    public void ChangedSubscriberExceptionsSurfaceOnCapturedContext()
    {
        var context = new QueuedContext();
        using var coordinator = new ODataExecutionCoordinator((doc, request, env, token, captures, run) => Task.FromResult(Ok()),
            () => Task.FromResult<DataverseEnvironment>(Environment), new ODataCaptureSession());
        coordinator.Changed += (_, _) => throw new InvalidOperationException("subscriber");
        var previous = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(context);
            Assert.True(coordinator.TryStartDocument(ODataDocument.Parse(""), Environment));
        }
        finally { SynchronizationContext.SetSynchronizationContext(previous); }
        context.RunOne(); // The empty execution completes and posts its notification.
        var error = Assert.Throws<InvalidOperationException>(() => context.RunOne());
        Assert.Equal("subscriber", error.Message);
        Assert.Empty(coordinator.Result.Rows);
    }

    private sealed class QueuedContext : SynchronizationContext
    {
        private readonly Queue<(SendOrPostCallback Callback, object State)> callbacks = new();
        public override void Post(SendOrPostCallback callback, object state) => callbacks.Enqueue((callback, state));
        public void RunOne()
        {
            var item = callbacks.Dequeue();
            item.Callback(item.State);
        }
    }

    private static AuthenticationResult UserToken(string name)
    {
        var account = new Mock<IAccount>();
        account.SetupGet(a => a.HomeAccountId).Returns(new AccountId(name, name, "tenant"));
        return new AuthenticationResult("token", false, "id", DateTimeOffset.UtcNow.AddMinutes(10), DateTimeOffset.UtcNow.AddMinutes(10), "tenant", account.Object, null, [], Guid.Empty);
    }
}
