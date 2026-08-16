namespace XrmTools.Shell.Converters;

using Microsoft.VisualStudio.PlatformUI;
using XrmTools.Shell.Conveters;
using XrmTools.Shell.Styles;

public static class Converters
{
    private static object isGreaterThanConverter;

    public static readonly object MultiplyConverter = new MultiplyingConverter();
    public static readonly object BitmapScalingModeConverter = new BitmapScalingModeConverter();
    public static readonly object ClipConverter = new Styles.ClipConverter();
    public static readonly object ColorOpacityConverter = new ColorOpacityConverter();
    public static object IsGreaterThanConverter
    {
        get
        {
            if (isGreaterThanConverter != null)
                return isGreaterThanConverter;
            isGreaterThanConverter = new NumericComparisonConverter
            {
                Operation = Operation.IsGreaterThan
            }; ;
            return isGreaterThanConverter;
        }
    }
}