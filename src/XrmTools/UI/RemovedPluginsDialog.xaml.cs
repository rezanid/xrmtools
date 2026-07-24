#nullable enable
namespace XrmTools.UI;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.Windows;

/// <summary>
/// Interaction logic for RemovedPluginsDialog.xaml.
/// Presents plugins that exist in Dataverse but no longer exist in the compiled assembly (renamed or
/// removed in code) and lets the user decide whether to delete them (authoritative sync) or cancel.
/// </summary>
public partial class RemovedPluginsDialog : DialogWindow
{
    internal RemovedPluginsDecision Decision { get; private set; } = RemovedPluginsDecision.Cancel;

    internal RemovedPluginsDialog(IReadOnlyList<RemovedPluginSummary> removedPlugins)
    {
        InitializeComponent();
        DataContext = new RemovedPluginsViewModel(removedPlugins);
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        Decision = RemovedPluginsDecision.Delete;
        DialogResult = true;
        Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Decision = RemovedPluginsDecision.Cancel;
        DialogResult = false;
        Close();
    }
}
#nullable restore
