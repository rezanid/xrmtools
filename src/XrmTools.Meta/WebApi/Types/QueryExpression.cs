#nullable enable
namespace XrmTools.WebApi.Types;

using System.Collections.Generic;

public class QueryExpression(string entityName)
{
    public ColumnSet ColumnSet { get; set; }
    public FilterExpression Criteria { get; set; }
    public string DataSource { get; set; }
    public bool Distinct { get; set; }
    public string EntityName { get; set; } = entityName;
    public List<LinkEntity> LinkEntities { get; set; } = [];
    public bool NoLock { get; set; }
    public List<OrderExpression> Orders { get; set; } = [];
    public PagingInfo PageInfo { get; set; }
    public string QueryHints { get; set; }
    public QueryExpression SubQueryExpression { get; set; }
    public int TopCount { get; set; }
}
#nullable restore