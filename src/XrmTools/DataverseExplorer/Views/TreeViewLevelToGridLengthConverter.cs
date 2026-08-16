namespace XrmTools.DataverseExplorer.Views;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

public sealed class TreeViewLevelToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TreeViewItem item)
            return new GridLength(0);

        var indentSize = 16.0;
        if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
            indentSize = p;

        int depth = 0;
        DependencyObject current = item;

        while (true)
        {
            var parent = ItemsControl.ItemsControlFromItemContainer(current);
            if (parent is TreeView || parent is null)
                break;

            depth++;
            current = parent;
        }

        return new GridLength(depth * indentSize, GridUnitType.Pixel);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}