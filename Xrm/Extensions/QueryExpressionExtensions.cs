namespace XrmGen.Xrm.Extensions;

using Microsoft.Xrm.Sdk.Query;

public static class QueryExpressionExtensions
{
    public static LinkEntity LinkWith(this QueryExpression query, LinkEntity target)
    {
        query.LinkEntities.Add(target);
        return target;
    }
}
