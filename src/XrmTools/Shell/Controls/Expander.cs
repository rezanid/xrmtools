#nullable enable
namespace XrmTools.Shell.Controls;

using XrmTools.Shell.Helpers;
using System.Windows;
using System.Windows.Input;

[TemplatePart(Name = PART_HeaderButton, Type = typeof(ToggleButton))]
public class Expander : System.Windows.Controls.Expander
{
    private const string PART_HeaderButton = "HeaderSite";
    private ToggleButton? headerButton;
    private static readonly DependencyPropertyKey HeaderHeightPropertyKey = Property.RegisterReadOnly<Expander, double>(nameof(HeaderHeight));
    public static readonly DependencyProperty HeaderHeightProperty = HeaderHeightPropertyKey.DependencyProperty;

    static Expander() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Expander), new FrameworkPropertyMetadata(typeof(Expander)));

    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        private set => SetValue(HeaderHeightPropertyKey, Boxes.Box<double>(value));
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        headerButton = (ToggleButton)GetTemplateChild(PART_HeaderButton);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
        {
            switch (e.Key)
            {
                case Key.Return:
                case Key.Space:
                    e.Handled = true;
                    IsExpanded = !IsExpanded;
                    return;
                case Key.Left:
                    e.Handled = true;
                    IsExpanded = false;
                    return;
                case Key.Right:
                    e.Handled = true;
                    IsExpanded = true;
                    return;
            }
        }
        base.OnKeyDown(e);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (headerButton is not null) HeaderHeight = headerButton.ActualHeight;
    }
}
