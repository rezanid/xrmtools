#nullable enable
namespace XrmTools.DataverseExplorer.Views;

using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public sealed class SuppressTreeViewAutoBringIntoViewBehavior : Behavior<TreeView>
{
    private static readonly DependencyProperty IsInternalBringIntoViewProperty =
    DependencyProperty.RegisterAttached(
        "IsInternalBringIntoView",
        typeof(bool),
        typeof(SuppressTreeViewAutoBringIntoViewBehavior),
        new PropertyMetadata(false));

    private static bool GetIsInternalBringIntoView(DependencyObject obj) =>
        (bool)obj.GetValue(IsInternalBringIntoViewProperty);

    private static void SetIsInternalBringIntoView(DependencyObject obj, bool value) =>
        obj.SetValue(IsInternalBringIntoViewProperty, value);

    // If true, keep vertical ensure-visible (no horizontal jump).
    // If false, suppress all bring-into-view scrolling.
    public bool VerticalOnly
    {
        get => (bool)GetValue(VerticalOnlyProperty);
        set => SetValue(VerticalOnlyProperty, value);
    }

    public static readonly DependencyProperty VerticalOnlyProperty =
        DependencyProperty.Register(nameof(VerticalOnly), typeof(bool),
            typeof(SuppressTreeViewAutoBringIntoViewBehavior),
            new PropertyMetadata(true));

    // Track which TreeViews have this behavior enabled.
    private static readonly HashSet<TreeView> EnabledTrees = [];

    private static bool ClassHandlerRegistered;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (!ClassHandlerRegistered)
        {
            ClassHandlerRegistered = true;
            EventManager.RegisterClassHandler(
                typeof(TreeViewItem),
                FrameworkElement.RequestBringIntoViewEvent,
                new RequestBringIntoViewEventHandler(OnTreeViewItemRequestBringIntoView),
                handledEventsToo: true);
        }

        EnabledTrees.Add(AssociatedObject);
    }

    protected override void OnDetaching()
    {
        EnabledTrees.Remove(AssociatedObject);
        base.OnDetaching();
    }

    private static void OnTreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
        if (sender is not TreeViewItem current) return;

        // Ignore recursion from our own BringIntoView call
        if (GetIsInternalBringIntoView(current))
            return;

        // Only handle the originating item (prevents ancestor effects)
        var origin = FindAncestor<TreeViewItem>(e.OriginalSource as DependencyObject)
                     ?? (e.OriginalSource as TreeViewItem);

        if (origin is null || !ReferenceEquals(current, origin))
            return;

        var tree = FindAncestor<TreeView>(current);
        if (tree is null) return;
        if (!EnabledTrees.Contains(tree)) return;

        // Stop default behavior (prevents horizontal jump)
        e.Handled = true;

        var behavior = FindBehavior(tree);
        if (behavior?.VerticalOnly != true || !current.IsSelected)
            return;

        // Do vertical ensure-visible safely (async + guarded)
        current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
        {
            var sv = FindDescendant<ScrollViewer>(tree);
            if (sv is null) return;

            if (sv is { ViewportWidth: > 0 } scrollViewer)
            {
                try
                {
                    SetIsInternalBringIntoView(current, true);

                    var h = current.ActualHeight > 0 ? current.ActualHeight : 1;
                    current.BringIntoView(new Rect(0, 0, scrollViewer.ViewportWidth, h));
                }
                finally
                {
                    SetIsInternalBringIntoView(current, false);
                }
            }
            else
            {
                // fallback (rare)
                try
                {
                    SetIsInternalBringIntoView(current, true);
                    current.BringIntoView();
                }
                finally
                {
                    SetIsInternalBringIntoView(current, false);
                }
            }
        }));
    }

    // Find the behavior instance for this TreeView (so we can read VerticalOnly)
    private static SuppressTreeViewAutoBringIntoViewBehavior? FindBehavior(TreeView tree)
    {
        // Behaviors are stored in Interaction.GetBehaviors(tree)
        var behaviors = Interaction.GetBehaviors(tree);
        for (int i = 0; i < behaviors.Count; i++)
        {
            if (behaviors[i] is SuppressTreeViewAutoBringIntoViewBehavior b)
                return b;
        }
        return null;
    }

    private static void BringIntoViewVertically(TreeView tree, TreeViewItem item)
    {
        var sv = FindDescendant<ScrollViewer>(tree);
        if (sv is null || sv.ViewportHeight <= 0 || item.ActualHeight <= 0) return;

        Rect bounds = item.TransformToAncestor(sv)
                          .TransformBounds(new Rect(new Point(0, 0), item.RenderSize));

        double newV = sv.VerticalOffset;

        if (bounds.Top < 0)
            newV += bounds.Top;
        else if (bounds.Bottom > sv.ViewportHeight)
            newV += (bounds.Bottom - sv.ViewportHeight);
        else
            return;

        newV = Math.Max(0, Math.Min(newV, Math.Max(0, sv.ExtentHeight - sv.ViewportHeight)));
        sv.ScrollToVerticalOffset(newV);
    }

    private static T? FindAncestor<T>(DependencyObject? start) where T : DependencyObject
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