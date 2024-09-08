namespace XrmGen.Xrm.Extensions;

using Microsoft.Xrm.Sdk.Query;

public static class LinkEntityExtensions
{
    public static LinkEntity LinkWith(this LinkEntity linkEntity, LinkEntity target)
    {
        linkEntity.LinkEntities.Add(target);
        return linkEntity;
    }
}
