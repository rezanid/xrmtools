namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Input;
using XrmTools.Shell.Helpers;

public class ContextMenu : System.Windows.Controls.ContextMenu
{
    private static readonly DependencyPropertyKey IsClosedPropertyKey = Property.RegisterReadOnly<ContextMenu, bool>(nameof(IsClosed), true);
    public static readonly DependencyProperty IsClosedProperty = IsClosedPropertyKey.DependencyProperty;
    
    internal static event RoutedEventHandler? ContextMenuOpened;

    internal static event RoutedEventHandler? ContextMenuClosed;

    static ContextMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ContextMenu), new FrameworkPropertyMetadata(typeof(ContextMenu)));
    }

    public bool IsClosed
    {
        get => (bool)this.GetValue(IsClosedProperty);
        private set => this.SetValue(IsClosedPropertyKey, Boxes.Box<bool>(value));
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return (DependencyObject)new MenuItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MenuItem || item is Separator;
    }

    protected override void OnClosed(RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnClosed");
        base.OnClosed(e);
        this.IsClosed = true;
        RoutedEventHandler contextMenuClosed = ContextMenuClosed;
        if (contextMenuClosed == null)
            return;
        contextMenuClosed((object)this, e);
    }

    protected override void OnOpened(RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnOpened");
        base.OnOpened(e);
        this.IsClosed = false;
        RoutedEventHandler contextMenuOpened = ContextMenuOpened;
        if (contextMenuOpened == null)
            return;
        contextMenuOpened((object)this, e);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnPreviewMouseLeftButtonDown");
        this.HandleAutoClose(e.OriginalSource);
        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("OnPreviewMouseRightButtonDown");
        this.HandleAutoClose(e.OriginalSource);
        base.OnPreviewMouseRightButtonDown(e);
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        object obj1 = (object)null;
        object obj2 = (object)null;
        Separator separator = element as Separator;
        if (separator != null)
        {
            obj1 = separator.GetValue(FrameworkElement.DefaultStyleKeyProperty);
            obj2 = separator.GetValue(FrameworkElement.StyleProperty);
        }
        base.PrepareContainerForItemOverride(element, item);
        if (separator == null)
            return;
        separator.SetValue(FrameworkElement.DefaultStyleKeyProperty, obj1);
        separator.SetValue(FrameworkElement.StyleProperty, obj2);
    }

    private void HandleAutoClose(object originalSource)
    {
        if (!(originalSource is PassThroughBorder))
            return;
        this.IsOpen = false;
    }
}
