namespace XrmTools.Shell.Helpers;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using XrmTools.Shell.Controls;
using XrmTools.Shell.Styles;

internal static class DataGridColumns
{
    private static Style? textBoxStyle;

    private static Style TextBoxStyle
    {
        get
        {
            if (textBoxStyle is not null) return textBoxStyle;
            var trigger = new Trigger
            {
                Property = Validation.HasErrorProperty,
                Value = Boxes.BoolFalse,
            };
            trigger.Setters.Add(new Setter(Control.BorderThicknessProperty, StrokeWidths.None));
            textBoxStyle = new Style(typeof(Controls.TextBox));
            textBoxStyle.Triggers.Add(trigger);
            return textBoxStyle;
        }
    }

    public static double CalculateMinWidth(this System.Windows.Controls.DataGridColumn column)
    {
        var width = Sizes.ControlMinWidth;
        if (column.CanUserReorder || column.CanUserResize || column.CanUserSort) width = 56.0;
        return Math.Max(width, column.MinWidth);
    }

    public static Controls.TextBlock CreateTextBlock(XrmTools.Shell.Controls.DataGridTextColumn column)
    {
        var element = new Controls.TextBlock { Margin = Spacings.HorizontalS };
        Bind(element, column.Binding, column.Header, column.IsReadOnly, System.Windows.Controls.TextBlock.TextProperty);
        return element;
    }

    public static Controls.TextBox CreateTextBox(DataGridBoundColumn column)
    {
        var element = new Controls.TextBox
        {
            CornerRadius = CornerRadii.None,
            Padding = new Thickness(Spacings.PrimitiveSNudge, Spacings.PrimitiveXS, Spacings.PrimitiveSNudge, Spacings.PrimitiveXS),
            Style = TextBoxStyle,
        };
        Bind(element, column.Binding, column.Header, column.IsReadOnly, System.Windows.Controls.TextBox.TextProperty);
        return element;
    }

    private static void Bind(FrameworkElement element, BindingBase binding, object header, bool isReadOnly, DependencyProperty property)
    {
        if (binding is not null)
        {
            element.SetBinding(property, binding);
            return;
        }

        if (header is string path)
        {
            element.SetBinding(property, new Binding(path) { Mode = isReadOnly ? BindingMode.OneWay : BindingMode.TwoWay });
            return;
        }

        if (header is null) throw new ArgumentNullException(nameof(header));
        throw new NotSupportedException(header.GetType().ToString());
    }
}
