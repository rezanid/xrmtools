namespace XrmTools.Shell.Controls;

using System.Windows;
using XrmTools.Shell.Helpers;

public class TabControl : System.Windows.Controls.TabControl
{
    public static readonly DependencyProperty ShowSelectedContentProperty =
        DependencyProperty.Register(nameof(ShowSelectedContent), typeof(bool), typeof(TabControl),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    static TabControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControl), new FrameworkPropertyMetadata(typeof(TabControl)));

    public bool ShowSelectedContent
    {
        get => (bool)GetValue(ShowSelectedContentProperty);
        set => SetValue(ShowSelectedContentProperty, Boxes.Box(value));
    }

    protected override DependencyObject GetContainerForItemOverride() => new TabItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is TabItem;
}
