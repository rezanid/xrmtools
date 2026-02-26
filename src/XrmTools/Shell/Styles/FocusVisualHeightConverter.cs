namespace XrmTools.Shell.Styles;

using Microsoft.VisualStudio.PlatformUI;
using XrmTools.Shell.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

internal class FocusVisualHeightConverter : ValueConverter<FrameworkElement, double>
{
    protected override double Convert(
        FrameworkElement element, object parameter, CultureInfo culture)
    {
        return Keyboard.FocusedElement switch
        {
            Expander expander => expander.HeaderHeight,
            TreeViewItem treeViewItem => treeViewItem.HeaderHeight,
            _ => double.NaN,
        };
    }
}