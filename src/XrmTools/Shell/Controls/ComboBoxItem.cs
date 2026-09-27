namespace XrmTools.Shell.Controls;
using System.Windows;

public class ComboBoxItem : System.Windows.Controls.ComboBoxItem
{
    static ComboBoxItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxItem), new FrameworkPropertyMetadata(typeof(ComboBoxItem)));
}
