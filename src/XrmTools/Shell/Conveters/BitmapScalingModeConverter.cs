namespace XrmTools.Shell.Converters;

using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Utilities;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

public class BitmapScalingModeConverter :
  MultiValueConverter<FrameworkElement, object, BitmapScalingMode>
{
    protected override BitmapScalingMode Convert(
      FrameworkElement element,
      object trigger,
      object parameter,
      CultureInfo culture)
    {
        //return VisualOptions.Instance.GetBitmapScalingMode((Visual)element);
        return GetBitmapScalingMode(element);
    }
    
    public static BitmapScalingMode GetBitmapScalingMode(Visual visual)
    {
        if (visual == null)
            return BitmapScalingMode.Unspecified;
        double dpiX = visual.GetDpiX();
        return dpiX < 96.0 ? BitmapScalingMode.LowQuality : (dpiX % 96.0 != 0.0 ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor);
    }
}