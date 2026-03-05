namespace XrmTools.DataverseExplorer.Views;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

public sealed class TreeViewLevelToIndentPxConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TreeViewItem item)
            return 0.0;

        double indentSize = 16.0; // default
        if (parameter != null &&
            double.TryParse(parameter.ToString(), NumberStyles.Any,
                            CultureInfo.InvariantCulture, out var parsed))
        {
            indentSize = parsed;
        }

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

        return depth * indentSize;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}