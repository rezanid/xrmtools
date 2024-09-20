using Microsoft.Xrm.Sdk;

namespace XrmTools.Xrm.Extensions;

public static class LabelExtensions
{
    public static string GetLocalized(this Label label) => label?.UserLocalizedLabel?.Label;
}
