#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

public static class TreeViewScrollBehavior
{
    public static readonly DependencyProperty VerticalBringIntoViewOnlyProperty =
        DependencyProperty.RegisterAttached(
            "VerticalBringIntoViewOnly",
            typeof(bool),
            typeof(TreeViewScrollBehavior),
            new PropertyMetadata(false, OnChanged));

    public static void SetVerticalBringIntoViewOnly(DependencyObject element, bool value) =>
        element.SetValue(VerticalBringIntoViewOnlyProperty, value);

    public static bool GetVerticalBringIntoViewOnly(DependencyObject element) =>
        (bool)element.GetValue(VerticalBringIntoViewOnlyProperty);

    private static bool _classHandlerRegistered;

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Register once globally; we’ll only act when the attached property is enabled on the owning TreeView.
        if (!_classHandlerRegistered)
        {
            _classHandlerRegistered = true;
            EventManager.RegisterClassHandler(
                typeof(TreeViewItem),
                FrameworkElement.RequestBringIntoViewEvent,
                new RequestBringIntoViewEventHandler(OnTreeViewItemRequestBringIntoView),
                handledEventsToo: true);
        }
    }

    private static void OnTreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        if (sender is not TreeViewItem item)
            return;
        if (e.TargetObject is not TreeViewItem)
            return;

        // Find the owning TreeView
        var tree = FindAncestor<TreeView>(item);
        if (tree is null)
            return;

        // Only affect TreeViews that opted in
        if (!GetVerticalBringIntoViewOnly(tree))
            return;

        // Stop the default scrolling (this prevents the internal ScrollViewer horizontal jump)
        e.Handled = true;

        var sv = FindDescendant<ScrollViewer>(tree);
        if (sv is null || sv.ViewportHeight <= 0 || item.ActualHeight <= 0)
            return;

        // Compute item bounds relative to the ScrollViewer viewport
        Rect bounds = item.TransformToAncestor(sv)
                          .TransformBounds(new Rect(new Point(0, 0), item.RenderSize));

        double newV = sv.VerticalOffset;

        if (bounds.Top < 0)
            newV += bounds.Top;
        else if (bounds.Bottom > sv.ViewportHeight)
            newV += (bounds.Bottom - sv.ViewportHeight);
        else
            return; // already visible vertically

        // Clamp
        if (newV < 0) newV = 0;
        double max = Math.Max(0, sv.ExtentHeight - sv.ViewportHeight);
        if (newV > max) newV = max;

        sv.ScrollToVerticalOffset(newV);
        // Intentionally do NOT touch HorizontalOffset.
    }

    private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject
    {
        DependencyObject? current = start;
        while (current is not null)
        {
            if (current is T typed) return typed;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0, n = VisualTreeHelper.GetChildrenCount(root); i < n; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed) return typed;

            var found = FindDescendant<T>(child);
            if (found is not null) return found;
        }
        return null;
    }
}
#nullable restore