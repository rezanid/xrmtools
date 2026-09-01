namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using XrmTools.Shell.Helpers;

public class DropDownButton : Button
{
    public static readonly DependencyProperty DropDownProperty = Property.Register<DropDownButton, object>(nameof(DropDown), propertyChanged: DropDownChanged);
    public static readonly DependencyProperty PlacementProperty = Property.Register<DropDownButton, PlacementMode>(nameof(Placement), PlacementMode.Bottom, PlacementChanged);
    public static readonly DependencyProperty ShowDropDownGlyphProperty = Property.RegisterFull<DropDownButton, bool>(nameof(ShowDropDownGlyph));

    static DropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
    }

    public object DropDown
    {
        get => GetValue(DropDownProperty);
        set => SetValue(DropDownProperty, value);
    }

    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, Boxes.Box(value));
    }

    public bool ShowDropDownGlyph
    {
        get => (bool)GetValue(ShowDropDownGlyphProperty);
        set => SetValue(ShowDropDownGlyphProperty, Boxes.Box(value));
    }

    public void ShowDropDown() => OnClickCore(true);

    protected override void OnClick() => OnClickCore(true);

    protected virtual void OnClickCore(bool showDropDown)
    {
        if (!showDropDown)
        {
            base.OnClick();
            return;
        }

        switch (DropDown)
        {
            case XrmTools.Shell.Controls.ContextMenu contextMenu:
                contextMenu.MinWidth = ActualWidth;
                contextMenu.Placement = Placement;
                contextMenu.PlacementTarget = this;
                contextMenu.IsOpen = true;
                contextMenu.Closed += OnDropDownClosed;
                RaiseExpandCollapseAutomationEvent(false, true);
                break;
            case Popup popup:
                popup.MinWidth = ActualWidth;
                popup.Placement = Placement;
                popup.PlacementTarget = this;
                popup.StaysOpen = false;
                popup.IsOpen = true;
                popup.Closed += OnDropDownClosed;
                RaiseExpandCollapseAutomationEvent(false, true);
                break;
        }
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new DropDownButtonAutomationPeer(this);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Down && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
        {
            OnClickCore(true);
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private static void DropDownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not null and not XrmTools.Shell.Controls.ContextMenu and not Popup)
        {
            throw new NotSupportedException(e.NewValue.GetType().ToString());
        }
    }

    private static void PlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not PlacementMode.Bottom and not PlacementMode.Left and not PlacementMode.Right and not PlacementMode.Top)
        {
            throw new NotSupportedException(e.NewValue?.ToString());
        }
    }

    private void OnDropDownClosed(object sender, EventArgs e)
    {
        switch (sender)
        {
            case XrmTools.Shell.Controls.ContextMenu contextMenu:
                contextMenu.Closed -= OnDropDownClosed;
                break;
            case Popup popup:
                popup.Closed -= OnDropDownClosed;
                break;
        }

        RaiseExpandCollapseAutomationEvent(true, false);
    }

    private void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
    {
        if (UIElementAutomationPeer.FromElement(this) is DropDownButtonAutomationPeer peer)
        {
            peer.RaiseExpandCollapseAutomationEvent(oldValue, newValue);
        }
    }

    private sealed class DropDownButtonAutomationPeer(DropDownButton owner) : ButtonAutomationPeer(owner), IExpandCollapseProvider
    {
        public ExpandCollapseState ExpandCollapseState => ((DropDownButton)Owner).DropDown switch
        {
            XrmTools.Shell.Controls.ContextMenu menu => GetState(menu.IsOpen),
            Popup popup => GetState(popup.IsOpen),
            _ => ExpandCollapseState.LeafNode,
        };

        public void Collapse()
        {
            if (!IsEnabled()) throw new ElementNotEnabledException();
            switch (((DropDownButton)Owner).DropDown)
            {
                case XrmTools.Shell.Controls.ContextMenu menu: menu.IsOpen = false; break;
                case Popup popup: popup.IsOpen = false; break;
            }
        }

        public void Expand()
        {
            if (!IsEnabled()) throw new ElementNotEnabledException();
            ((DropDownButton)Owner).OnClickCore(true);
        }

        public override object GetPattern(PatternInterface patternInterface)
            => patternInterface == PatternInterface.ExpandCollapse ? this : base.GetPattern(patternInterface);

        protected override string GetClassNameCore() => nameof(DropDownButton);

        internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
            => RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, GetState(oldValue), GetState(newValue));

        private static ExpandCollapseState GetState(bool expanded)
            => expanded ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
    }
}
