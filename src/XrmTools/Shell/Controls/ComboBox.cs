#nullable enable
namespace XrmTools.Shell.Controls;

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using XrmTools.Shell.Helpers;

/// <summary>Shell-themed combo with the same editable field and flyout as the VS control.</summary>
[TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class ComboBox : System.Windows.Controls.ComboBox
{
    public static readonly double ControlMinWidth = 76;
    public static readonly double PopupMinHeight = 76;
    public static readonly double PopupMaxHeight = 380;
    public static readonly DependencyProperty CornerRadiusProperty = Property.Register<ComboBox, CornerRadius>(nameof(CornerRadius));
    public static readonly DependencyProperty PlacementProperty = Property.Register<ComboBox, PlacementMode>(nameof(Placement), PlacementMode.Bottom);

    static ComboBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBox), new FrameworkPropertyMetadata(typeof(ComboBox)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Popup") is Popup popup)
            BindingOperations.SetBinding(popup, Popup.PlacementProperty, new Binding(nameof(Placement)) { Source = this });
    }

    protected override DependencyObject GetContainerForItemOverride() => new ComboBoxItem();
    protected override bool IsItemItsOwnContainerOverride(object item) => item is ComboBoxItem;
}
