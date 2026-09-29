namespace XrmTools.Tests.UI;

using CommunityToolkit.Mvvm.Input;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using XrmTools.Authentication;
using XrmTools.Http;
using XrmTools.UI;

public class EnvironmentEditorViewModelTests
{
    [Fact]
    public void EditingOrSwitchingEnvironments_ClearsPreviousConnectionResult()
    {
        var vm = new EnvironmentEditorViewModel();
        vm.TestResult = "Connection succeeded.";
        vm.SelectedEnvironment.EnvironmentUrl = "https://changed.crm.dynamics.com";
        Assert.Null(vm.TestResult);

        vm.TestResult = "Connection succeeded.";
        vm.AddEnvironmentCommand.Execute(null);
        Assert.Null(vm.TestResult);
    }

    [Fact]
    public void RemovingLastEnvironment_DisablesSelectionActionsButAllowsAdding()
    {
        var vm = new EnvironmentEditorViewModel();
        vm.RemoveEnvironmentCommand.Execute(null);
        Assert.Null(vm.SelectedEnvironment);
        Assert.False(vm.TestConnectionCommand.CanExecute(null));
        Assert.False(vm.ResetSignInCommand.CanExecute(null));
        Assert.False(vm.SetActiveEnvironmentCommand.CanExecute(null));
        Assert.True(vm.AddEnvironmentCommand.CanExecute(null));
    }

    [Fact]
    public async Task Disconnect_PreventsConcurrentEditsAndReenablesActionsAfterCompletion()
    {
        var completion = new TaskCompletionSource<bool>();
        var cache = new Mock<IAuthenticationCacheService>();
        var factory = new Mock<IXrmHttpClientFactory>();
        cache.Setup(c => c.ClearEnvironmentTokenCacheAsync(It.IsAny<DataverseEnvironment>(), It.IsAny<CancellationToken>()))
            .Returns(completion.Task);
        var vm = new EnvironmentEditorViewModel(null, factory.Object, cache.Object);
        vm.AddEnvironmentCommand.Execute(null);
        vm.SelectedEnvironment.EnvironmentUrl = "https://contoso.crm.dynamics.com";

        var operation = ((IAsyncRelayCommand)vm.ResetSignInCommand).ExecuteAsync(null);
        Assert.False(vm.IsIdle);
        Assert.False(vm.AddEnvironmentCommand.CanExecute(null));
        Assert.False(vm.RemoveEnvironmentCommand.CanExecute(null));
        Assert.False(vm.TestConnectionCommand.CanExecute(null));
        Assert.False(vm.SaveCommand.CanExecute(null));
        Assert.False(vm.CancelCommand.CanExecute(null));

        completion.SetResult(true);
        await operation;
        Assert.True(vm.IsIdle);
        Assert.True(vm.AddEnvironmentCommand.CanExecute(null));
        Assert.True(vm.SaveCommand.CanExecute(null));
        Assert.Contains("Sign-in reset", vm.TestResult);
        factory.Verify(f => f.InvalidateAuthenticationCache(It.IsAny<DataverseEnvironment>()), Times.Once);
    }
}
