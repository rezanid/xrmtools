namespace XrmTools.WebApi.Types;

public class LocalizedLabel : MetadataBase
{
    public string Label { get; set; } = string.Empty;
    public int LanguageCode { get; set; }
    public bool? IsManaged { get; set; } = null;
}