#nullable enable
namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class ColumnSet(params string[] columns)
{
    public bool AllColumns { get; set; }

    public List<string> Columns { get; set; } = [.. columns];
}
#nullable restore