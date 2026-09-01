namespace XrmTools.Shell.Controls;

using System.Windows;

internal class DataGridRow : System.Windows.Controls.DataGridRow
{
    static DataGridRow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(typeof(DataGridRow)));
    }
}
