namespace XrmTools.DataverseExplorer.Views;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

/// <summary>
/// Returns left indentation based on TreeViewItem depth: depth * IndentSize.
/// </summary>
public sealed class TreeViewLevelToIndentConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TreeViewItem item)
            return new Thickness(0);

        // parameter: indent size in pixels (default 16)
        var indentSize = 16.0;
        if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var p))
            indentSize = p;

        var depth = GetDepth(item);
        return new Thickness(depth * indentSize, 0, 0, 0);
    }

    static int GetDepth(DependencyObject item)
    {
        var depth = 0;
        DependencyObject current = item;

        while (true)
        {
            var parent = ItemsControl.ItemsControlFromItemContainer(current);
            if (parent is TreeView)
                break;

            if (parent is null)
                break;

            depth++;
            current = parent;
        }

        return depth;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
