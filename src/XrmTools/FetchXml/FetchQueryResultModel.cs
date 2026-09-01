namespace XrmTools.FetchXml;

using Newtonsoft.Json.Linq;

public class FetchQueryResultModel
{
    public JArray Records { get; set; } = [];
    public long ElapsedMs { get; set; }
    public string? Error { get; set; }
    public bool MoreRecords { get; set; }
}
