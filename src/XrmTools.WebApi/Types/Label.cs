namespace XrmTools.WebApi.Types;
public class Label
{
    public LocalizedLabel[] LocalizedLabels { get; set; } = [];
    public LocalizedLabel UserLocalizedLabel { get; set; } = new();
}
