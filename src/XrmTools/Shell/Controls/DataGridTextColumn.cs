namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Controls;
using XrmTools.Shell.Helpers;

public class DataGridTextColumn : System.Windows.Controls.DataGridTextColumn
{
    protected override FrameworkElement GenerateEditingElement(System.Windows.Controls.DataGridCell cell, object dataItem)
        => DataGridColumns.CreateTextBox(this);

    protected override FrameworkElement GenerateElement(System.Windows.Controls.DataGridCell cell, object dataItem)
        => DataGridColumns.CreateTextBlock(this);

    internal static DataGridTextColumn CreateFrom(System.Windows.Controls.DataGridTextColumn column)
    {
        return new DataGridTextColumn
        {
            Binding = column.Binding,
            CanUserSort = column.CanUserSort,
            ClipboardContentBinding = column.ClipboardContentBinding,
            Header = column.Header,
            IsReadOnly = column.IsReadOnly,
            MinWidth = column.CalculateMinWidth(),
            SortMemberPath = column.SortMemberPath,
            Width = column.Width,
        };
    }
}
