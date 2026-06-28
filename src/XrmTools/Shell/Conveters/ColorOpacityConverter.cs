namespace XrmTools.Shell.Conveters;

using Microsoft.VisualStudio.PlatformUI;
using System.Globalization;
using System.Windows.Media;

public class ColorOpacityConverter : ValueConverter<object, double>
{
    protected override double Convert(object value, object parameter, CultureInfo culture)
    {
        double num;
        switch (value)
        {
            case Color color:
                num = (double)color.A / (double)byte.MaxValue;
                break;
            case SolidColorBrush solidColorBrush:
                num = (double)solidColorBrush.Color.A / (double)byte.MaxValue;
                break;
            default:
                num = 1.0;
                break;
        }
        return num;
    }
}
