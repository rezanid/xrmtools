namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class DataGrid : System.Windows.Controls.DataGrid
{
    static DataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGrid), new FrameworkPropertyMetadata(typeof(DataGrid)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new DataGridRow();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is DataGridRow;

    protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
    {
        if (e.Column is System.Windows.Controls.DataGridTextColumn textColumn)
        {
            e.Column = DataGridTextColumn.CreateFrom(textColumn);
        }

        base.OnAutoGeneratingColumn(e);
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        if (e.OriginalSource is DataGridColumnHeader header)
        {
            ScrollIntoView(null, header.Column);
        }
    }
}
