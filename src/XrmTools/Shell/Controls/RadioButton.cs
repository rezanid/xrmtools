namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using System.Windows;
using System.Windows.Input;

public class RadioButton : System.Windows.Controls.RadioButton
{
    public static readonly DependencyProperty FocusOnlyOnAccessKeyProperty = Property.Register<RadioButton, bool>(nameof(FocusOnlyOnAccessKey));

    static RadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButton), new FrameworkPropertyMetadata(typeof(RadioButton)));
    }

    public bool FocusOnlyOnAccessKey
    {
        get => (bool)GetValue(FocusOnlyOnAccessKeyProperty);
        set => SetValue(FocusOnlyOnAccessKeyProperty, value);
    }

    protected override void OnAccessKey(AccessKeyEventArgs e)
    {
        if (FocusOnlyOnAccessKey)
            Focus();
        else
            base.OnAccessKey(e);
    }
}
