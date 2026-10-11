#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XrmTools.DataverseExplorer.ViewModels;

public partial class ExplorerCollectionControl : UserControl
{
    public ExplorerCollectionControl() => InitializeComponent();
    private void Workspace_Loaded(object sender, RoutedEventArgs e) => SearchBox.Focus();
    private void Selection_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ExplorerCollectionRow row }) row.IsChecked = row.IsChecked != true;
    }
    private void Workspace_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not ExplorerCollectionSession session) return;
        if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus(); SearchBox.SelectAll(); e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            session.CancelCommand.Execute(null); e.Handled = true;
        }
        else if (e.Key == Key.Space && e.OriginalSource is XrmTools.Shell.Controls.TreeViewItem { DataContext: ExplorerCollectionRow row } && !session.IsBusy)
        {
            row.IsChecked = row.IsChecked != true; e.Handled = true;
        }
    }
}
