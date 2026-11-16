#nullable enable
namespace XrmTools.UI;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

/// <summary>The Node.js selection and install option shown by both the new-project wizard and Set Up PCF Build.</summary>
internal partial class PcfSetupView : UserControl
{
    internal const string NodeDownloadPage = "https://nodejs.org/en/download";
    public static readonly DependencyProperty ShowNoticesProperty = DependencyProperty.Register(
        nameof(ShowNotices), typeof(bool), typeof(PcfSetupView), new PropertyMetadata(true));

    public PcfSetupView() => InitializeComponent();

    /// <summary>False when the host displays these notices outside its collapsible setup controls.</summary>
    public bool ShowNotices { get => (bool)GetValue(ShowNoticesProperty); set => SetValue(ShowNoticesProperty, value); }

    /// <summary>Re-reads the environment; supplied by the hosting dialog, which knows the project.</summary>
    public Func<Task>? Refresh { get; set; }
    public Action<string>? Report { get; set; }
    public CancellationToken Token { get; set; }

#pragma warning disable VSTHRD100 // WPF event handlers must be void; all exceptions are reported below.
    private async void OnBrowse(object sender, RoutedEventArgs e)
    {
        if (DataContext is not PcfSetupViewModel model) return;
        try
        {
            string? directory;
            using (var browser = new System.Windows.Forms.FolderBrowserDialog { Description = "Select the folder that contains node.exe", ShowNewFolderButton = false })
            {
                var owner = Window.GetWindow(this);
                if (browser.ShowDialog(owner is null ? null : new OwnerWindow(new WindowInteropHelper(owner).Handle)) != System.Windows.Forms.DialogResult.OK) return;
                directory = browser.SelectedPath;
            }
            if (!await model.SelectDirectoryAsync(directory, Token))
                Report?.Invoke($"'{directory}' does not contain a working node.exe. Select the folder of a Node.js installation.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { Report?.Invoke(ex.Message); }
        catch (OperationCanceledException) { }
    }

    private async void OnCheckAgain(object sender, RoutedEventArgs e)
    {
        try { if (Refresh is not null) await Refresh(); }
        catch (Exception ex) when (ex is not OperationCanceledException) { Report?.Invoke(ex.Message); }
        catch (OperationCanceledException) { }
    }

    private async void OnAutomatic(object sender, RoutedEventArgs e)
    {
        try
        {
            (DataContext as PcfSetupViewModel)?.ClearSelection();
            if (Refresh is not null) await Refresh();
        }
        catch (Exception ex) when (ex is not OperationCanceledException) { Report?.Invoke(ex.Message); }
        catch (OperationCanceledException) { }
    }
#pragma warning restore VSTHRD100

    private void OnGetNode(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo(NodeDownloadPage) { UseShellExecute = true })?.Dispose(); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Report?.Invoke($"The browser could not be opened. Go to {NodeDownloadPage} to get Node.js.");
        }
    }

    private sealed class OwnerWindow(IntPtr handle) : System.Windows.Forms.IWin32Window { public IntPtr Handle { get; } = handle; }
}
#nullable restore
