namespace XrmTools.Shell.Styles;

using Microsoft.VisualStudio.PlatformUI;
using System.Globalization;
using System.Windows;

internal class IndentingConverter : MultiValueConverter<Thickness, int, Thickness>
{
    protected override Thickness Convert(
      Thickness margin,
      int level,
      object parameter,
      CultureInfo culture)
    {
        double right = 0.0;
        --level;
        if (level >= 0)
            right = margin.Right * (double)level + margin.Right * (double)level / 2.0;
        return new Thickness(0.0, 0.0, right, 0.0);
    }
}
