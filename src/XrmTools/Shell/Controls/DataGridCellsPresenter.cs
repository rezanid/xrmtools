namespace XrmTools.Shell.Controls;

using System.Windows;

internal class DataGridCellsPresenter : System.Windows.Controls.Primitives.DataGridCellsPresenter
{
    static DataGridCellsPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(typeof(DataGridCellsPresenter)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new DataGridCell();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is DataGridCell;
}
