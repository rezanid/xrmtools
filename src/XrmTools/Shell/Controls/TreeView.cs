namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Input;

public class TreeView : System.Windows.Controls.TreeView
{
    static TreeView() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeView), new FrameworkPropertyMetadata(typeof(TreeView)));

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TreeViewItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TreeViewItem;

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        if (e.NewFocus is TreeViewItem)
            return;
        object obj = SelectedItem ?? (Items.Count > 0 ? Items[0] : null);
        if (obj == null)
            return;
        if (obj is TreeViewItem treeViewItem1)
        {
            treeViewItem1.Focus();
            e.Handled = true;
        }
        if (!(ItemContainerGenerator.ContainerFromItem(obj) is TreeViewItem treeViewItem2))
            return;
        treeViewItem2.Focus();
        e.Handled = true;
    }
}