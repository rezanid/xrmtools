#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
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
    internal IEnumerable<IExplorerCollectionOperation> CollectionOperations { get; set; } = [];
    private FrameworkElement? _returnFocus;

    private void TreeViewItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (sender is not XrmTools.Shell.Controls.TreeViewItem item || item.DataContext is not ExplorerNodeBase node) return;
        e.Handled = true;
        item.IsSelected = true;
        _returnFocus = item;
        var entries = CommandProviders.SelectMany(provider => provider.GetCommands(node)).ToList();
        if (DataContext is DataverseExplorerViewModel viewModel)
            foreach (var operation in CollectionOperations.Where(operation => operation.SupportsScope(node)))
                entries.Add(new ExplorerMenuItem { Header = operation.Title + "…",
                    Command = new AsyncRelayCommand(() => viewModel.BeginCollectionAsync(operation, node), () => viewModel.Collection == null && !node.SessionToken.IsCancellationRequested) });
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
        PreviewKeyDown += (_, e) =>
        {
            if (DataContext is DataverseExplorerViewModel { Collection: null } && e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            { BrowseSearch.Focus(); BrowseSearch.SelectAll(); e.Handled = true; }
            else if (e.Key == Key.Escape && BrowseSearch.IsKeyboardFocusWithin && DataContext is DataverseExplorerViewModel model)
            { model.SearchText = string.Empty; e.Handled = true; }
        };
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is DataverseExplorerViewModel oldModel) oldModel.PropertyChanged -= WorkspaceChanged;
            if (e.NewValue is DataverseExplorerViewModel newModel) newModel.PropertyChanged += WorkspaceChanged;
        };
    }

    private void WorkspaceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(DataverseExplorerViewModel.Collection) || sender is not DataverseExplorerViewModel { Collection: null }) return;
        // Restore keyboard focus on cancellation, but do not steal it from a newly opened document.
        if (Keyboard.FocusedElement is DependencyObject focused && IsAncestorOf(focused))
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() => _returnFocus?.Focus()));
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
