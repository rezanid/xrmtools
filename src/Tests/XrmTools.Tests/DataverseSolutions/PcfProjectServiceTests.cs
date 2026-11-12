#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools;
using XrmTools.DataverseSolutions;
using XrmTools.Environments;
using XrmTools.UI;

public sealed class PcfProjectServiceTests : IDisposable
{
    private const string Url = "https://org.crm.dynamics.com";
    private readonly string _root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pcf-service-test-" + Guid.NewGuid().ToString("N"));
    private readonly Mock<IPacCli> _pac = new(MockBehavior.Strict);
    private readonly Mock<IPacAuthBridge> _auth = new(MockBehavior.Strict);
    private readonly Mock<IEnvironmentProvider> _environments = new(MockBehavior.Strict);
    private readonly TestUi _ui = new();
    private readonly DataverseSolutionOperationState _state = new();

    public PcfProjectServiceTests() => Directory.CreateDirectory(_root);

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private PcfProjectService Service() => new(_pac.Object, _auth.Object, _environments.Object, _ui, _state);
    private PcfCreateRequest CreateRequest(bool install = false) => new()
    {
        ParentDirectory = _root, ProjectName = "Example Project", ControlName = "ExampleControl",
        Namespace = "Contoso.Controls", Template = "dataset", Framework = "react", InstallDependencies = install
    };
    private PcfPushRequest PushRequest()
    {
        var project = Path.Combine(_root, "Example.pcfproj");
        File.WriteAllText(project, "project");
        return new PcfPushRequest { ProjectFilePath = project, EnvironmentUrl = Url, SolutionUniqueName = "MySolution", Incremental = true };
    }
    private static DataverseEnvironment Environment(string url = Url) => new() { ConnectionString = url };
    private void ActiveEnvironment(DataverseEnvironment environment)
    {
        _environments.Setup(x => x.GetActiveEnvironmentAsync(It.IsAny<bool>())).ReturnsAsync(environment);
    }
    private void Authenticates()
    {
        _auth.Setup(x => x.EnsurePacProfileForCurrentEnvironmentAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }
    private static ProcessCommandResult Success() => new() { ExitCode = 0 };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAsync_InvokesPacWithExactOptions_WithoutAuthentication(bool install)
    {
        var request = CreateRequest(install);
        var directory = Path.Combine(_root, request.ProjectName);
        var project = Path.Combine(directory, "generated.pcfproj");
        IReadOnlyList<string>? arguments = null;
        using var cts = new CancellationTokenSource();
        _pac.Setup(x => x.RunPcfCommandAsync(directory, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), cts.Token))
            .Callback<string, IReadOnlyList<string>, IProgress<ProcessOutputLine>, CancellationToken>((_, args, _, _) => { arguments = args; File.WriteAllText(project, "generated"); })
            .ReturnsAsync(Success());

        var service = Service();
        var result = await service.CreateAsync(request, cts.Token);

        result.Should().Be(project);
        var expected = new List<string> { "pcf", "init", "--namespace", "Contoso.Controls", "--name", "ExampleControl", "--template", "dataset", "--framework", "react", "--outputDirectory", directory };
        if (install) expected.Add("--run-npm-install");
        arguments.Should().Equal(expected);
        _pac.VerifyAll();
        _auth.VerifyNoOtherCalls(); _environments.VerifyNoOtherCalls();
        _ui.Completed.Should().Be(1); _ui.Starts.Should().Be(1); _ui.Ends.Should().Be(1);
        service.IsBusy.Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateAsync_FailureOrCancellation_RetainsFilesAndReleasesBusy(bool cancel)
    {
        var request = CreateRequest();
        var directory = Path.Combine(_root, request.ProjectName);
        var generated = Path.Combine(directory, "generated.pcfproj");
        _pac.Setup(x => x.RunPcfCommandAsync(directory, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), It.IsAny<CancellationToken>()))
            .Callback(() => File.WriteAllText(generated, "partial"))
            .Returns(() => cancel ? Task.FromException<ProcessCommandResult>(new OperationCanceledException()) : Task.FromException<ProcessCommandResult>(new InvalidOperationException("pac failed")));
        var service = Service();
        var model = new PcfCreateDialogViewModel(_root) { ProjectName = request.ProjectName, ControlName = request.ControlName, Namespace = request.Namespace, Template = request.Template, Framework = request.Framework };

        var result = await model.CreateAsync(service.CreateAsync, CancellationToken.None);

        result.Should().BeNull();
        File.ReadAllText(generated).Should().Be("partial");
        if (cancel) model.Status.Should().Contain("canceled"); else model.Status.Should().Contain("retained").And.Contain("pac failed");
        service.IsBusy.Should().BeFalse(); _ui.Completed.Should().Be(0); _ui.Ends.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_AlreadyCanceled_DoesNotCreateDirectoryOrRunPac()
    {
        using var cts = new CancellationTokenSource(); cts.Cancel();
        var request = CreateRequest(); var service = Service();
        Func<Task> act = async () => await service.CreateAsync(request, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        Directory.Exists(Path.Combine(_root, request.ProjectName)).Should().BeFalse();
        service.IsBusy.Should().BeFalse(); _pac.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_RejectsBusyOperationWithoutTouchingDestination()
    {
        _state.TryEnter().Should().BeTrue();
        try
        {
            var request = CreateRequest();
            Func<Task> act = async () => await Service().CreateAsync(request, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already running*");
            Directory.Exists(Path.Combine(_root, request.ProjectName)).Should().BeFalse();
            _pac.VerifyNoOtherCalls();
        }
        finally { _state.Exit(); }
    }

    [Fact]
    public async Task PushAsync_UsesSelectedProjectDirectoryAndExplicitEnvironment()
    {
        var request = PushRequest(); ActiveEnvironment(Environment()); Authenticates();
        IReadOnlyList<string>? arguments = null;
        using var cts = new CancellationTokenSource();
        _pac.Setup(x => x.RunPcfCommandAsync(_root, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), cts.Token))
            .Callback<string, IReadOnlyList<string>, IProgress<ProcessOutputLine>, CancellationToken>((_, args, _, _) => arguments = args)
            .ReturnsAsync(Success());
        var service = Service();

        await service.PushAsync(request, cts.Token);

        arguments.Should().Equal("pcf", "push", "--environment", Url, "--solution-unique-name", "MySolution", "--incremental");
        _auth.Verify(x => x.EnsurePacProfileForCurrentEnvironmentAsync(cts.Token), Times.Once);
        _pac.VerifyAll(); _ui.Completed.Should().Be(1); service.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task PushAsync_RejectsConflictingOptionsBeforeAuthentication()
    {
        var request = PushRequest();
        request.PublisherPrefix = "dev";
        Func<Task> act = () => Service().PushAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*not both*");
        _auth.VerifyNoOtherCalls(); _pac.VerifyNoOtherCalls(); _environments.VerifyNoOtherCalls();
        _state.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task PushAsync_ReportsAuthenticationAndLivePacOutput()
    {
        var request = PushRequest(); ActiveEnvironment(Environment());
        var progress = new RecordingProgress();
        _auth.Setup(x => x.EnsurePacProfileForCurrentEnvironmentAsync(It.IsAny<CancellationToken>()))
            .Callback(() => progress.Messages.Last().Should().Contain("browser sign-in"))
            .Returns(Task.CompletedTask);
        var line = new ProcessOutputLine(ProcessOutputSource.StandardOutput, "Importing PCF control...");
        _pac.Setup(x => x.RunPcfCommandAsync(_root, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), It.IsAny<CancellationToken>()))
            .Callback<string, IReadOnlyList<string>, IProgress<ProcessOutputLine>, CancellationToken>((_, _, output, _) => output.Report(line))
            .ReturnsAsync(Success());

        await Service().PushAsync(request, CancellationToken.None, progress);

        progress.Messages.Should().Contain(message => message.Contains("Building and pushing"));
        progress.Messages.Last().Should().Be(line.Text);
        _ui.Lines.Should().Contain(line);
        _ui.Shows.Should().Be(1);
    }

    [Fact]
    public async Task PushAsync_ExpiredAuthentication_ReauthenticatesAndRetriesOnce()
    {
        var request = PushRequest(); ActiveEnvironment(Environment()); Authenticates();
        _auth.Setup(x => x.ReauthenticatePacProfileForCurrentEnvironmentAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failure = new PacCommandFailedException("expired", new ProcessCommandResult { ExitCode = 1, Output = [new ProcessOutputLine(ProcessOutputSource.StandardError, "AADSTS700082")] });
        _pac.SetupSequence(x => x.RunPcfCommandAsync(_root, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(failure).ReturnsAsync(Success());
        var service = Service();
        var progress = new RecordingProgress();

        await service.PushAsync(request, CancellationToken.None, progress);

        progress.Messages.Should().Contain(message => message.Contains("authentication expired"));
        progress.Messages.Should().Contain(message => message.Contains("Retrying the PCF"));
        _pac.Verify(x => x.RunPcfCommandAsync(_root, It.Is<IReadOnlyList<string>>(args => args.Contains(Url)), It.IsAny<IProgress<ProcessOutputLine>>(), CancellationToken.None), Times.Exactly(2));
        _auth.Verify(x => x.ReauthenticatePacProfileForCurrentEnvironmentAsync(CancellationToken.None), Times.Once);
        _ui.Completed.Should().Be(1); _ui.Ends.Should().Be(1); service.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task PushAsync_EnvironmentChangesDuringAuthentication_DoesNotRunPac()
    {
        var request = PushRequest();
        _environments.SetupSequence(x => x.GetActiveEnvironmentAsync(It.IsAny<bool>()))
            .ReturnsAsync(Environment()).ReturnsAsync(Environment("https://other.crm.dynamics.com"));
        Authenticates(); var service = Service();
        Func<Task> act = () => service.PushAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*changed before deployment*");
        _pac.VerifyNoOtherCalls(); service.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task PushAsync_CanceledDuringAuthentication_DoesNotRunPac()
    {
        var request = PushRequest(); ActiveEnvironment(Environment());
        using var cts = new CancellationTokenSource();
        _auth.Setup(x => x.EnsurePacProfileForCurrentEnvironmentAsync(cts.Token)).Callback(() => cts.Cancel()).Returns(Task.CompletedTask);
        var service = Service(); Func<Task> act = () => service.PushAsync(request, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
        _pac.VerifyNoOtherCalls(); service.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task PushAsync_EnvironmentChangesDuringReauthentication_DoesNotRetry()
    {
        var request = PushRequest();
        _environments.SetupSequence(x => x.GetActiveEnvironmentAsync(It.IsAny<bool>()))
            .ReturnsAsync(Environment()).ReturnsAsync(Environment()).ReturnsAsync(Environment())
            .ReturnsAsync(Environment("https://other.crm.dynamics.com"));
        Authenticates();
        _auth.Setup(x => x.ReauthenticatePacProfileForCurrentEnvironmentAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var failure = new PacCommandFailedException("expired", new ProcessCommandResult { ExitCode = 1, Output = [new ProcessOutputLine(ProcessOutputSource.StandardError, "AADSTS700082")] });
        _pac.Setup(x => x.RunPcfCommandAsync(_root, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), It.IsAny<CancellationToken>())).ThrowsAsync(failure);
        var service = Service(); Func<Task> act = () => service.PushAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*changed before retry*");
        _pac.Verify(x => x.RunPcfCommandAsync(_root, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), CancellationToken.None), Times.Once);
        _ui.Completed.Should().Be(0); _ui.Ends.Should().Be(1); service.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task PushAsync_CanceledByPac_ReleasesBusyAndStopsAnimation()
    {
        var request = PushRequest(); ActiveEnvironment(Environment()); Authenticates();
        _pac.Setup(x => x.RunPcfCommandAsync(_root, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IProgress<ProcessOutputLine>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        var service = Service(); Func<Task> act = () => service.PushAsync(request, CancellationToken.None);
        await act.Should().ThrowAsync<OperationCanceledException>();
        _auth.Verify(x => x.ReauthenticatePacProfileForCurrentEnvironmentAsync(It.IsAny<CancellationToken>()), Times.Never);
        _ui.Completed.Should().Be(0); _ui.Ends.Should().Be(1); service.IsBusy.Should().BeFalse();
    }

    private sealed class TestUi : IPcfOperationUi
    {
        public int Starts { get; private set; }
        public int Ends { get; private set; }
        public int Completed { get; private set; }
        public int Shows { get; private set; }
        public List<ProcessOutputLine> Lines { get; } = [];
        public Task ShowPaneAsync() { Shows++; return Task.CompletedTask; }
        public Task StartAsync() { Starts++; return Task.CompletedTask; }
        public Task EndAsync() { Ends++; return Task.CompletedTask; }
        public void WriteHeader(CdsProjectInfo project, string? environmentUrl, string command) { }
        public IProgress<ProcessOutputLine> CreateProgress() => new RecordingOutput(Lines);
        public void WriteCompleted() => Completed++;
        public void WriteWarning(string message) { }
    }

    private sealed class RecordingProgress : IProgress<string>
    {
        public List<string> Messages { get; } = [];
        public void Report(string value) => Messages.Add(value);
    }

    private sealed class RecordingOutput(List<ProcessOutputLine> lines) : IProgress<ProcessOutputLine>
    {
        public void Report(ProcessOutputLine value) => lines.Add(value);
    }
}
#nullable restore
