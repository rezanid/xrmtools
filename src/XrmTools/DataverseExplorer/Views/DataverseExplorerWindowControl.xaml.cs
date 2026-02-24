#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.Windows;
using System.Windows.Controls;
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
        if (sender is TreeViewItem { DataContext: var node } &&
            DataContext is DataverseExplorerViewModel viewModel)
        {
            viewModel.NodeExpandedCommand.Execute(node);
        }
    }

    private void TreeView_RequestBringIntoView(object sender, System.Windows.RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }

    private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }

}

#nullable restore
