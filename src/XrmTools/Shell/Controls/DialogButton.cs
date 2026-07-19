namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using System.Windows;

public class DialogButton : Button
{
    public static readonly double ControlMinWidth = 120.0;
    public static readonly DependencyProperty MessageButtonsProperty = Property.Register<DialogButton, MessageBoxButton>(nameof(MessageButtons));
    public static readonly DependencyProperty ResultKindProperty = Property.Register<DialogButton, MessageBoxResult>(nameof(ResultKind));

    static DialogButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DialogButton), new FrameworkPropertyMetadata(typeof(DialogButton)));
    }

    public MessageBoxButton MessageButtons
    {
        get => (MessageBoxButton)GetValue(MessageButtonsProperty);
        set => SetValue(MessageButtonsProperty, Boxes.Box(value));
    }

    public MessageBoxResult ResultKind
    {
        get => (MessageBoxResult)GetValue(ResultKindProperty);
        set => SetValue(ResultKindProperty, Boxes.Box(value));
    }
}
