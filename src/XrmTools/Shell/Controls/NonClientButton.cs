namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Input;
using XrmTools.Shell.Helpers;

public class NonClientButton : PathButton
{
    static NonClientButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NonClientButton), new FrameworkPropertyMetadata(typeof(NonClientButton)));

    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);
        var window = System.Windows.Window.GetWindow(this);
        if (window != null)
        {
            WindowInterop.ShowSystemMenu(window, PointToScreen(e.GetPosition(this)));
            e.Handled = true;
        }
    }
}
