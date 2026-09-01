namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Data;

internal class DataGridColumnHeadersPresenter : System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter
{
    static DataGridColumnHeadersPresenter()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnHeadersPresenter), new FrameworkPropertyMetadata(typeof(DataGridColumnHeadersPresenter)));
    }

    protected override DependencyObject GetContainerForItemOverride() => new DataGridColumnHeader();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is DataGridColumnHeader;

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is not DataGridColumnHeader header) return;

        header.SetBinding(DataGridColumnHeader.CanUserReorderProperty, new Binding(nameof(System.Windows.Controls.DataGridColumn.CanUserReorder))
        {
            Source = header.Column,
            FallbackValue = false,
        });
        header.SetBinding(DataGridColumnHeader.CanUserResizeProperty, new Binding(nameof(System.Windows.Controls.DataGridColumn.CanUserResize))
        {
            Source = header.Column,
            FallbackValue = false,
        });
    }
}
