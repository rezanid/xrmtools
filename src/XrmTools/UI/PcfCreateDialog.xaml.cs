#nullable enable
namespace XrmTools.UI;

using Microsoft.VisualStudio.Shell;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using XrmTools.DataverseSolutions;
using XrmTools.Shell.Controls;

internal partial class PcfCreateDialog : DialogWindow
{
    private Func<PcfCreateRequest, CancellationToken, Task<string>> create = null!;
    private CancellationToken token;
    private string? result;
    private PcfCreateDialog() => InitializeComponent();
    public static async Task<string?> ShowAsync(string location, Func<PcfCreateRequest, CancellationToken, Task<string>> create, CancellationToken token)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        var dialog = new PcfCreateDialog { DataContext = new PcfCreateDialogViewModel(location), create = create, token = token };
        return dialog.ShowModal() == true ? dialog.result : null;
    }
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
    private async void OnCreate(object sender, RoutedEventArgs e)
#pragma warning restore VSTHRD100
    {
        if (DataContext is not PcfCreateDialogViewModel model) return;
        try
        {
            result = await model.CreateAsync(create, token);
            if (result is not null) DialogResult = true;
        }
        catch (Exception ex)
        {
            model.ReportFailure(ex);
        }
    }
    private sealed class OwnerWindow(IntPtr handle) : System.Windows.Forms.IWin32Window { public IntPtr Handle { get; } = handle; }
}
#nullable restore
