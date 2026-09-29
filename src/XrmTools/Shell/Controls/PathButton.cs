namespace XrmTools.Shell.Controls;

using System.Windows;
using System.Windows.Media;
using XrmTools.Shell.Helpers;

public class PathButton : Button
{
    public static readonly DependencyProperty DataProperty = Property.RegisterFull<PathButton, Geometry>(nameof(Data));
    static PathButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(PathButton), new FrameworkPropertyMetadata(typeof(PathButton)));
    public Geometry Data { get => (Geometry)GetValue(DataProperty); set => SetValue(DataProperty, value); }
}
