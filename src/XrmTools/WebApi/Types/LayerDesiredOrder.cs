#nullable enable
namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class LayerDesiredOrder
{
    public LayerDesiredOrderType Type { get; set; }
    public List<SolutionInfo> Solutions { get; set; }
}
#nullable restore