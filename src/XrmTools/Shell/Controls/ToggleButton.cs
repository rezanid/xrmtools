namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;
using System.Windows;

public class ToggleButton : System.Windows.Controls.Primitives.ToggleButton
{
    public static readonly DependencyProperty CornerRadiusProperty = Property.Register<ToggleButton, CornerRadius>(nameof(CornerRadius));
    public static readonly DependencyProperty KindProperty = Property.Register<ToggleButton, ButtonKind>(nameof(Kind));

    static ToggleButton() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButton), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(ToggleButton)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, Boxes.Box<CornerRadius>(value));
    }

    public ButtonKind Kind
    {
        get => (ButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, Boxes.Box<ButtonKind>(value));
    }
}
