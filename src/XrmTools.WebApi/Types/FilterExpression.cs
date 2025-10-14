#nullable enable
namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class FilterExpression(LogicalOperator logicalOperator)
{
    public List<ConditionExpression> Conditions { get; set; } = [];
    public string? FilterHint { get; set; }
    public LogicalOperator FilterOperator { get; set; } = logicalOperator;
    public List<FilterExpression> Filters { get; set; } = [];
    public bool IsQuickFindFilter { get; set; }
}
#nullable restore