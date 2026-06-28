#nullable enable
namespace XrmTools.Shell.Helpers;

using System.Windows;
using System.Windows.Media;

internal static class VisualTree
{
    public static void InvalidateMeasureToAncestor(
      FrameworkElement? descendant,
      FrameworkElement? ancestor)
    {
        if (descendant == ancestor)
            return;
        FrameworkElement reference = descendant;
        while (reference != null)
        {
            reference.InvalidateMeasure();
            reference = VisualTreeHelper.GetParent((DependencyObject)reference) as FrameworkElement;
            if (reference == ancestor)
                break;
        }
    }
}

