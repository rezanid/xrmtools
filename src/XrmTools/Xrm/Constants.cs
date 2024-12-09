using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace XrmTools.Xrm.CodeCompletion;

public static class Constants
{
    public static readonly string EnvironmentUrl = "https://aguflowd.crm4.dynamics.com";
    public static readonly string ApplicationId = "1950a258-227b-4e31-a9cf-717495945fc2";
    public static ImageSource Icon = BitmapFrame.Create(new Uri("pack://application:,,,/XrmGen;component/Xrm/Resources/npm.png", UriKind.RelativeOrAbsolute));
}
