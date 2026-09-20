#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using XrmTools.DataverseExplorer.Services;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.ViewModels;
using ContextMenu = XrmTools.Shell.Controls.ContextMenu;
using MenuItem = XrmTools.Shell.Controls.MenuItem;

/// <summary>
/// Interaction logic for DataverseExplorerWindowControl.xaml
/// </summary>
public partial class DataverseExplorerWindowControl : UserControl
{
    internal IEnumerable<IExplorerCommandProvider> CommandProviders { get; set; } = [];

    private void TreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not XrmTools.Shell.Controls.TreeViewItem item || item.DataContext is not ExplorerNodeBase node) return;
        e.Handled = true;
        item.IsSelected = true;
        var entries = CommandProviders.SelectMany(provider => provider.GetCommands(node)).ToList();
        if (entries.Count == 0) return;
        var menu = new ContextMenu
        {
            PlacementTarget = item,
            Placement = e.CursorLeft < 0 ? PlacementMode.Bottom : PlacementMode.MousePoint,
        };
        foreach (var entry in entries) menu.Items.Add(CreateMenuItem(entry));
        menu.IsOpen = true;
    }

    private static MenuItem CreateMenuItem(ExplorerMenuItem entry)
    {
        var item = new MenuItem
        {
            Header = entry.Header, ToolTip = entry.ToolTip, IsEnabled = entry.IsEnabled,
            Command = entry.Command, CommandParameter = entry.CommandParameter,
        };
        ToolTipService.SetShowOnDisabled(item, true);
        foreach (var child in entry.Children) item.Items.Add(CreateMenuItem(child));
        return item;
    }

    public DataverseExplorerWindowControl()
    {
        InitializeComponent();
    }

    private void TreeViewItem_Expanded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (e.OriginalSource is XrmTools.Shell.Controls.TreeViewItem { DataContext: var node } &&
            DataContext is DataverseExplorerViewModel viewModel)
        {
            viewModel.NodeExpandedCommand.Execute(node);
        }
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is DataverseExplorerViewModel viewModel)
        {
            viewModel.SelectedNode = e.NewValue as ExplorerNodeBase;
        }
    }
}

#nullable restore
