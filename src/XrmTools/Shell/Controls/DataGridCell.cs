namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Input;
using XrmTools.Shell.Helpers;

internal class DataGridCell : System.Windows.Controls.DataGridCell
{
    public static readonly DependencyProperty OwnerProperty = Property.Register<DataGridCell, DataGrid>(nameof(Owner));

    static DataGridCell()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(typeof(DataGridCell)));
    }

    public DataGrid Owner
    {
        get => (DataGrid)GetValue(OwnerProperty);
        set => SetValue(OwnerProperty, value);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space && Column is DataGridTextColumn && !IsReadOnly)
        {
            e.Handled = true;
            var editEvent = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.F2);
            Owner?.BeginEdit(editEvent);
        }

        if (!e.Handled)
        {
            base.OnKeyDown(e);
        }
    }
}
