namespace XrmTools.Shell.Controls;

using System.Windows;
using XrmTools.Shell.Helpers;

public class MenuScrollViewer : System.Windows.Controls.ScrollViewer
{
    static MenuScrollViewer()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuScrollViewer), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(MenuScrollViewer)));
    }

    protected override Size MeasureOverride(Size constraint)
    {
        VisualTree.InvalidateMeasureToAncestor(this.Content as FrameworkElement, (FrameworkElement)this);
        return base.MeasureOverride(constraint);
    }
}
