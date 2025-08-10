namespace XrmTools.WebApi.Types;
using System.Collections.Generic;

public sealed class EntitySetting
{
    public string? Name { get; set; }

    public Dictionary<string, object> Value { get; set; } = [];

    public EntitySetting[] ChildSettings { get; set; } = [];
}
