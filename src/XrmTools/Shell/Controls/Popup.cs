namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using XrmTools.Shell.Helpers;
using XrmTools.Shell.Styles;
using XrmTools.Shell.Converters;

[System.Windows.Markup.ContentProperty("Content")]
public class Popup : System.Windows.Controls.Primitives.Popup
{
    private Popup.MenuModeScope? menuModeScope;
    public static readonly DependencyProperty ContentProperty = Property.RegisterFull<Popup, object>(nameof(Content));
    public static readonly DependencyProperty EnableMenuModeProperty = Property.Register<Popup, bool>(nameof(EnableMenuMode));
    public static readonly DependencyProperty FocusOnOpenProperty = Property.Register<Popup, bool>(nameof(FocusOnOpen));
    private static readonly DependencyPropertyKey IsClosedPropertyKey = Property.RegisterReadOnly<Popup, bool>(nameof(IsClosed), true);
    public static readonly DependencyProperty IsClosedProperty = Popup.IsClosedPropertyKey.DependencyProperty;

    static Popup()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(Popup), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(Popup)));
    }

    public Popup()
    {
        this.Child = (UIElement)new FlyoutSurface();
        BindingOperations.SetBinding((DependencyObject)this.Child, ContentControl.ContentProperty, (BindingBase)new Binding(nameof(Content))
        {
            Source = (object)this
        });
        BindingOperations.SetBinding((DependencyObject)this.Child, FrameworkElement.MinHeightProperty, (BindingBase)new Binding("MinHeight")
        {
            Source = (object)this
        });
        BindingOperations.SetBinding((DependencyObject)this.Child, FrameworkElement.MinWidthProperty, (BindingBase)new Binding("MinWidth")
        {
            Source = (object)this
        });
    }

    public object Content
    {
        get => this.GetValue(Popup.ContentProperty);
        set => this.SetValue(Popup.ContentProperty, value);
    }

    public bool EnableMenuMode
    {
        get => (bool)this.GetValue(Popup.EnableMenuModeProperty);
        set => this.SetValue(Popup.EnableMenuModeProperty, Boxes.Box<bool>(value));
    }

    public bool FocusOnOpen
    {
        get => (bool)this.GetValue(Popup.FocusOnOpenProperty);
        set => this.SetValue(Popup.FocusOnOpenProperty, Boxes.Box<bool>(value));
    }

    public bool IsClosed
    {
        get => (bool)this.GetValue(Popup.IsClosedProperty);
        private set => this.SetValue(Popup.IsClosedPropertyKey, Boxes.Box<bool>(value));
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        this.IsClosed = true;
        if (this.menuModeScope == null)
            return;
        this.menuModeScope.Dispose();
        this.menuModeScope = (Popup.MenuModeScope)null;
        if (!this.IsKeyboardFocusWithin)
            return;
        this.PlacementTarget?.Focus();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            this.IsOpen = false;
            e.Handled = true;
            this.PlacementTarget?.Focus();
        }
        else
            base.OnKeyDown(e);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        this.IsClosed = false;
        if (this.EnableMenuMode && this.menuModeScope == null)
            this.menuModeScope = new Popup.MenuModeScope(this.PlacementTarget);
        RenderOptions.SetBitmapScalingMode((DependencyObject)this.Child, BitmapScalingModeConverter.GetBitmapScalingMode((Visual)this.Child));
        if (!this.FocusOnOpen)
            return;
        this.Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        this.HandleAutoClose(e.OriginalSource);
        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
    {
        this.HandleAutoClose(e.OriginalSource);
        base.OnPreviewMouseRightButtonDown(e);
    }

    private void HandleAutoClose(object originalSource)
    {
        if (this.StaysOpen && !InputManager.Current.IsInMenuMode || !(originalSource is PassThroughBorder))
            return;
        this.IsOpen = false;
    }

    private class MenuModeScope : IDisposable
    {
        private PresentationSource? source;

        public MenuModeScope(UIElement element)
        {
            this.source = PresentationSource.FromVisual((Visual)element);
            if (this.source == null)
                return;
            InputManager.Current.PushMenuMode(this.source);
        }

        public void Dispose()
        {
            if (this.source != null)
                InputManager.Current.PopMenuMode(this.source);
            this.source = (PresentationSource)null;
        }
    }
}