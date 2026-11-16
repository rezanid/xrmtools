#nullable enable
namespace XrmTools.UI;

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using XrmTools.Pcf.Setup;
using XrmTools.Shell.Controls;

internal partial class PcfSetupDialog : DialogWindow
{
    private CancellationToken token;
    private CancellationTokenSource? operation;
    private PcfInstanceRequest? request;
    private bool loaded;
    private PcfSetupDialog() => InitializeComponent();

    /// <returns>The restart the developer chose, to be carried out after this dialog has closed; otherwise <c>null</c>.</returns>
    public static async Task<PcfInstanceRequest?> ShowAsync(string projectFile, IPcfSetupService service, Action<string>? log, CancellationToken token)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var dialog = new PcfSetupDialog { DataContext = new PcfSetupDialogViewModel(projectFile, service, new PcfSetupViewModel(service, log)), token = token };
        dialog.ShowModal();
        return dialog.request;
    }

    private PcfSetupDialogViewModel? Model => DataContext as PcfSetupDialogViewModel;

#pragma warning disable VSTHRD100 // WPF event handlers must be void; failures are shown by the view model.
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (loaded || Model is not { } model) return;
        loaded = true;
        SetupView.Token = token;
        SetupView.Report = message => model.ReportFailure(new InvalidOperationException(message));
        SetupView.Refresh = () => RunAsync(model.LoadAsync);
        await SetupView.Refresh();
    }

    private async void OnSetUp(object sender, RoutedEventArgs e)
    {
        var close = false;
        if (Model is { } model) await RunAsync(async cancellation => close = await model.RunMainActionAsync(cancellation));
        if (close) DialogResult = true;
    }
    private async void OnUpdateLockFile(object sender, RoutedEventArgs e) => await ApplyAsync(true);
#pragma warning restore VSTHRD100

    private Task ApplyAsync(bool updateLockFile)
        => Model is { } model ? RunAsync(cancellation => model.ApplyAsync(updateLockFile, cancellation)) : Task.CompletedTask;

    private void OnRestart(object sender, RoutedEventArgs e) => RequestInstance(closeThisWindow: true);
    private void OnSecondWindow(object sender, RoutedEventArgs e) => RequestInstance(closeThisWindow: false);

    // The dialog only records the choice. Closing a solution from inside a modal dialog is not safe, so the caller
    // starts the new window once this one is gone.
    private void RequestInstance(bool closeThisWindow)
    {
        try
        {
            request = Model?.RequestInstance(closeThisWindow);
            if (request is not null) DialogResult = true;
        }
        catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException) { Model?.ReportFailure(ex); }
    }

    private async Task RunAsync(Func<CancellationToken, Task> action)
    {
        if (Model is not { IsIdle: true } model) return;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        operation = cancellation;
        try { await action(cancellation.Token); }
        catch (Exception ex) { model.ReportFailure(ex); }
        finally { operation = null; }
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        if (Model is { IsIdle: false })
        {
            e.Handled = true;
            CancelOperation();
        }
        else DialogResult = false;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (Model is { IsIdle: false })
        {
            e.Cancel = true;
            CancelOperation();
        }
        base.OnClosing(e);
    }

    private void CancelOperation()
    {
        if (operation is null || operation.IsCancellationRequested) return;
        Model?.ReportCanceling();
        operation.Cancel();
    }

    private void OnOutputChanged(object sender, TextChangedEventArgs e) => ((System.Windows.Controls.TextBox)sender).ScrollToEnd();
}
#nullable restore
