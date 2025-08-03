namespace XrmTools.WebApi.Types;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum ComponentState
{
    Published = 0,
    Unpublished = 1,
    Deleted = 2,
    DeletedUnpublished = 3,
}
