#nullable enable
namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class ConditionExpression(
    string entityName, string attributeName, ConditionOperator conditionOperator, List<Object> values)
{
    public string AttributeName { get; set; } = attributeName;
    public bool CompareColumns { get; set; }
    public string EntityName { get; set; } = entityName;
    public ConditionOperator Operator { get; set; } = conditionOperator;
    public List<Object> Values { get; set; } = values;
}
#nullable restore