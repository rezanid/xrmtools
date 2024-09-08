namespace XrmToolbox.UI.TextBoxControl;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

public class TextBox : System.Windows.Controls.TextBox
{
    public static readonly DependencyProperty PlaceholderProperty = 
        DependencyProperty.Register(
            "Placeholder",
            typeof(string),
            typeof(TextBox),
            new PropertyMetadata(default(string)));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly(
            "IsEmpty",
            typeof(bool),
            typeof(TextBox),
            new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    public bool IsEmpty
    {
        get => (bool)GetValue(IsEmptyProperty);
        private set => SetValue(IsEmptyPropertyKey, value);
    }

    static TextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBox), new FrameworkPropertyMetadata(typeof(TextBox)));
    }

    protected override void OnTextChanged(System.Windows.Controls.TextChangedEventArgs e)
    {
        IsEmpty = string.IsNullOrEmpty(Text);
        base.OnTextChanged(e);
    }
}
