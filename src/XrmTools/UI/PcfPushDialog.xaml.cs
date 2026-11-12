#nullable enable
namespace XrmTools.UI;

using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XrmTools.DataverseSolutions;
using XrmTools.Shell.Controls;
using XrmTools.WebApi.Entities;

internal partial class PcfPushDialog : DialogWindow
{
    private Func<PcfPushRequest, CancellationToken, IProgress<string>?, Task> push = null!;
    private CancellationToken token;
    private CancellationTokenSource? operation;
    private Func<CancellationToken, Task<IEnumerable<Solution>>> loadSolutions = null!;
    private Func<CancellationToken, Task<IReadOnlyList<PcfSolutionDestination>>>? findReferences;
    private bool initialized;
    private PcfPushDialog() => InitializeComponent();
    public static async Task ShowAsync(string projectPath, string environmentUrl, Func<PcfPushRequest, CancellationToken, IProgress<string>?, Task> push,
        Func<CancellationToken, Task<IEnumerable<Solution>>> loadSolutions,
        Func<CancellationToken, Task<IReadOnlyList<PcfSolutionDestination>>> findReferences, CancellationToken token)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var dialog = new PcfPushDialog { DataContext = new PcfPushDialogViewModel(projectPath, environmentUrl), push = push, loadSolutions = loadSolutions, findReferences = findReferences, token = token };
        dialog.ShowModal();
    }
    #pragma warning disable VSTHRD100 // WPF event handlers must be void; all exceptions are handled below.
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (initialized || findReferences is null || DataContext is not PcfPushDialogViewModel model) return;
        initialized = true;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        operation = cancellation;
        try { await model.InitializeAsync(findReferences, loadSolutions, cancellation.Token); }
        catch (Exception ex) { model.ReportFailure(ex); }
        finally { operation = null; }
    }
    #pragma warning restore VSTHRD100

    private void OnChangeDestination(object sender, RoutedEventArgs e)
    {
        if (DataContext is PcfPushDialogViewModel { IsIdle: true } model) model.ChangeDestination();
    }

    #pragma warning disable VSTHRD100 // WPF event handlers must be void; all exceptions are handled below.
    private async void OnPush(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (DataContext is not PcfPushDialogViewModel model || !model.IsIdle) return;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        operation = cancellation;
        try
        {
            var result = await model.PushAsync(push, cancellation.Token);
            if (result is not null) DialogResult = true;
        }
        catch (Exception ex)
        {
            model.ReportFailure(ex);
        }
        finally { operation = null; }
    }

    #pragma warning disable VSTHRD100 // WPF event handlers; loading failures are shown by the view model.
    private async void OnSolutionSelected(object sender, RoutedEventArgs e) => await LoadSolutionsAsync(false);
    private async void OnRefreshSolutions(object sender, RoutedEventArgs e) => await LoadSolutionsAsync(true);
    #pragma warning restore VSTHRD100

    private async Task LoadSolutionsAsync(bool refresh)
    {
        if (DataContext is not PcfPushDialogViewModel model || !model.IsIdle) return;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        operation = cancellation;
        try { await model.LoadSolutionsAsync(loadSolutions, cancellation.Token, refresh); }
        catch (Exception ex) { model.ReportFailure(ex); }
        finally { operation = null; }
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        if (DataContext is PcfPushDialogViewModel { IsIdle: false })
        {
            e.Handled = true;
            CancelOperation();
        }
        else DialogResult = false;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is PcfPushDialogViewModel { IsIdle: false })
        {
            e.Cancel = true;
            CancelOperation();
        }
        base.OnClosing(e);
    }

    private void CancelOperation()
    {
        if (operation is null || operation.IsCancellationRequested) return;
        ((PcfPushDialogViewModel)DataContext).ReportCanceling();
        operation.Cancel();
    }
}
#nullable restore
