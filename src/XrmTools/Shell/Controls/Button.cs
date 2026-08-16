namespace XrmTools.Shell.Controls;

using System.Windows;
using XrmTools.Shell.Styles;
using XrmTools.Shell.Helpers;

public class Button : System.Windows.Controls.Button
{
    public static readonly DependencyProperty CornerRadiusProperty = Property.Register<Button, CornerRadius>(nameof(CornerRadius));
    public static readonly DependencyProperty KindProperty = Property.Register<Button, ButtonKind>(nameof(Kind));

    static Button()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, Boxes.Box(value));
    }

    public ButtonKind Kind
    {
        get => (ButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, Boxes.Box(value));
    }
}