#nullable enable
namespace XrmTools.Tests.DataverseSolutions;

using FluentAssertions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.DataverseSolutions;

public sealed class DataverseSolutionOperationStateTests
{
    [Fact]
    public void TryEnter_AllowsOnlyOneConcurrentOperation()
    {
        var state = new DataverseSolutionOperationState();
        var entered = 0;

        Parallel.For(0, 20, _ =>
        {
            if (state.TryEnter()) Interlocked.Increment(ref entered);
        });

        entered.Should().Be(1);
        state.IsBusy.Should().BeTrue();
        state.Exit();
        state.IsBusy.Should().BeFalse();
        state.TryEnter().Should().BeTrue();
        state.Exit();
    }

    [Fact]
    public async Task Services_ShareBusyState_AndRejectedCreationDoesNotReleaseRunningOperation()
    {
        var state = new DataverseSolutionOperationState();
        var commands = new DataverseSolutionCommandService(null!, null!, null!, null!, null!, null!, null!, state, null!);
        var creation = new DataverseSolutionProjectCreationService(null!, null!, null!, null!, null!, state);
        state.TryEnter().Should().BeTrue();

        commands.IsBusy.Should().BeTrue();
        creation.IsBusy.Should().BeTrue();
        Func<Task> create = () => creation.CreateAsync(new DataverseSolutionProjectCreationRequest(), CancellationToken.None);
        await create.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already running*");
        state.IsBusy.Should().BeTrue();

        state.Exit();
        commands.IsBusy.Should().BeFalse();
        creation.IsBusy.Should().BeFalse();
    }

    [Fact]
    public async Task Creation_ValidationFailure_ReleasesSharedBusyState()
    {
        var state = new DataverseSolutionOperationState();
        var creation = new DataverseSolutionProjectCreationService(null!, null!, null!, null!, null!, state);
        var parent = Path.Combine(Path.GetTempPath(), "xrmtools-operation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(parent, "Existing"));
        try
        {
            Func<Task> create = () => creation.CreateAsync(new DataverseSolutionProjectCreationRequest
            {
                ParentDirectory = parent,
                ProjectName = "Existing"
            }, CancellationToken.None);

            await create.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
            state.IsBusy.Should().BeFalse();
            state.TryEnter().Should().BeTrue();
            state.Exit();
        }
        finally
        {
            Directory.Delete(parent, recursive: true);
        }
    }
}
#nullable restore
