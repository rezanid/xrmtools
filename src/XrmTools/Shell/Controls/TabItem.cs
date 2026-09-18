namespace XrmTools.Shell.Controls;

using System.Windows;

public class TabItem : System.Windows.Controls.TabItem
{
    static TabItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TabItem), new FrameworkPropertyMetadata(typeof(TabItem)));
}
