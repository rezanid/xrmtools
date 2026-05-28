namespace XrmTools.Xrm.Extensions;

using Microsoft.Xrm.Sdk.Query;

public static class QueryExpressionExtensions
{
    public static QueryExpression LinkWith(this QueryExpression query, LinkEntity target)
    {
        query.LinkEntities.Add(target);
        return query;
    }
    public static QueryExpression WithFilter(this QueryExpression query, FilterExpression filter)
    {
        query.Criteria.AddFilter(filter);
        return query;
    }
    public static QueryExpression WithCondition(this QueryExpression query, ConditionExpression condition)
    {
        query.Criteria.AddCondition(condition);
        return query;
    }
}
