#nullable enable
namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;

[System.Windows.Markup.ContentProperty("Content")]
public class Popup : System.Windows.Controls.Primitives.Popup
{
    private MenuModeScope? menuModeScope;
    public static readonly DependencyProperty ContentProperty = Property.RegisterFull<Popup, object>(nameof(Content));
    public static readonly DependencyProperty EnableMenuModeProperty = Property.Register<Popup, bool>(nameof(EnableMenuMode));
    public static readonly DependencyProperty FocusOnOpenProperty = Property.Register<Popup, bool>(nameof(FocusOnOpen));
    private static readonly DependencyPropertyKey IsClosedPropertyKey = Property.RegisterReadOnly<Popup, bool>(nameof(IsClosed), true);
    public static readonly DependencyProperty IsClosedProperty = IsClosedPropertyKey.DependencyProperty;

    static Popup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Popup), new FrameworkPropertyMetadata(typeof(Popup)));
    }

    public Popup()
    {
        Child = new FlyoutSurface();
        BindingOperations.SetBinding(Child, ContentControl.ContentProperty, new Binding(nameof(Content))
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

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool EnableMenuMode
    {
        get => (bool)GetValue(EnableMenuModeProperty);
        set => SetValue(EnableMenuModeProperty, Boxes.Box(value));
    }

    public bool FocusOnOpen
    {
        get => (bool)GetValue(FocusOnOpenProperty);
        set => SetValue(FocusOnOpenProperty, Boxes.Box(value));
    }

    public bool IsClosed
    {
        get => (bool)GetValue(IsClosedProperty);
        private set => SetValue(IsClosedPropertyKey, Boxes.Box(value));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        IsClosed = true;
        if (menuModeScope == null)
            return;
        menuModeScope.Dispose();
        menuModeScope = null;
        if (!IsKeyboardFocusWithin)
            return;
        PlacementTarget?.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsOpen = false;
            e.Handled = true;
            if (PlacementTarget == null)
                return;
            PlacementTarget.Focus();
        }
        else
            base.OnKeyDown(e);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        IsClosed = false;
        if (EnableMenuMode)
            menuModeScope = new MenuModeScope(PlacementTarget);
        RenderOptions.SetBitmapScalingMode(Child, VisualOptions.Instance.GetBitmapScalingMode(Child));
        if (!FocusOnOpen)
            return;
        Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        HandleAutoClose(e.OriginalSource);
        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        HandleAutoClose(e.OriginalSource);
        base.OnPreviewMouseRightButtonDown(e);
    }

    private void HandleAutoClose(object originalSource)
    {
        if (StaysOpen && !InputManager.Current.IsInMenuMode || originalSource is not PassThroughBorder)
            return;
        IsOpen = false;
    }

    private class MenuModeScope : IDisposable
    {
        private PresentationSource? source;

        public MenuModeScope(UIElement element)
        {
            source = PresentationSource.FromVisual(element);
            if (source == null)
                return;
            InputManager.Current.PushMenuMode(source);
        }

        public void Dispose()
        {
            if (source != null)
                InputManager.Current.PopMenuMode(source);
            source = null;
        }
    }
}
