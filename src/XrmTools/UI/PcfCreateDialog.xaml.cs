#nullable enable
namespace XrmTools.UI;

using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using XrmTools.DataverseSolutions;
using XrmTools.Shell.Controls;

/// <param name="ProjectFile">The created project, to be added to the solution.</param>
/// <param name="SetupIncomplete">Dependencies were requested but are not installed yet.</param>
internal sealed record PcfCreateResult(string ProjectFile, bool SetupIncomplete);

internal partial class PcfCreateDialog : DialogWindow
{
    private Func<PcfCreateRequest, CancellationToken, Task<string>> create = null!;
    private CancellationToken token;
    private CancellationTokenSource? operation;
    private PcfCreateResult? result;
    private bool loaded;
    private PcfCreateDialog() => InitializeComponent();
    public static async Task<PcfCreateResult?> ShowAsync(string location, PcfSetupViewModel? setup, Func<PcfCreateRequest, CancellationToken, Task<string>> create, CancellationToken token)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var dialog = new PcfCreateDialog { DataContext = new PcfCreateDialogViewModel(location, setup), create = create, token = token };
        return dialog.ShowModal() == true ? dialog.result : null;
    }
    private PcfCreateDialogViewModel? Model => DataContext as PcfCreateDialogViewModel;
    private void OnBrowse(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PcfCreateDialogViewModel model) return;
        try
        {
            using var browser = new System.Windows.Forms.FolderBrowserDialog { Description = "Select the parent folder for the PCF project", SelectedPath = model.Location, ShowNewFolderButton = true };
            if (browser.ShowDialog(new OwnerWindow(new WindowInteropHelper(this).Handle)) == System.Windows.Forms.DialogResult.OK) model.Location = browser.SelectedPath;
        }
        catch (Exception ex)
        {
            model.ReportFailure(ex);
        }
    }
#pragma warning disable VSTHRD100 // WPF event handlers must be void; all exceptions are handled below.
    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (loaded || Model is not { Setup: not null } model) return;
        loaded = true;
        SetupView.Token = token;
        SetupView.Report = message => model.ReportFailure(new InvalidOperationException(message));
        // Node.js is checked while the developer fills in the project details; the fields stay usable meanwhile.
        SetupView.Refresh = () => model.Setup.RefreshAsync(model.CreatedProject, model.TargetDirectory, token);
        try { await SetupView.Refresh(); }
        catch (Exception ex) { model.ReportFailure(ex); }
    }
    private async void OnCreate(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (DataContext is not PcfCreateDialogViewModel { IsIdle: true } model) return;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        operation = cancellation;
        try
        {
            var project = await model.CreateAsync(create, cancellation.Token);
            if (project is null) return;
            result = new PcfCreateResult(project, false);
            DialogResult = true;
        }
        catch (Exception ex)
        {
            model.ReportFailure(ex);
        }
        finally { operation = null; }
    }
    private void OnOpen(object sender, RoutedEventArgs e)
    {
        if (Model is not { IsIdle: true, CreatedProject: { } project } model) return;
        result = new PcfCreateResult(project, model.SetupIncomplete);
        DialogResult = true;
    }
    private void OnCancel(object sender, RoutedEventArgs e)
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
    private sealed class OwnerWindow(IntPtr handle) : System.Windows.Forms.IWin32Window { public IntPtr Handle { get; } = handle; }
}
#nullable restore