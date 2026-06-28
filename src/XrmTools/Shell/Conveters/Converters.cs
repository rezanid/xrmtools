namespace XrmTools.Shell.Converters;

using Microsoft.VisualStudio.PlatformUI;
using XrmTools.Shell.Conveters;

public static class Converters
{
    public static readonly object MultiplyConverter = new MultiplyingConverter();
    public static readonly object BitmapScalingModeConverter = new BitmapScalingModeConverter();
    public static readonly object ClipConverter = new Styles.ClipConverter();
    public static readonly object ColorOpacityConverter = new ColorOpacityConverter();
}