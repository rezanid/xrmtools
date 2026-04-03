using System.Windows;
using System.Windows.Media;

#nullable enable
namespace XrmTools.Shell.Helpers;

internal static class VisualTree
{
    public static void InvalidateMeasureToAncestor(
      FrameworkElement? descendant,
      FrameworkElement? ancestor)
    {
        FrameworkElement? reference = descendant;
        while (reference != null)
        {
            reference.InvalidateMeasure();
            reference = VisualTreeHelper.GetParent(reference) as FrameworkElement;
            if (reference == ancestor)
                break;
        }
    }
}