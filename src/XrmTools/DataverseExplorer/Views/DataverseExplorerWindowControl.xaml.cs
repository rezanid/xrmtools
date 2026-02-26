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
        if (e.OriginalSource is XrmTools.Shell.Controls.TreeViewItem { DataContext: var node } &&
            DataContext is DataverseExplorerViewModel viewModel)
        {
            viewModel.NodeExpandedCommand.Execute(node);
        }
    }
}

#nullable restore
