namespace XrmTools.Xrm.Model;

using System.Collections.Generic;
using XrmTools.Meta.Helpers;

public class CodeGenReplacePrefixConfig
{
    private string prefixes = "";
    private List<string> prefixList = [];

    public string Prefixes 
    { 
        get => prefixes; 
        set
        {
            prefixes = value ?? "";
            prefixList = prefixes.SplitAndTrim(',') ?? [];
        }
    }

    public string? ReplaceWith { get; set; }

    public List<string> PrefixList { get => prefixList; }
}
