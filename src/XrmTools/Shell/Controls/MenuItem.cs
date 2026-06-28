namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XrmTools.Shell.Helpers;

[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class MenuItem : System.Windows.Controls.MenuItem
{
    private const string PART_Popup = "PART_Popup";
    public static readonly GridLength HeaderGestureSpaceGridLength = new GridLength(24.0);
    public static readonly DependencyProperty GroupNameProperty = Property.Register<MenuItem, string>(nameof(GroupName));
    public static readonly DependencyProperty IsSelectableProperty = Property.Register<MenuItem, bool>(nameof(IsSelectable), propertyChanged: new PropertyChangedCallback(MenuItem.IsSelectableChanged));
    public new static readonly DependencyProperty IsSelectedProperty = Property.Register<MenuItem, bool>(nameof(IsSelected), propertyChanged: new PropertyChangedCallback(MenuItem.IsSelectedChanged));

    static MenuItem()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuItem), (PropertyMetadata)new FrameworkPropertyMetadata((object)typeof(MenuItem)));
    }

    public string GroupName
    {
        get => (string)this.GetValue(MenuItem.GroupNameProperty);
        set => this.SetValue(MenuItem.GroupNameProperty, (object)value);
    }

    public bool IsSelectable
    {
        get => (bool)this.GetValue(MenuItem.IsSelectableProperty);
        set => this.SetValue(MenuItem.IsSelectableProperty, Boxes.Box<bool>(value));
    }

    private static void IsSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (!(d is MenuItem menuItem) || !(e.NewValue is bool newValue) || newValue)
            return;
        menuItem.IsSelected = false;
    }

    public new bool IsSelected
    {
        get => this.IsSelectable && (bool)this.GetValue(MenuItem.IsSelectedProperty);
        set => this.SetValue(MenuItem.IsSelectedProperty, Boxes.Box<bool>(value));
    }

    private static void IsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue == e.NewValue || !(d is MenuItem menuItem1))
            return;
        MenuItem.MenuItemAutomationPeer.RaiseIsSelectedAutomationEvent(menuItem1, (bool)e.OldValue, (bool)e.NewValue);
        if (e.NewValue is bool newValue && !newValue || !(menuItem1.Parent is FrameworkElement parent))
            return;
        foreach (FrameworkElement child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is MenuItem menuItem2 && menuItem2 != menuItem1 && menuItem2.IsSelectable && !(menuItem2.GroupName != menuItem1.GroupName))
                menuItem2.IsSelected = false;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return (DependencyObject)new MenuItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MenuItem || item is Separator;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (!(this.GetTemplateChild("PART_Popup") is Popup templateChild))
            return;
        templateChild.Placement = PlacementMode.Custom;
        templateChild.CustomPopupPlacementCallback = new CustomPopupPlacementCallback(this.PlacementCallback);
    }

    protected override void OnClick()
    {
        if (this.IsSelectable)
            this.IsSelected = true;
        base.OnClick();
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return (AutomationPeer)new MenuItem.MenuItemAutomationPeer((System.Windows.Controls.MenuItem)this);
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        object obj1 = null;
        object obj2 = null;
        Separator separator = element as Separator;
        if (separator != null)
        {
            obj1 = separator.GetValue(FrameworkElement.DefaultStyleKeyProperty);
            obj2 = separator.GetValue(FrameworkElement.StyleProperty);
        }
        base.PrepareContainerForItemOverride(element, item);
        if (separator == null)
            return;
        separator.SetValue(FrameworkElement.DefaultStyleKeyProperty, obj1);
        separator.SetValue(FrameworkElement.StyleProperty, obj2);
    }

    private CustomPopupPlacement[] PlacementCallback(Size popupSize, Size targetSize, Point offset)
    {
        Point point1 = new Point(0.0, targetSize.Height);
        Point point2 = new Point(2.0 * FlyoutSurface.ShadowSize - popupSize.Width, 0.0);
        Point point3 = new Point(targetSize.Width, 0.0);
        Point point4 = new Point(0.0, FlyoutSurface.ShadowSize - FlyoutSurface.Offset - popupSize.Height);
        switch (this.Role)
        {
            case MenuItemRole.TopLevelItem:
            case MenuItemRole.TopLevelHeader:
                return new CustomPopupPlacement[2]
                {
          new CustomPopupPlacement(point1, PopupPrimaryAxis.None),
          new CustomPopupPlacement(point4, PopupPrimaryAxis.None)
                };
            case MenuItemRole.SubmenuItem:
            case MenuItemRole.SubmenuHeader:
                return new CustomPopupPlacement[2]
                {
          new CustomPopupPlacement(point3, PopupPrimaryAxis.None),
          new CustomPopupPlacement(point2, PopupPrimaryAxis.None)
                };
            default:
                throw new NotSupportedException(this.Role.ToString());
        }
    }

    private class MenuItemAutomationPeer(System.Windows.Controls.MenuItem owner) :
      System.Windows.Automation.Peers.MenuItemAutomationPeer(owner),
      ISelectionItemProvider
    {
        public bool IsSelected => ((MenuItem)this.Owner).IsSelected;

        public IRawElementProviderSimple? SelectionContainer => (IRawElementProviderSimple)null;

        public void AddToSelection()
        {
            if (!this.IsSelected)
                throw new InvalidOperationException();
        }

        public override object? GetPattern(PatternInterface patternInterface)
        {
            if (((MenuItem)this.Owner).IsSelectable)
            {
                if (patternInterface == PatternInterface.SelectionItem)
                    return (object)this;
                if (patternInterface == PatternInterface.Toggle)
                    return (object)null;
            }
            return base.GetPattern(patternInterface);
        }

        public void RemoveFromSelection()
        {
            if (this.IsSelected)
                throw new InvalidOperationException();
        }

        public void Select()
        {
            if (!this.IsEnabled())
                throw new ElementNotEnabledException();
            if (!((MenuItem)this.Owner).IsSelectable)
                throw new InvalidOperationException();
            ((MenuItem)this.Owner).IsSelected = true;
        }

        internal static void RaiseIsSelectedAutomationEvent(
          MenuItem menuItem,
          bool oldValue,
          bool newValue)
        {
            UIElementAutomationPeer.FromElement((UIElement)menuItem)?.RaisePropertyChangedEvent(SelectionItemPattern.IsSelectedProperty, (object)oldValue, (object)newValue);
        }
    }
}
