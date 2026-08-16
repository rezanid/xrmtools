#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System.Windows;
using System.Windows.Controls;

public static class TreeViewITemBringIntoViewBehavior
{
    public static readonly DependencyProperty SuppressBringIntoViewProperty =
        DependencyProperty.RegisterAttached(
            "SuppressBringIntoView",
            typeof(bool),
            typeof(TreeViewITemBringIntoViewBehavior),
            new PropertyMetadata(false));

    public static void SetSuppressBringIntoView(DependencyObject element, bool value) =>
        element.SetValue(SuppressBringIntoViewProperty, value);

    public static bool GetSuppressBringIntoView(DependencyObject element) =>
        (bool)element.GetValue(SuppressBringIntoViewProperty);

    // This is the handler you already validated works.
    public static void OnTreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        if (sender is not TreeViewItem item) return;

        if (GetSuppressBringIntoView(item))
            e.Handled = true;
    }
}
#nullable restore