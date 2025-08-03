namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class LinkEntity
{
    public ColumnSet Columns { get; set; }
    public string EntityAlias { get; set; }
    public JoinOperator JoinOperator { get; set; }
    public FilterExpression LinkCriteria { get; set; }
    public List<LinkEntity> LinkEntities { get; set; } = [];
    public string LinkFromAttributeName { get; set; }
    public string LinkFromEntityName { get; set; }
    public string LinkToAttributeName { get; set; }
    public string LinkToEntityName { get; set; }
    public List<OrderExpression> Orders { get; set; } = [];

}
#nullable restore