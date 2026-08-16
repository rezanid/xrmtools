#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.Windows;
using System.Windows.Controls;
using XrmTools.DataverseExplorer.Models;
using XrmTools.DataverseExplorer.ViewModels;

/// <summary>
/// Interaction logic for DataverseExplorerWindowControl.xaml
/// </summary>
public partial class DataverseExplorerWindowControl : UserControl
{
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
