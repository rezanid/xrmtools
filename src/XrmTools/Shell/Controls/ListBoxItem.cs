namespace XrmTools.Shell.Controls;

using System.Windows;

public class ListBoxItem : System.Windows.Controls.ListBoxItem
{
    static ListBoxItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ListBoxItem), new FrameworkPropertyMetadata(typeof(ListBoxItem)));
}
