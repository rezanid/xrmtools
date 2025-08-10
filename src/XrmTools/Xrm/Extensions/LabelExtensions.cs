namespace XrmTools.Xrm.Extensions;
using XrmTools.WebApi.Types;
public static class LabelExtensions
{
    public static string GetLocalized(this Label label) => label?.UserLocalizedLabel?.Label;
}
