namespace XrmTools.Shell.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XrmTools.Shell.Helpers;

public class CalloutSurface : ContentControl
{
    public static readonly double ControlMaxWidth = 660.0;
    public static readonly double ShadowSize = 12.0;
    public static readonly Thickness ShadowMargin = new Thickness(CalloutSurface.ShadowSize);
    public static readonly double HorizontalOffsetInvert = -CalloutSurface.ShadowSize;
    public static readonly double VerticalOffsetInvert = -CalloutSurface.ShadowSize;
    public static readonly DependencyProperty PointerAlignmentProperty = Property.Register<CalloutSurface, Dock?>(nameof(PointerAlignment));
    public static readonly DependencyProperty PointerPlacementProperty = Property.Register<CalloutSurface, PlacementMode?>(nameof(PointerPlacement), propertyChanged: new PropertyChangedCallback(CalloutSurface.PointerPlacementChanged));

    static CalloutSurface()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalloutSurface), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(CalloutSurface)));
    }

    public Dock? PointerAlignment
    {
        get => (Dock?)this.GetValue(CalloutSurface.PointerAlignmentProperty);
        set => this.SetValue(CalloutSurface.PointerAlignmentProperty, Boxes.Box<Dock?>(value));
    }

    public PlacementMode? PointerPlacement
    {
        get => (PlacementMode?)this.GetValue(CalloutSurface.PointerPlacementProperty);
        set => this.SetValue(CalloutSurface.PointerPlacementProperty, Boxes.Box<PlacementMode?>(value));
    }

    private static void PointerPlacementChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue != null)
        {
            if (e.NewValue is PlacementMode newValue)
            {
                switch (newValue)
                {
                    case PlacementMode.Bottom:
                        return;
                    case PlacementMode.Right:
                        return;
                    case PlacementMode.Left:
                    case PlacementMode.Top:
                        return;
                }
            }
            throw new NotSupportedException(e.NewValue.ToString());
        }
    }

    protected override Size MeasureOverride(Size constraint)
    {
        base.MeasureOverride(new Size());
        return base.MeasureOverride(constraint);
    }
}
