namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using XrmTools.Shell.Helpers;

public class Callout : Popup
{
    public static readonly DependencyProperty PreferredPlacementProperty = Property.Register<Callout, PlacementMode>(nameof(PreferredPlacement), PlacementMode.Bottom, new PropertyChangedCallback(PreferredPlacementChanged));

    static Callout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Callout), new FrameworkPropertyMetadata(typeof(Callout)));
    }

    public Callout()
    {
        Placement = PlacementMode.Custom;
        CustomPopupPlacementCallback = new CustomPopupPlacementCallback(PlacementCallback);
        Child = new CalloutSurface();
        BindingOperations.SetBinding(Child, ContentControl.ContentProperty, new Binding("Content")
        {
            Source = this
        });
        BindingOperations.SetBinding(Child, MinHeightProperty, new Binding("MinHeight")
        {
            Source = this
        });
        BindingOperations.SetBinding(Child, MinWidthProperty, new Binding("MinWidth")
        {
            Source = this
        });
    }

    public PlacementMode PreferredPlacement
    {
        get => (PlacementMode)GetValue(PreferredPlacementProperty);
        set => SetValue(PreferredPlacementProperty, Boxes.Box(value));
    }

    private static void PreferredPlacementChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
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

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        CalloutSurface surface = (CalloutSurface)Child;
        Rect popupBounds = new Rect(Child.PointToScreen(new Point()), Child.PointToScreen(new Point(Child.RenderSize.Width, Child.RenderSize.Height)));
        Rect targetBounds = new Rect(GetTargetScreenPoint(element => new Point()), GetTargetScreenPoint(element => new Point(element.RenderSize.Width, element.RenderSize.Height)));

        if (popupBounds.Top >= targetBounds.Bottom)
        {
            surface.PointerPlacement = PlacementMode.Top;
            SetHorizontalPointerAlignment();
        }
        else if (popupBounds.Bottom <= targetBounds.Top)
        {
            surface.PointerPlacement = PlacementMode.Bottom;
            SetHorizontalPointerAlignment();
        }
        else if (popupBounds.Left >= targetBounds.Right)
        {
            surface.PointerPlacement = PlacementMode.Left;
            SetVerticalPointerAlignment();
        }
        else if (popupBounds.Right <= targetBounds.Left)
        {
            surface.PointerPlacement = PlacementMode.Right;
            SetVerticalPointerAlignment();
        }

        void SetHorizontalPointerAlignment()
        {
            if (popupBounds.Left > targetBounds.Left)
            {
                surface.PointerAlignment = Dock.Left;
            }
            else if (popupBounds.Right < targetBounds.Right)
            {
                surface.PointerAlignment = Dock.Right;
            }
            else
            {
                surface.PointerAlignment = null;
            }
        }

        void SetVerticalPointerAlignment()
        {
            if (popupBounds.Top > targetBounds.Top)
            {
                surface.PointerAlignment = Dock.Top;
            }
            else if (popupBounds.Bottom < targetBounds.Bottom)
            {
                surface.PointerAlignment = Dock.Bottom;
            }
            else
            {
                surface.PointerAlignment = null;
            }
        }
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!(e.Property.Name == "Placement"))
            return;
        ValidatePlacement((PlacementMode)e.NewValue);
    }

    private Point GetTargetScreenPoint(Func<FrameworkElement, Point> getPoint)
    {
        switch (PreferredPlacement)
        {
            case PlacementMode.Bottom:
            case PlacementMode.Right:
            case PlacementMode.Left:
            case PlacementMode.Top:
                if (((DependencyObject)PlacementTarget ?? Parent) is FrameworkElement frameworkElement)
                    return frameworkElement.PointToScreen(getPoint(frameworkElement));
                throw new NotSupportedException(((DependencyObject)PlacementTarget ?? Parent)?.ToString());
            default:
                throw new NotSupportedException(PreferredPlacement.ToString());
        }
    }

    private CustomPopupPlacement[] PlacementCallback(Size popupSize, Size targetSize, Point offset)
    {
        double y1 = targetSize.Height - popupSize.Height;
        double x1 = (targetSize.Width - popupSize.Width) / 2.0;
        double y2 = (targetSize.Height - popupSize.Height) / 2.0;
        double x2 = -popupSize.Width;
        double x3 = targetSize.Width - popupSize.Width;
        double y3 = -popupSize.Height;
        Point point1 = new Point(x1, targetSize.Height);
        Point point2 = new Point(0.0, targetSize.Height);
        Point point3 = new Point(x3, targetSize.Height);
        Point point4 = new Point(x2, y1);
        Point point5 = new Point(x2, y2);
        Point point6 = new Point(x2, 0.0);
        Point point7 = new Point(targetSize.Width, y1);
        Point point8 = new Point(targetSize.Width, y2);
        Point point9 = new Point(targetSize.Width, 0.0);
        Point point10 = new Point(x1, y3);
        Point point11 = new Point(0.0, y3);
        Point point12 = new Point(x3, y3);
        return PreferredPlacement switch
        {
            PlacementMode.Bottom => [
                new(point1, PopupPrimaryAxis.None),
                new(point2, PopupPrimaryAxis.None),
                new(point3, PopupPrimaryAxis.None),
                new(point10, PopupPrimaryAxis.None),
                new(point11, PopupPrimaryAxis.None),
                new(point12, PopupPrimaryAxis.None)
                            ],
            PlacementMode.Right => [
                new CustomPopupPlacement(point8, PopupPrimaryAxis.None),
                new CustomPopupPlacement(point9, PopupPrimaryAxis.None),
                new CustomPopupPlacement(point7, PopupPrimaryAxis.None),
                new CustomPopupPlacement(point5, PopupPrimaryAxis.None),
                new CustomPopupPlacement(point6, PopupPrimaryAxis.None),
                new CustomPopupPlacement(point4, PopupPrimaryAxis.None)
                            ],
            PlacementMode.Left => [
                new(point5, PopupPrimaryAxis.None),
                new(point6, PopupPrimaryAxis.None),
                new(point4, PopupPrimaryAxis.None),
                new(point8, PopupPrimaryAxis.None),
                new(point9, PopupPrimaryAxis.None),
                new(point7, PopupPrimaryAxis.None)
                            ],
            PlacementMode.Top => [
                new(point10, PopupPrimaryAxis.None),
                new(point11, PopupPrimaryAxis.None),
                new(point12, PopupPrimaryAxis.None),
                new(point1, PopupPrimaryAxis.None),
                new(point2, PopupPrimaryAxis.None),
                new(point3, PopupPrimaryAxis.None)
                            ],
            _ => throw new NotSupportedException(PreferredPlacement.ToString()),
        };
    }

    private void ValidatePlacement(PlacementMode placement)
    {
        if (placement != PlacementMode.Custom)
            throw new NotSupportedException(placement.ToString());
    }
}