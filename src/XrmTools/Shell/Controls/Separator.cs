namespace XrmTools.Shell.Controls;

using System.Windows;

public class Separator : System.Windows.Controls.Separator
{
    public static readonly double StrokeWidth = 1.0;

    static Separator()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Separator), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(Separator)));
    }
}