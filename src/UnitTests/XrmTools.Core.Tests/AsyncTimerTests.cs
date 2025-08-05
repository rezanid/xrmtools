namespace XrmTools.Core.Tests;
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Time.Testing;

public class AsyncTimerTests
{
    [Fact]
    public async Task Timer_Should_Invoke_Callback_Periodically()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var callCount = 0;

        var timer = new AsyncTimer(async _ =>
        {
            callCount++;
            await Task.CompletedTask;
        }, fakeTimeProvider);

        // Act
        timer.Start(TimeSpan.FromSeconds(1));
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1)); // Trigger first tick
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1)); // Trigger second tick
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1)); // Trigger third tick

        // Assert
        callCount.Should().Be(3);

        // Cleanup
        timer.Dispose();
    }

    [Fact]
    public async Task Timer_Should_Stop_Gracefully()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var callCount = 0;

        var timer = new AsyncTimer(async _ =>
        {
            callCount++;
            await Task.CompletedTask;
        }, fakeTimeProvider);

        // Act
        timer.Start(TimeSpan.FromSeconds(1));
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1)); // Trigger first tick
        timer.Stop();
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1)); // This should not trigger

        // Assert
        callCount.Should().Be(1);

        // Cleanup
        timer.Dispose();
    }

    [Fact]
    public async Task Timer_Should_Handle_Cancellation()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var wasCancelled = false;

        var timer = new AsyncTimer(async token =>
        {
            try
            {
                await Task.Delay(5000, token); // Simulate long-running task
            }
            catch (TaskCanceledException)
            {
                wasCancelled = true;
            }
        }, fakeTimeProvider);

        // Act
        timer.Start(TimeSpan.FromSeconds(1));
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(1)); // Start the timer
        timer.Stop();

        // Assert
        wasCancelled.Should().BeTrue();

        // Cleanup
        timer.Dispose();
    }

    [Fact]
    public void Timer_Should_Throw_Exception_If_Already_Started()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var timer = new AsyncTimer(_ => Task.CompletedTask, fakeTimeProvider);

        // Act
        timer.Start(TimeSpan.FromSeconds(1));
        Action act = () => timer.Start(TimeSpan.FromSeconds(1)); // Start again

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Timer is already running.");

        // Cleanup
        timer.Dispose();
    }
}